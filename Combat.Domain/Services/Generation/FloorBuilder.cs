using Combat.Domain.Entities;
using Combat.Domain.Enums;
using Combat.Domain.Services.Randomness;
using Combat.Domain.ValueObjects;

namespace Combat.Domain.Services.Generation;

/// <summary>
/// Stages 2 to 4: turns the room graph into tiles, then furnishes the rooms.
/// <para>
/// Each grid cell is a 15 x 11 tile block. A room's interior always covers the block's
/// centre row and centre column, so the corridor between two neighbouring rooms is a
/// straight line along that row or column: it can never miss a door or cross another room.
/// Obstacles are never put on that central cross, so every door stays reachable.
/// </para>
/// </summary>
internal static class FloorBuilder
{
    public const int CellWidth = 15;
    public const int CellHeight = 11;

    private const int CenterX = CellWidth / 2;
    private const int CenterY = CellHeight / 2;

    // Interior sizes, walls excluded. The largest room still leaves a one-tile margin
    // around its walls, so two rooms never share a wall.
    private const int MinimumWidth = 5;
    private const int MaximumWidth = 11;
    private const int MinimumHeight = 4;
    private const int MaximumHeight = 7;

    private const int TilesPerObstacle = 14;
    private const int MaximumEnemiesPerRoom = 4;
    private const int ExtraEnemyChancePercent = 35;
    private const int CombatLootChancePercent = 25;

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
        RaiseWalls(grid);

        DeterministicRandom content = DeterministicRandom.ForStream(
            seed,
            RandomStream.Content,
            floorIndex
        );
        PlaceObstacles(content, grid, rooms);
        PlaceStairs(grid, rooms, floorIndex);
        List<DungeonElement> elements = PlaceElements(content, grid, rooms);

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
                RoomType.Start or RoomType.Stairs => (7, 5),
                RoomType.Boss => (MaximumWidth, MaximumHeight),
                _ => (
                    random.NextInt(MinimumWidth, MaximumWidth + 1),
                    random.NextInt(MinimumHeight, MaximumHeight + 1)
                ),
            };

            // Local offset of the interior inside its block: walls stay off the block edge
            // and the interior always covers the block's centre tile.
            int localX = random.NextInt(
                Math.Max(2, CenterX - width + 1),
                Math.Min(CenterX, CellWidth - 2 - width) + 1
            );
            int localY = random.NextInt(
                Math.Max(2, CenterY - height + 1),
                Math.Min(CenterY, CellHeight - 2 - height) + 1
            );

            int originX = (room.GridCell.X - minGridX) * CellWidth;
            int originY = (room.GridCell.Y - minGridY) * CellHeight;

            room.Interior = new RoomBounds(originX + localX, originY + localY, width, height);
            room.Center = new Position(originX + CenterX, originY + CenterY);
        }
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
            foreach (RoomDraft other in room.Connections.Where(id => id > room.Id).Select(id => rooms[id]))
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

    private static void PlaceObstacles(DeterministicRandom random, TileGrid grid, List<RoomDraft> rooms)
    {
        foreach (
            RoomDraft room in rooms.Where(room =>
                room.Type is RoomType.Combat or RoomType.Treasure or RoomType.Empty
            )
        )
        {
            int target = random.NextInt(room.Interior.Area / TilesPerObstacle + 1);
            List<Position> candidates = InteriorTiles(room)
                .Where(tile => tile.X != room.Center.X && tile.Y != room.Center.Y)
                .ToList();
            random.Shuffle(candidates);

            int placed = 0;
            foreach (Position tile in candidates)
            {
                if (placed == target)
                {
                    break;
                }

                grid[tile.X, tile.Y] = CellType.Obstacle;

                // An obstacle must never wall off part of the room.
                if (IsRoomFullyConnected(grid, room))
                {
                    placed++;
                }
                else
                {
                    grid[tile.X, tile.Y] = CellType.Floor;
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
                    List<Position> freeTiles = InteriorTiles(room)
                        .Where(tile => grid[tile.X, tile.Y] == CellType.Floor && tile != room.Center)
                        .ToList();
                    random.Shuffle(freeTiles);

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

    private static bool IsRoomFullyConnected(TileGrid grid, RoomDraft room)
    {
        RoomBounds interior = room.Interior;
        int walkableCount = InteriorTiles(room).Count(tile => grid[tile.X, tile.Y].IsWalkable());

        bool[] visited = new bool[interior.Area];
        Queue<Position> queue = new();
        queue.Enqueue(room.Center);
        visited[Index(interior, room.Center)] = true;
        int reached = 0;

        while (queue.Count > 0)
        {
            Position current = queue.Dequeue();
            reached++;

            foreach (Direction direction in Enum.GetValues<Direction>())
            {
                Position next = current.Step(direction);
                if (
                    interior.Contains(next)
                    && !visited[Index(interior, next)]
                    && grid[next.X, next.Y].IsWalkable()
                )
                {
                    visited[Index(interior, next)] = true;
                    queue.Enqueue(next);
                }
            }
        }

        return reached == walkableCount;
    }

    private static IEnumerable<Position> InteriorTiles(RoomDraft room)
    {
        RoomBounds interior = room.Interior;
        for (int y = interior.Y; y <= interior.Bottom; y++)
        {
            for (int x = interior.X; x <= interior.Right; x++)
            {
                yield return new Position(x, y);
            }
        }
    }

    private static int Index(RoomBounds interior, Position position)
    {
        return (position.Y - interior.Y) * interior.Width + (position.X - interior.X);
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

    private sealed class TileGrid(int width, int height)
    {
        public int Width { get; } = width;

        public int Height { get; } = height;

        public CellType[] Cells { get; } = new CellType[width * height];

        public CellType this[int x, int y]
        {
            get => Cells[y * Width + x];
            set => Cells[y * Width + x] = value;
        }

        public bool Contains(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }
    }
}
