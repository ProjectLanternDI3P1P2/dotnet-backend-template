using Combat.Domain.Entities;
using Combat.Domain.Enums;
using Combat.Domain.Services.Randomness;
using Combat.Domain.ValueObjects;

namespace Combat.Domain.Services.Generation;

/// <summary>
/// Stages 2 to 5: turns the room graph into tiles, arranges and furnishes the rooms, then
/// places the elements.
/// <para>
/// Each grid cell is a 19 x 15 tile block. A room's interior always covers the block's
/// centre row and centre column with a tile to spare, so the corridor between two
/// neighbouring rooms is a straight line along that row or column: it can never miss a door
/// or cross another room. The margins leave room for the wall faces drawn above and below.
/// </para>
/// </summary>
internal static class FloorBuilder
{
    public const int CellWidth = 19;
    public const int CellHeight = 15;

    private const int CenterX = CellWidth / 2;
    private const int CenterY = CellHeight / 2;

    // Where walls may stand inside a block: a free column on each side, two free rows above
    // (for the top of the wall face) and three below (for the outer face of the south wall).
    private const int FirstWallColumn = 1;
    private const int LastWallColumn = CellWidth - 2;
    private const int FirstWallRow = 2;
    private const int LastWallRow = CellHeight - 4;

    private const int SmallRoomPercent = 30;
    private const int MediumRoomPercent = 45;

    private const int MaximumEnemiesPerRoom = 4;
    private const int ExtraEnemyChancePercent = 35;
    private const int CombatLootChancePercent = 25;
    private const int TrapChancePercent = 35;

    public static DungeonFloor Build(
        Seed seed,
        int floorIndex,
        bool isFinalFloor,
        List<RoomDraft> rooms
    )
    {
        int minGridX = rooms.Min(room => room.GridCell.X);
        int minGridY = rooms.Min(room => room.GridCell.Y);
        int width = (rooms.Max(room => room.GridCell.X) - minGridX + 1) * CellWidth;
        int height = (rooms.Max(room => room.GridCell.Y) - minGridY + 1) * CellHeight;

        ShapeRooms(
            DeterministicRandom.ForStream(seed, RandomStream.RoomShapes, floorIndex),
            rooms,
            minGridX,
            minGridY
        );

        TileGrid grid = new(width, height);
        CarveRooms(grid, rooms);
        CarveCorridors(grid, rooms);

        DeterministicRandom layouts = DeterministicRandom.ForStream(
            seed,
            RandomStream.RoomLayouts,
            floorIndex
        );
        RoomLayouts.Choose(layouts, rooms);
        RoomLayouts.CarveStructures(layouts, grid, rooms);
        RaiseWalls(grid);
        RoomLayouts.Furnish(layouts, grid, rooms);

        PlaceStairs(grid, rooms, floorIndex);
        List<DungeonElement> elements = PlaceElements(
            DeterministicRandom.ForStream(seed, RandomStream.Content, floorIndex),
            grid,
            rooms
        );

        List<Room> frozenRooms = rooms
            .Select(room => new Room(
                room.Id,
                room.Type,
                new Position(room.GridCell.X - minGridX, room.GridCell.Y - minGridY),
                room.Interior,
                room.Center,
                room.Depth,
                room.Connections.Order().ToArray()
            ))
            .ToList();

        return new DungeonFloor(
            floorIndex,
            isFinalFloor,
            width,
            height,
            grid.Cells,
            frozenRooms,
            elements,
            rooms[0].Center
        );
    }

    private static void ShapeRooms(
        DeterministicRandom random,
        List<RoomDraft> rooms,
        int minGridX,
        int minGridY
    )
    {
        foreach (RoomDraft room in rooms)
        {
            (int width, int height) = room.Type switch
            {
                RoomType.Start => (9, 5),
                RoomType.Stairs => (9, 7),
                RoomType.Boss => (15, 8),
                RoomType.Treasure => (random.NextInt(7, 10), random.NextInt(5, 8)),
                _ => RandomSize(random),
            };

            // The rooms whose centre holds something keep two free tiles around it.
            int margin = room.Type is RoomType.Combat or RoomType.Empty ? 1 : 2;

            int localX = Place(
                random,
                Math.Max(FirstWallColumn + 1, CenterX - width + 1 + margin),
                Math.Min(CenterX - margin, LastWallColumn - width),
                HasNeighbour(room, rooms, -1, 0),
                HasNeighbour(room, rooms, 1, 0)
            );
            int localY = Place(
                random,
                Math.Max(FirstWallRow + 1, CenterY - height + 1 + margin),
                Math.Min(CenterY - margin, LastWallRow - height),
                HasNeighbour(room, rooms, 0, -1),
                HasNeighbour(room, rooms, 0, 1)
            );

            int originX = (room.GridCell.X - minGridX) * CellWidth;
            int originY = (room.GridCell.Y - minGridY) * CellHeight;

            room.Interior = new RoomBounds(originX + localX, originY + localY, width, height);
            room.Center = new Position(originX + CenterX, originY + CenterY);
        }
    }

    /// <summary>
    /// A room with doors on one side only is pushed towards that side: its corridors get
    /// shorter, and the rooms of a floor feel closer together.
    /// </summary>
    private static int Place(
        DeterministicRandom random,
        int minimum,
        int maximum,
        bool towardsMinimum,
        bool towardsMaximum
    )
    {
        return (towardsMinimum, towardsMaximum) switch
        {
            (true, false) => minimum,
            (false, true) => maximum,
            _ => random.NextInt(minimum, maximum + 1),
        };
    }

    private static bool HasNeighbour(RoomDraft room, List<RoomDraft> rooms, int dx, int dy)
    {
        return room.Connections.Any(id =>
            rooms[id].GridCell.X - room.GridCell.X == dx
            && rooms[id].GridCell.Y - room.GridCell.Y == dy
        );
    }

    /// <summary>Small closets, medium rooms and large halls, in fixed proportions.</summary>
    private static (int Width, int Height) RandomSize(DeterministicRandom random)
    {
        int roll = random.NextInt(100);
        if (roll < SmallRoomPercent)
        {
            return (random.NextInt(5, 8), random.NextInt(4, 6));
        }

        return roll < SmallRoomPercent + MediumRoomPercent
            ? (random.NextInt(8, 12), random.NextInt(5, 7))
            : (random.NextInt(12, 16), random.NextInt(7, 9));
    }

    private static void CarveRooms(TileGrid grid, List<RoomDraft> rooms)
    {
        foreach (RoomDraft room in rooms)
        {
            RoomBounds interior = room.Interior;
            for (int y = interior.Y; y <= interior.Bottom; y++)
            {
                for (int x = interior.X; x <= interior.Right; x++)
                {
                    grid[x, y] = CellType.Floor;
                }
            }
        }
    }

    private static void CarveCorridors(TileGrid grid, List<RoomDraft> rooms)
    {
        foreach (RoomDraft room in rooms)
        {
            foreach (
                RoomDraft other in room
                    .Connections.Where(id => id > room.Id)
                    .Select(id => rooms[id])
            )
            {
                bool horizontal = room.GridCell.Y == other.GridCell.Y;
                (RoomDraft first, RoomDraft second) = horizontal
                    ? (room.GridCell.X < other.GridCell.X ? (room, other) : (other, room))
                    : (room.GridCell.Y < other.GridCell.Y ? (room, other) : (other, room));

                if (horizontal)
                {
                    int y = first.Center.Y;
                    int firstDoor = first.Interior.Right + 1;
                    int secondDoor = second.Interior.X - 1;
                    for (int x = firstDoor + 1; x < secondDoor; x++)
                    {
                        grid[x, y] = CellType.Floor;
                    }

                    grid[firstDoor, y] = CellType.Door;
                    grid[secondDoor, y] = CellType.Door;
                }
                else
                {
                    int x = first.Center.X;
                    int firstDoor = first.Interior.Bottom + 1;
                    int secondDoor = second.Interior.Y - 1;
                    for (int y = firstDoor + 1; y < secondDoor; y++)
                    {
                        grid[x, y] = CellType.Floor;
                    }

                    grid[x, firstDoor] = CellType.Door;
                    grid[x, secondDoor] = CellType.Door;
                }
            }
        }
    }

    /// <summary>Every void tile touching a walkable tile, diagonals included, becomes a wall.</summary>
    private static void RaiseWalls(TileGrid grid)
    {
        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                if (grid[x, y] == CellType.Void && TouchesWalkable(grid, x, y))
                {
                    grid[x, y] = CellType.Wall;
                }
            }
        }
    }

    private static void PlaceStairs(TileGrid grid, List<RoomDraft> rooms, int floorIndex)
    {
        if (floorIndex > 0)
        {
            Position arrival = rooms[0].Center;
            grid[arrival.X, arrival.Y] = CellType.StairsUp;
        }

        foreach (RoomDraft room in rooms.Where(room => room.Type == RoomType.Stairs))
        {
            grid[room.Center.X, room.Center.Y] = CellType.StairsDown;
        }
    }

    private static List<DungeonElement> PlaceElements(
        DeterministicRandom random,
        TileGrid grid,
        List<RoomDraft> rooms
    )
    {
        List<DungeonElement> elements = [];
        int maxDepth = Math.Max(1, rooms.Max(room => room.Depth));

        void Add(ElementType type, Position position, RoomDraft room) =>
            elements.Add(new DungeonElement(elements.Count, type, position, room.Id));

        foreach (RoomDraft room in rooms)
        {
            switch (room.Type)
            {
                case RoomType.Boss:
                    Add(ElementType.Boss, room.Center, room);
                    break;

                case RoomType.Treasure:
                    Add(ElementType.Item, room.Center, room);
                    break;

                case RoomType.Combat:
                    List<Position> freeTiles = RoomGeometry
                        .InteriorTiles(room)
                        .Where(tile =>
                            grid[tile.X, tile.Y] == CellType.Floor && tile != room.Center
                        )
                        .ToList();
                    random.Shuffle(freeTiles);

                    if (random.Chance(TrapChancePercent))
                    {
                        foreach (Position tile in TrapLine(random, grid, room, freeTiles))
                        {
                            Add(ElementType.Trap, tile, room);
                            freeTiles.Remove(tile);
                        }
                    }

                    // Difficulty grows with the distance from the start room.
                    int enemyCount = Math.Min(
                        MaximumEnemiesPerRoom,
                        1
                            + room.Depth * 2 / maxDepth
                            + (random.Chance(ExtraEnemyChancePercent) ? 1 : 0)
                    );

                    foreach (Position tile in freeTiles.Take(enemyCount))
                    {
                        Add(ElementType.Enemy, tile, room);
                    }

                    if (random.Chance(CombatLootChancePercent) && freeTiles.Count > enemyCount)
                    {
                        Add(ElementType.Item, freeTiles[enemyCount], room);
                    }

                    break;

                default:
                    // Start, Stairs and Empty rooms hold no element.
                    break;
            }
        }

        return elements;
    }

    /// <summary>Two to four spikes in a row, across the path of the party.</summary>
    private static List<Position> TrapLine(
        DeterministicRandom random,
        TileGrid grid,
        RoomDraft room,
        List<Position> freeTiles
    )
    {
        if (freeTiles.Count == 0)
        {
            return [];
        }

        Position start = freeTiles[0];
        Direction direction = random.NextInt(2) == 0 ? Direction.East : Direction.South;
        int length = random.NextInt(2, 5);
        List<Position> line = [start];

        Position next = start.Step(direction);
        while (
            line.Count < length
            && room.Interior.Contains(next)
            && next != room.Center
            && grid[next.X, next.Y] == CellType.Floor
        )
        {
            line.Add(next);
            next = next.Step(direction);
        }

        return line.Count >= 2 ? line : [];
    }

    private static bool TouchesWalkable(TileGrid grid, int x, int y)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int nx = x + dx;
                int ny = y + dy;
                if (grid.Contains(nx, ny) && grid[nx, ny].IsWalkable())
                {
                    return true;
                }
            }
        }

        return false;
    }
}
