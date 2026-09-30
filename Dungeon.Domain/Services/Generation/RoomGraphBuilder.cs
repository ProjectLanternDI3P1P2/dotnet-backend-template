using Dungeon.Domain.Enums;
using Dungeon.Domain.Services.Randomness;
using Dungeon.Domain.ValueObjects;

namespace Dungeon.Domain.Services.Generation;

/// <summary>
/// Stage 1: places the rooms on a coarse grid and decides how they connect and what they are.
/// <para>
/// Rooms grow one at a time from the start room, always attached to an existing room, so the
/// floor is connected by construction and holds exactly the requested number of rooms: there
/// is no retry loop that could fail. A few extra doors are then added between neighbouring
/// rooms to create loops, which cuts down on backtracking.
/// </para>
/// <para>
/// The boss waits at the end of the longest branch. On every floor but the last, the stairs
/// room is then added just north of the boss room, connected to it alone: the way down goes
/// through the boss, and through the gate in the north wall of its room.
/// </para>
/// </summary>
internal static class RoomGraphBuilder
{
    /// <summary>13 x 13 = 169 cells: plenty of room for 80 rooms, the per-floor maximum.</summary>
    public const int GridSize = 13;

    private const int CompactGrowthChancePercent = 40;
    private const int LoopChancePercent = 30;
    private const int TreasureRoomRatio = 10;
    private const int EmptyRoomPercent = 15;
    private const int NoRoom = -1;

    private static readonly Position[] NeighbourOffsets =
    [
        new(0, -1),
        new(1, 0),
        new(0, 1),
        new(-1, 0),
    ];

    // East and south only, so that each pair of neighbouring rooms is considered once.
    private static readonly Position[] ForwardOffsets = [new(1, 0), new(0, 1)];

    public static List<RoomDraft> Build(
        DeterministicRandom random,
        int roomCount,
        bool isFinalFloor
    )
    {
        int[] roomIdByCell = new int[GridSize * GridSize];
        Array.Fill(roomIdByCell, NoRoom);
        List<RoomDraft> rooms = new(roomCount);

        AddRoom(rooms, roomIdByCell, new Position(GridSize / 2, GridSize / 2));

        while (rooms.Count < roomCount)
        {
            GrowOneRoom(random, rooms, roomIdByCell);
        }

        AddLoops(random, rooms, roomIdByCell);
        ComputeDepths(rooms);

        // Chosen after the loops, so that no shortcut can bring the boss closer to the start.
        RoomDraft boss = isFinalFloor ? FindExitRoom(rooms) : FindGuardedExit(rooms, roomIdByCell);
        boss.Type = RoomType.Boss;

        if (!isFinalFloor)
        {
            if (!IsFreeCell(roomIdByCell, North(boss.GridCell)))
            {
                // The boss room only has space to the south: mirror the floor top to bottom.
                FlipVertically(rooms, roomIdByCell);
            }

            // Added after the loops: the boss room is its only neighbour.
            RoomDraft stairs = AddRoom(rooms, roomIdByCell, North(boss.GridCell));
            Connect(boss, stairs);
            stairs.Type = RoomType.Stairs;
            stairs.Depth = boss.Depth + 1;
        }

        AssignRoomTypes(random, rooms);

        return rooms;
    }

    private static void GrowOneRoom(
        DeterministicRandom random,
        List<RoomDraft> rooms,
        int[] roomIdByCell
    )
    {
        List<Position> frontier = [];
        List<Position> preferred = [];

        // Row-major scan: the candidate order never depends on hashing or allocation.
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                Position cell = new(x, y);
                if (RoomAt(roomIdByCell, cell) != NoRoom)
                {
                    continue;
                }

                int occupiedNeighbours = OccupiedNeighbours(roomIdByCell, cell).Count;
                if (occupiedNeighbours == 0)
                {
                    continue;
                }

                frontier.Add(cell);
                if (occupiedNeighbours == 1)
                {
                    preferred.Add(cell);
                }
            }
        }

        // Cells touching a single room grow branches and dead ends instead of a compact blob.
        // Now and then any frontier cell is accepted: rooms then end up side by side, which is
        // what gives AddLoops somewhere to put an extra door.
        bool branch = preferred.Count > 0 && !random.Chance(CompactGrowthChancePercent);
        Position chosen = random.Pick(branch ? preferred : frontier);
        int parentId = random.Pick(OccupiedNeighbours(roomIdByCell, chosen));

        RoomDraft room = AddRoom(rooms, roomIdByCell, chosen);
        Connect(rooms[parentId], room);
    }

    /// <summary>
    /// The farthest dead end from the start: the final boss waits at the end of the longest
    /// branch, behind a single door. Should the loops have left no dead end at all,
    /// the farthest room is used.
    /// </summary>
    private static RoomDraft FindExitRoom(List<RoomDraft> rooms)
    {
        IEnumerable<RoomDraft> candidates = rooms.Where(room => room.Id != 0);
        if (candidates.Any(room => room.IsDeadEnd))
        {
            candidates = candidates.Where(room => room.IsDeadEnd);
        }

        return candidates.OrderByDescending(room => room.Depth).ThenBy(room => room.Id).First();
    }

    /// <summary>
    /// Like <see cref="FindExitRoom"/>, among the rooms with a free cell to the north or to
    /// the south, where the stairs room can go. There always is one: the northernmost room
    /// other than the start has a free cell above it, unless the start room is alone at the
    /// top, in which case the southernmost other room has a free cell below it. (Ten rooms
    /// grown from the centre of a 13 x 13 grid never reach its edges on both sides.)
    /// </summary>
    private static RoomDraft FindGuardedExit(List<RoomDraft> rooms, int[] roomIdByCell)
    {
        bool HasRoomForStairs(RoomDraft room) =>
            IsFreeCell(roomIdByCell, North(room.GridCell))
            || IsFreeCell(roomIdByCell, South(room.GridCell));

        return rooms
            .Where(room => room.Id != 0 && HasRoomForStairs(room))
            .OrderByDescending(room => room.IsDeadEnd)
            .ThenByDescending(room => room.Depth)
            .ThenBy(room => room.Id)
            .First();
    }

    private static void FlipVertically(List<RoomDraft> rooms, int[] roomIdByCell)
    {
        Array.Fill(roomIdByCell, NoRoom);
        foreach (RoomDraft room in rooms)
        {
            room.GridCell = new Position(room.GridCell.X, GridSize - 1 - room.GridCell.Y);
            roomIdByCell[room.GridCell.Y * GridSize + room.GridCell.X] = room.Id;
        }
    }

    /// <summary>Inside the grid and free: a room can be added there.</summary>
    private static bool IsFreeCell(int[] roomIdByCell, Position cell)
    {
        bool inside = cell.X >= 0 && cell.Y >= 0 && cell.X < GridSize && cell.Y < GridSize;
        return inside && roomIdByCell[cell.Y * GridSize + cell.X] == NoRoom;
    }

    private static Position North(Position cell) => new(cell.X, cell.Y - 1);

    private static Position South(Position cell) => new(cell.X, cell.Y + 1);

    private static void AddLoops(
        DeterministicRandom random,
        List<RoomDraft> rooms,
        int[] roomIdByCell
    )
    {
        foreach (RoomDraft room in rooms)
        {
            foreach (Position offset in ForwardOffsets)
            {
                Position neighbourCell = new(
                    room.GridCell.X + offset.X,
                    room.GridCell.Y + offset.Y
                );
                int neighbourId = RoomAt(roomIdByCell, neighbourCell);

                bool canConnect = neighbourId != NoRoom && !room.Connections.Contains(neighbourId);
                if (canConnect && random.Chance(LoopChancePercent))
                {
                    Connect(room, rooms[neighbourId]);
                }
            }
        }
    }

    private static void AssignRoomTypes(DeterministicRandom random, List<RoomDraft> rooms)
    {
        rooms[0].Type = RoomType.Start;

        List<RoomDraft> deadEnds = [];
        List<RoomDraft> others = [];
        foreach (
            RoomDraft room in rooms.Where(room =>
                room.Id != 0 && room.Type is not (RoomType.Boss or RoomType.Stairs)
            )
        )
        {
            (room.IsDeadEnd ? deadEnds : others).Add(room);
        }

        random.Shuffle(deadEnds);
        random.Shuffle(others);

        // Fixed quotas rather than a probability per room: every dungeon gets the same mix,
        // only the placement changes. Treasure rewards exploring a dead end first.
        int countedRooms = rooms.Count(room => room.Type != RoomType.Stairs);
        int treasureCount = Math.Max(1, countedRooms / TreasureRoomRatio);
        int emptyCount = countedRooms * EmptyRoomPercent / 100;

        List<RoomDraft> treasureCandidates = [.. deadEnds, .. others];
        foreach (RoomDraft room in treasureCandidates.Take(treasureCount))
        {
            room.Type = RoomType.Treasure;
        }

        List<RoomDraft> remaining = treasureCandidates.Skip(treasureCount).ToList();
        random.Shuffle(remaining);
        foreach (RoomDraft room in remaining.Take(emptyCount))
        {
            room.Type = RoomType.Empty;
        }
    }

    private static void ComputeDepths(List<RoomDraft> rooms)
    {
        int[] depths = new int[rooms.Count];
        Array.Fill(depths, -1);
        depths[0] = 0;

        Queue<int> queue = new();
        queue.Enqueue(0);
        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            foreach (int next in rooms[current].Connections.Where(next => depths[next] < 0))
            {
                depths[next] = depths[current] + 1;
                queue.Enqueue(next);
            }
        }

        foreach (RoomDraft room in rooms)
        {
            room.Depth = depths[room.Id];
        }
    }

    private static RoomDraft AddRoom(List<RoomDraft> rooms, int[] roomIdByCell, Position cell)
    {
        RoomDraft room = new(rooms.Count, cell);
        rooms.Add(room);
        roomIdByCell[cell.Y * GridSize + cell.X] = room.Id;
        return room;
    }

    private static void Connect(RoomDraft first, RoomDraft second)
    {
        first.Connections.Add(second.Id);
        second.Connections.Add(first.Id);
    }

    private static List<int> OccupiedNeighbours(int[] roomIdByCell, Position cell)
    {
        List<int> neighbours = [];
        foreach (Position offset in NeighbourOffsets)
        {
            int roomId = RoomAt(roomIdByCell, new Position(cell.X + offset.X, cell.Y + offset.Y));
            if (roomId != NoRoom)
            {
                neighbours.Add(roomId);
            }
        }

        return neighbours;
    }

    private static int RoomAt(int[] roomIdByCell, Position cell)
    {
        bool inside = cell.X >= 0 && cell.Y >= 0 && cell.X < GridSize && cell.Y < GridSize;
        return inside ? roomIdByCell[cell.Y * GridSize + cell.X] : NoRoom;
    }
}
