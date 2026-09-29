using Combat.Domain.Entities;
using Combat.Domain.Enums;
using Combat.Domain.ValueObjects;

namespace Combat.Domain.Services.Generation;

/// <summary>
/// Checks the business rules of a generated dungeon (US-DUNGEON-01) and returns every
/// violation found. An empty list means the dungeon is valid.
/// </summary>
public static class DungeonValidator
{
    public static IReadOnlyList<string> Validate(GeneratedDungeon dungeon)
    {
        List<string> violations = [];

        if (dungeon.RoomCount != dungeon.Settings.RoomCount)
        {
            violations.Add(
                $"the dungeon has {dungeon.RoomCount} rooms instead of {dungeon.Settings.RoomCount}"
            );
        }

        foreach (DungeonFloor floor in dungeon.Floors)
        {
            violations.AddRange(
                ValidateFloor(floor, dungeon.Settings.RoomCountForFloor(floor.Index))
                    .Select(violation => $"floor {floor.Index}: {violation}")
            );
        }

        return violations;
    }

    public static IReadOnlyList<string> ValidateFloor(DungeonFloor floor, int expectedRoomCount)
    {
        List<string> violations = [];

        CheckRooms(floor, expectedRoomCount, violations);
        CheckExit(floor, violations);
        CheckTraversability(floor, violations);
        CheckElements(floor, violations);

        return violations;
    }

    private static void CheckRooms(
        DungeonFloor floor,
        int expectedRoomCount,
        List<string> violations
    )
    {
        int roomCount = floor.Rooms.Count(room => room.Type != RoomType.Stairs);
        if (roomCount != expectedRoomCount)
        {
            violations.Add($"{roomCount} rooms instead of {expectedRoomCount}");
        }

        if (
            floor.Rooms.Count(room => room.Type == RoomType.Start) != 1
            || floor.Rooms[0].Type != RoomType.Start
        )
        {
            violations.Add("room 0 must be the only start room");
        }

        if (floor.Rooms.Select((room, index) => room.Id != index).Any(mismatch => mismatch))
        {
            violations.Add("room ids must be 0..n-1 in order");
        }

        foreach (Room room in floor.Rooms)
        {
            foreach (int connectedId in room.ConnectedRoomIds)
            {
                if (!floor.Rooms[connectedId].ConnectedRoomIds.Contains(room.Id))
                {
                    violations.Add($"connection {room.Id}-{connectedId} is one-way");
                }
            }

            if (!floor.GetCell(room.Center).IsWalkable())
            {
                violations.Add($"the centre of room {room.Id} is blocked");
            }
        }

        if (floor.Rooms.Any(room => room.Depth < 0))
        {
            violations.Add("a room is not connected to the start room");
        }

        Room start = floor.Rooms[0];
        if (!start.Interior.Contains(floor.Entrance))
        {
            violations.Add("the entrance is not in the start room");
        }
    }

    /// <summary>
    /// One boss per floor, in the boss room. On every floor but the last, the stairs room lies
    /// behind the boss room, and the only way in is the gate: the stairs cannot be reached
    /// without crossing it.
    /// </summary>
    private static void CheckExit(DungeonFloor floor, List<string> violations)
    {
        List<Room> bossRooms = floor.Rooms.Where(room => room.Type == RoomType.Boss).ToList();
        List<Room> stairsRooms = floor.Rooms.Where(room => room.Type == RoomType.Stairs).ToList();
        List<DungeonElement> bosses = floor
            .Elements.Where(element => element.Type == ElementType.Boss)
            .ToList();
        List<Position> stairsDown = PositionsOf(floor, CellType.StairsDown);
        List<Position> gates = PositionsOf(floor, CellType.Gate);
        int stairsUp = PositionsOf(floor, CellType.StairsUp).Count;

        if (bossRooms.Count != 1 || bosses.Count != 1 || bosses[0].RoomId != bossRooms[0].Id)
        {
            violations.Add("a floor needs one boss room holding its one boss");
        }

        if (floor.IsFinalFloor)
        {
            if (stairsRooms.Count != 0 || stairsDown.Count != 0 || gates.Count != 0)
            {
                violations.Add("the final floor has no stairs down and no gate");
            }
        }
        else if (stairsRooms.Count != 1 || gates.Count != 1 || stairsDown.Count == 0)
        {
            violations.Add("a floor needs one stairs room, stairs down and one gate");
        }
        else
        {
            Room stairsRoom = stairsRooms[0];
            bool connectedToBossOnly =
                bossRooms.Count == 1
                && stairsRoom.ConnectedRoomIds.SequenceEqual([bossRooms[0].Id]);
            if (!connectedToBossOnly)
            {
                violations.Add("the stairs room must be reached from the boss room only");
            }

            if (stairsDown.Any(stairs => floor.GetRoomId(stairs) != stairsRoom.Id))
            {
                violations.Add("stairs down lie outside the stairs room");
            }

            if (Reach(floor, gateIsOpen: false).Intersect(stairsDown).Any())
            {
                violations.Add("the stairs down can be reached without crossing the gate");
            }
        }

        bool expectsStairsUp = floor.Index > 0;
        if (stairsUp != (expectsStairsUp ? 1 : 0))
        {
            violations.Add($"{stairsUp} stairs up found");
        }
        else if (expectsStairsUp && floor.GetCell(floor.Entrance) != CellType.StairsUp)
        {
            violations.Add("the stairs up are not at the entrance");
        }
    }

    /// <summary>Every walkable tile, hence every room, must be reachable from the entrance.</summary>
    private static void CheckTraversability(DungeonFloor floor, List<string> violations)
    {
        if (!floor.IsWalkable(floor.Entrance))
        {
            violations.Add("the entrance is not walkable");
            return;
        }

        HashSet<Position> reached = Reach(floor, gateIsOpen: true);

        int walkable = 0;
        for (int y = 0; y < floor.Height; y++)
        {
            for (int x = 0; x < floor.Width; x++)
            {
                walkable += floor.GetCell(new Position(x, y)).IsWalkable() ? 1 : 0;
            }
        }

        if (reached.Count != walkable)
        {
            violations.Add($"{walkable - reached.Count} walkable tiles cannot be reached from the entrance");
        }

        foreach (Room room in floor.Rooms.Where(room => !reached.Contains(room.Center)))
        {
            violations.Add($"room {room.Id} cannot be reached");
        }
    }

    /// <summary>The walkable tiles reachable from the entrance, through the gate or not.</summary>
    private static HashSet<Position> Reach(DungeonFloor floor, bool gateIsOpen)
    {
        HashSet<Position> reached = [floor.Entrance];
        Queue<Position> queue = new();
        queue.Enqueue(floor.Entrance);

        while (queue.Count > 0)
        {
            Position current = queue.Dequeue();
            foreach (Direction direction in Enum.GetValues<Direction>())
            {
                Position next = current.Step(direction);
                bool passable =
                    floor.IsWalkable(next) && (gateIsOpen || floor.GetCell(next) != CellType.Gate);
                if (passable && reached.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        return reached;
    }

    private static void CheckElements(DungeonFloor floor, List<string> violations)
    {
        if (
            floor.Elements.Select((element, index) => element.Id != index).Any(mismatch => mismatch)
        )
        {
            violations.Add("element ids must be 0..n-1 in order");
        }

        if (
            floor.Elements.Select(element => element.Position).Distinct().Count()
            != floor.Elements.Count
        )
        {
            violations.Add("two elements share a tile");
        }

        foreach (DungeonElement element in floor.Elements)
        {
            bool onFloor =
                floor.Contains(element.Position)
                && floor.GetCell(element.Position) == CellType.Floor;
            if (!onFloor || floor.GetRoomId(element.Position) != element.RoomId)
            {
                violations.Add(
                    $"element {element.Id} is not on a floor tile of room {element.RoomId}"
                );
            }
        }
    }

    private static List<Position> PositionsOf(DungeonFloor floor, CellType cellType)
    {
        List<Position> positions = [];
        for (int y = 0; y < floor.Height; y++)
        {
            for (int x = 0; x < floor.Width; x++)
            {
                if (floor.GetCell(new Position(x, y)) == cellType)
                {
                    positions.Add(new Position(x, y));
                }
            }
        }

        return positions;
    }
}
