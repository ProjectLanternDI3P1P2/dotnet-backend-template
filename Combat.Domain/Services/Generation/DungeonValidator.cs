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
    public static IReadOnlyList<string> Validate(Dungeon dungeon)
    {
        List<string> violations = [];

        if (dungeon.TotalRoomCount != dungeon.Settings.RoomCount)
        {
            violations.Add(
                $"the dungeon has {dungeon.TotalRoomCount} rooms instead of {dungeon.Settings.RoomCount}"
            );
        }

        int bossCount = dungeon
            .Floors.SelectMany(floor => floor.Elements)
            .Count(element => element.Type == ElementType.Boss);
        if (bossCount != 1)
        {
            violations.Add($"the dungeon has {bossCount} final bosses instead of 1");
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

    private static void CheckRooms(DungeonFloor floor, int expectedRoomCount, List<string> violations)
    {
        if (floor.Rooms.Count != expectedRoomCount)
        {
            violations.Add($"{floor.Rooms.Count} rooms instead of {expectedRoomCount}");
        }

        if (floor.Rooms.Count(room => room.Type == RoomType.Start) != 1 || floor.Rooms[0].Type != RoomType.Start)
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

            if (floor.GetCell(room.Center) is not (CellType.Floor or CellType.StairsDown or CellType.StairsUp))
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

    private static void CheckExit(DungeonFloor floor, List<string> violations)
    {
        int bossRooms = floor.Rooms.Count(room => room.Type == RoomType.Boss);
        int stairsRooms = floor.Rooms.Count(room => room.Type == RoomType.Stairs);
        int stairsDown = CountCells(floor, CellType.StairsDown);
        int stairsUp = CountCells(floor, CellType.StairsUp);

        if (floor.IsFinalFloor)
        {
            Room? bossRoom = floor.Rooms.FirstOrDefault(room => room.Type == RoomType.Boss);
            DungeonElement? boss = floor.Elements.FirstOrDefault(element => element.Type == ElementType.Boss);

            if (bossRooms != 1 || stairsRooms != 0 || stairsDown != 0)
            {
                violations.Add("the final floor needs one boss room and no stairs down");
            }
            else if (boss is null || boss.RoomId != bossRoom!.Id)
            {
                violations.Add("the final boss is not in the boss room");
            }
        }
        else if (bossRooms != 0 || stairsRooms != 1 || stairsDown != 1)
        {
            violations.Add("a non-final floor needs one stairs room with one stairs down and no boss");
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

        bool[] visited = new bool[floor.Width * floor.Height];
        Queue<Position> queue = new();
        queue.Enqueue(floor.Entrance);
        visited[floor.Entrance.Y * floor.Width + floor.Entrance.X] = true;
        int reached = 0;

        while (queue.Count > 0)
        {
            Position current = queue.Dequeue();
            reached++;

            foreach (Direction direction in Enum.GetValues<Direction>())
            {
                Position next = current.Step(direction);
                if (floor.IsWalkable(next) && !visited[next.Y * floor.Width + next.X])
                {
                    visited[next.Y * floor.Width + next.X] = true;
                    queue.Enqueue(next);
                }
            }
        }

        int walkable = 0;
        for (int y = 0; y < floor.Height; y++)
        {
            for (int x = 0; x < floor.Width; x++)
            {
                walkable += floor.GetCell(new Position(x, y)).IsWalkable() ? 1 : 0;
            }
        }

        if (reached != walkable)
        {
            violations.Add($"{walkable - reached} walkable tiles cannot be reached from the entrance");
        }

        foreach (Room room in floor.Rooms.Where(room => !visited[room.Center.Y * floor.Width + room.Center.X]))
        {
            violations.Add($"room {room.Id} cannot be reached");
        }
    }

    private static void CheckElements(DungeonFloor floor, List<string> violations)
    {
        if (floor.Elements.Select((element, index) => element.Id != index).Any(mismatch => mismatch))
        {
            violations.Add("element ids must be 0..n-1 in order");
        }

        if (floor.Elements.Select(element => element.Position).Distinct().Count() != floor.Elements.Count)
        {
            violations.Add("two elements share a tile");
        }

        foreach (DungeonElement element in floor.Elements)
        {
            bool onFloor = floor.Contains(element.Position) && floor.GetCell(element.Position) == CellType.Floor;
            if (!onFloor || floor.GetRoomId(element.Position) != element.RoomId)
            {
                violations.Add($"element {element.Id} is not on a floor tile of room {element.RoomId}");
            }
        }
    }

    private static int CountCells(DungeonFloor floor, CellType cellType)
    {
        int count = 0;
        for (int y = 0; y < floor.Height; y++)
        {
            for (int x = 0; x < floor.Width; x++)
            {
                count += floor.GetCell(new Position(x, y)) == cellType ? 1 : 0;
            }
        }

        return count;
    }
}
