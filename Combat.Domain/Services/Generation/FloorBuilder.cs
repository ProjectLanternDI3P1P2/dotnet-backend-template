using Combat.Domain.Entities;
using Combat.Domain.Enums;
using Combat.Domain.Services.Randomness;
using Combat.Domain.ValueObjects;

namespace Combat.Domain.Services.Generation;

/// <summary>
/// Stages 2 to 4: gives every room a hand-drawn template, lays the rooms out, digs the
/// corridors, then places the elements on the spots the templates provide.
/// <para>
/// Each column of the room grid is as wide as its widest room plus a margin, and each row
/// as tall as its tallest room plus a margin. A room is centred in its cell, so every room of
/// a grid row shares the same centre row, and every room of a grid column the same centre
/// column: the corridor between two neighbouring rooms is a straight line that arrives in
/// the middle of a side, where every template has a walkable tile. The margins leave room
/// for the walls, the top of the north wall face and the outer face of the south wall.
/// </para>
/// </summary>
internal static class FloorBuilder
{
    /// <summary>Free tiles on each side of the largest room of a grid column or row.</summary>
    private const int Margin = 3;

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
        ChooseTemplates(
            DeterministicRandom.ForStream(seed, RandomStream.RoomTemplates, floorIndex),
            rooms
        );

        int minGridX = rooms.Min(room => room.GridCell.X);
        int minGridY = rooms.Min(room => room.GridCell.Y);
        (int width, int height) = PlaceRooms(rooms, minGridX, minGridY);

        TileGrid grid = new(width, height);
        StampRooms(grid, rooms, hasStairsUp: floorIndex > 0);
        CarveCorridors(grid, rooms);
        RaiseWalls(grid);

        List<DungeonElement> elements = PlaceElements(
            DeterministicRandom.ForStream(seed, RandomStream.Content, floorIndex),
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
            rooms[0].SpotsOf(RoomTemplate.ArrivalSpot)[0]
        );
    }

    /// <summary>
    /// A template per room, mirrored or not. A template is not used twice on a floor while
    /// another one of the same kind is still available.
    /// </summary>
    private static void ChooseTemplates(DeterministicRandom random, List<RoomDraft> rooms)
    {
        HashSet<string> used = [];

        foreach (RoomDraft room in rooms)
        {
            List<RoomTemplate> candidates = RoomTemplates
                .All.Where(template => template.RoomTypes.Contains(room.Type))
                .ToList();
            List<RoomTemplate> fresh = candidates
                .Where(template => !used.Contains(template.Name))
                .ToList();

            room.Template = random.Pick(fresh.Count > 0 ? fresh : candidates);
            room.IsMirrored = random.Chance(50);
            used.Add(room.Template.Name);
        }
    }

    /// <summary>Sizes the grid columns and rows, then centres each room in its cell.</summary>
    private static (int Width, int Height) PlaceRooms(
        List<RoomDraft> rooms,
        int minGridX,
        int minGridY
    )
    {
        int columns = rooms.Max(room => room.GridCell.X) - minGridX + 1;
        int rows = rooms.Max(room => room.GridCell.Y) - minGridY + 1;
        int[] columnWidths = new int[columns];
        int[] rowHeights = new int[rows];

        foreach (RoomDraft room in rooms)
        {
            int column = room.GridCell.X - minGridX;
            int row = room.GridCell.Y - minGridY;
            columnWidths[column] = Math.Max(columnWidths[column], room.Template.Width + 2 * Margin);
            rowHeights[row] = Math.Max(rowHeights[row], room.Template.Height + 2 * Margin);
        }

        int[] columnStarts = StartsOf(columnWidths);
        int[] rowStarts = StartsOf(rowHeights);

        foreach (RoomDraft room in rooms)
        {
            int column = room.GridCell.X - minGridX;
            int row = room.GridCell.Y - minGridY;

            // Template sizes and cell sizes are odd: the centres line up exactly.
            room.Center = new Position(
                columnStarts[column] + columnWidths[column] / 2,
                rowStarts[row] + rowHeights[row] / 2
            );
            room.Interior = new RoomBounds(
                room.Center.X - room.Template.Width / 2,
                room.Center.Y - room.Template.Height / 2,
                room.Template.Width,
                room.Template.Height
            );
        }

        return (columnWidths.Sum(), rowHeights.Sum());
    }

    private static int[] StartsOf(int[] sizes)
    {
        int[] starts = new int[sizes.Length];
        for (int index = 1; index < sizes.Length; index++)
        {
            starts[index] = starts[index - 1] + sizes[index - 1];
        }

        return starts;
    }

    private static void StampRooms(TileGrid grid, List<RoomDraft> rooms, bool hasStairsUp)
    {
        foreach (RoomDraft room in rooms)
        {
            RoomTemplate template = room.Template;
            for (int y = 0; y < template.Height; y++)
            {
                for (int x = 0; x < template.Width; x++)
                {
                    char tile = template.At(x, y, room.IsMirrored);
                    Position position = new(room.Interior.X + x, room.Interior.Y + y);
                    grid[position.X, position.Y] = RoomTemplate.CellTypeOf(tile, hasStairsUp);

                    if (tile is not (RoomTemplate.FloorTile or RoomTemplate.VoidTile))
                    {
                        if (!room.Spots.TryGetValue(tile, out List<Position>? spots))
                        {
                            spots = [];
                            room.Spots[tile] = spots;
                        }

                        spots.Add(position);
                    }
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

                    // The stairs room always lies north of the boss room: the door in the
                    // boss room's north wall is the gate, open once the boss is defeated.
                    bool isGate = first.Type == RoomType.Stairs && second.Type == RoomType.Boss;
                    grid[x, secondDoor] = isGate ? CellType.Gate : CellType.Door;
                }
            }
        }
    }

    /// <summary>
    /// Every void tile touching the inside of a room or corridor, diagonals included, becomes
    /// a wall: the railing of a balcony, say, stands in front of the wall that closes the room.
    /// </summary>
    private static void RaiseWalls(TileGrid grid)
    {
        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                if (grid[x, y] == CellType.Void && TouchesInside(grid, x, y))
                {
                    grid[x, y] = CellType.Wall;
                }
            }
        }
    }

    private static List<DungeonElement> PlaceElements(
        DeterministicRandom random,
        List<RoomDraft> rooms
    )
    {
        List<DungeonElement> elements = [];
        int maxDepth = Math.Max(
            1,
            rooms.Where(room => room.Type != RoomType.Stairs).Max(room => room.Depth)
        );

        void Add(ElementType type, Position position, RoomDraft room) =>
            elements.Add(new DungeonElement(elements.Count, type, position, room.Id));

        foreach (RoomDraft room in rooms)
        {
            if (room.Type is RoomType.Start or RoomType.Stairs)
            {
                continue;
            }

            // Spike traps are part of the design of a room: all of them are armed.
            foreach (Position trap in room.SpotsOf(RoomTemplate.TrapSpot))
            {
                Add(ElementType.Trap, trap, room);
            }

            switch (room.Type)
            {
                case RoomType.Boss:
                    Add(ElementType.Boss, room.SpotsOf(RoomTemplate.BossSpot)[0], room);
                    break;

                case RoomType.Treasure:
                    foreach (Position treasure in room.SpotsOf(RoomTemplate.TreasureSpot))
                    {
                        Add(ElementType.Item, treasure, room);
                    }

                    break;

                case RoomType.Combat:
                    List<Position> spots = [.. room.SpotsOf(RoomTemplate.EnemySpot)];
                    random.Shuffle(spots);

                    // Difficulty grows with the distance from the start room.
                    int enemyCount = Math.Min(
                        Math.Min(MaximumEnemiesPerRoom, spots.Count),
                        1
                            + room.Depth * 2 / maxDepth
                            + (random.Chance(ExtraEnemyChancePercent) ? 1 : 0)
                    );

                    foreach (Position spot in spots.Take(enemyCount))
                    {
                        Add(ElementType.Enemy, spot, room);
                    }

                    if (random.Chance(CombatLootChancePercent) && spots.Count > enemyCount)
                    {
                        Add(ElementType.Item, spots[enemyCount], room);
                    }

                    break;

                default:
                    // Empty rooms only hold their traps.
                    break;
            }
        }

        return elements;
    }

    private static bool TouchesInside(TileGrid grid, int x, int y)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int nx = x + dx;
                int ny = y + dy;
                if (grid.Contains(nx, ny) && grid[nx, ny] is not (CellType.Void or CellType.Wall))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
