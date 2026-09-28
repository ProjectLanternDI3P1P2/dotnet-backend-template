using Combat.Domain.Enums;
using Combat.Domain.Services.Randomness;
using Combat.Domain.ValueObjects;

namespace Combat.Domain.Services.Generation;

/// <summary>
/// Stage 3: gives each room its own arrangement, so that two rooms of the same size do not
/// look alike. Structures (cut corners, pits, partition walls) are carved before the walls
/// are raised, so they get proper walls; furniture (pillars, fences, barrels) comes after.
/// <para>
/// Every change goes through <see cref="RoomGeometry.TryApply"/>: it is undone if it would
/// block the centre, a door, or cut off any part of the room.
/// </para>
/// </summary>
internal static class RoomLayouts
{
    private const int TilesPerScatteredObstacle = 14;
    private const int TilesPerExtraObstacle = 30;
    private const int StorageCornerChancePercent = 60;

    private static readonly RoomLayout[] LargeCombatLayouts =
    [
        RoomLayout.Pits,
        RoomLayout.Partition,
        RoomLayout.Colonnade,
        RoomLayout.Notched,
        RoomLayout.Plain,
    ];

    private static readonly RoomLayout[] MediumCombatLayouts =
    [
        RoomLayout.Notched,
        RoomLayout.Colonnade,
        RoomLayout.Storage,
        RoomLayout.Plain,
    ];

    private static readonly RoomLayout[] SmallCombatLayouts =
    [
        RoomLayout.Plain,
        RoomLayout.Notched,
    ];

    private static readonly RoomLayout[] LargeEmptyLayouts =
    [
        RoomLayout.Storage,
        RoomLayout.Pits,
        RoomLayout.Notched,
        RoomLayout.Partition,
    ];

    private static readonly RoomLayout[] SmallEmptyLayouts =
    [
        RoomLayout.Storage,
        RoomLayout.Notched,
        RoomLayout.Plain,
    ];

    public static void Choose(DeterministicRandom random, List<RoomDraft> rooms)
    {
        foreach (RoomDraft room in rooms)
        {
            RoomBounds interior = room.Interior;
            bool large = interior.Width >= 11 && interior.Height >= 7;
            bool medium = interior.Width >= 8 && interior.Height >= 5;

            room.Layout = room.Type switch
            {
                RoomType.Start => RoomLayout.Plain,
                RoomType.Stairs or RoomType.Boss => RoomLayout.Colonnade,
                RoomType.Treasure => RoomLayout.Vault,
                RoomType.Empty => random.Pick(large ? LargeEmptyLayouts : SmallEmptyLayouts),
                _ => random.Pick(
                    large ? LargeCombatLayouts
                    : medium ? MediumCombatLayouts
                    : SmallCombatLayouts
                ),
            };
        }
    }

    /// <summary>Carves structures as void: raising the walls then outlines them.</summary>
    public static void CarveStructures(
        DeterministicRandom random,
        TileGrid grid,
        List<RoomDraft> rooms
    )
    {
        foreach (RoomDraft room in rooms)
        {
            switch (room.Layout)
            {
                case RoomLayout.Notched:
                    CarveNotches(random, grid, room, rooms);
                    break;
                case RoomLayout.Pits:
                    CarvePits(random, grid, room, rooms);
                    break;
                case RoomLayout.Partition:
                    CarvePartition(random, grid, room, rooms);
                    break;
                default:
                    break;
            }
        }
    }

    public static void Furnish(DeterministicRandom random, TileGrid grid, List<RoomDraft> rooms)
    {
        foreach (RoomDraft room in rooms)
        {
            switch (room.Layout)
            {
                case RoomLayout.Colonnade:
                    PlacePillars(grid, room, rooms);
                    break;
                case RoomLayout.Vault:
                    if (!PlaceVault(grid, room, rooms))
                    {
                        ScatterObstacles(random, grid, room, rooms, TilesPerScatteredObstacle);
                    }

                    break;
                case RoomLayout.Storage:
                    PlaceStorage(random, grid, room, rooms);
                    break;
                case RoomLayout.Plain when room.Type != RoomType.Start:
                    ScatterObstacles(random, grid, room, rooms, TilesPerScatteredObstacle);
                    break;
                case RoomLayout.Notched or RoomLayout.Pits or RoomLayout.Partition:
                    ScatterObstacles(random, grid, room, rooms, TilesPerExtraObstacle);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>Cuts one, two opposite or all four corners, never touching the central cross.</summary>
    private static void CarveNotches(
        DeterministicRandom random,
        TileGrid grid,
        RoomDraft room,
        List<RoomDraft> rooms
    )
    {
        RoomBounds interior = room.Interior;
        Position center = room.Center;

        // Free columns and rows between the interior edge and the cross, per side.
        int left = center.X - 1 - interior.X;
        int right = interior.Right - center.X - 1;
        int top = center.Y - 1 - interior.Y;
        int bottom = interior.Bottom - center.Y - 1;

        (bool IsLeft, bool IsTop)[] corners = random.NextInt(3) switch
        {
            0 => [((random.NextInt(2) == 0), (random.NextInt(2) == 0))],
            1 => random.NextInt(2) == 0
                ? [(true, true), (false, false)]
                : [(false, true), (true, false)],
            _ => [(true, true), (false, true), (true, false), (false, false)],
        };

        int maxWidth = corners.Min(corner => corner.IsLeft ? left : right);
        int maxHeight = corners.Min(corner => corner.IsTop ? top : bottom);
        if (maxWidth < 2 || maxHeight < 2)
        {
            return;
        }

        int width = random.NextInt(2, maxWidth + 1);
        int height = random.NextInt(2, maxHeight + 1);

        foreach ((bool isLeft, bool isTop) in corners)
        {
            int x0 = isLeft ? interior.X : interior.Right - width + 1;
            int y0 = isTop ? interior.Y : interior.Bottom - height + 1;
            RoomGeometry.TryApply(
                grid,
                room,
                rooms,
                Rectangle(x0, y0, width, height),
                CellType.Void
            );
        }
    }

    /// <summary>
    /// Walled pits along the left and/or right side of the room. They may cross the centre
    /// row: the path from a side door then goes around them.
    /// </summary>
    private static void CarvePits(
        DeterministicRandom random,
        TileGrid grid,
        RoomDraft room,
        List<RoomDraft> rooms
    )
    {
        RoomBounds interior = room.Interior;
        Position center = room.Center;

        // One free tile between a pit and the room edge, and between a pit and the cross.
        int leftSpace = center.X - interior.X - 2;
        int rightSpace = interior.Right - center.X - 2;
        int heightSpace = interior.Height - 2;

        // A pit is at least 3 wide and 4 tall: less than that cannot be drawn as a pit.
        if (heightSpace < 4 || Math.Max(leftSpace, rightSpace) < 3)
        {
            return;
        }

        int width = random.NextInt(3, Math.Min(5, Math.Max(leftSpace, rightSpace)) + 1);
        int height = random.NextInt(4, Math.Min(6, heightSpace) + 1);
        int y0 = interior.Y + 1 + random.NextInt(heightSpace - height + 1);

        if (leftSpace >= width)
        {
            int x0 = center.X - 1 - width;
            RoomGeometry.TryApply(
                grid,
                room,
                rooms,
                Rectangle(x0, y0, width, height),
                CellType.Void
            );
        }

        if (rightSpace >= width)
        {
            int x0 = center.X + 2;
            RoomGeometry.TryApply(
                grid,
                room,
                rooms,
                Rectangle(x0, y0, width, height),
                CellType.Void
            );
        }
    }

    /// <summary>
    /// A wall that closes off a side room. Vertical walls are one tile thick; horizontal ones
    /// are two, which is how a wall seen from the front is drawn. The opening sits on the
    /// central cross, so the corridors still line up with it.
    /// </summary>
    private static void CarvePartition(
        DeterministicRandom random,
        TileGrid grid,
        RoomDraft room,
        List<RoomDraft> rooms
    )
    {
        RoomBounds interior = room.Interior;
        Position center = room.Center;
        List<Func<bool>> options = [];

        // Vertical wall, leaving a side room at least 3 tiles wide.
        if (center.X - interior.X >= 5)
        {
            options.Add(() =>
                VerticalPartition(interior.X + random.NextInt(3, center.X - interior.X - 1))
            );
        }

        if (interior.Right - center.X >= 5)
        {
            options.Add(() =>
                VerticalPartition(
                    interior.Right - random.NextInt(3, interior.Right - center.X - 1)
                )
            );
        }

        // Horizontal wall, two tiles thick, leaving a side room at least 2 tiles tall.
        if (center.Y - interior.Y >= 5)
        {
            options.Add(() =>
                HorizontalPartition(interior.Y + random.NextInt(2, center.Y - interior.Y - 2))
            );
        }

        if (interior.Bottom - center.Y >= 5)
        {
            options.Add(() =>
                HorizontalPartition(center.Y + random.NextInt(2, interior.Bottom - center.Y - 2))
            );
        }

        if (options.Count > 0)
        {
            random.Pick(options)();
        }

        bool VerticalPartition(int x)
        {
            List<Position> wall = [];
            for (int y = interior.Y; y <= interior.Bottom; y++)
            {
                if (y != center.Y)
                {
                    wall.Add(new Position(x, y));
                }
            }

            if (!RoomGeometry.TryApply(grid, room, rooms, wall, CellType.Void))
            {
                return false;
            }

            grid[x, center.Y] = CellType.Door;
            return true;
        }

        bool HorizontalPartition(int y)
        {
            List<Position> wall = [];
            for (int x = interior.X; x <= interior.Right; x++)
            {
                if (x != center.X)
                {
                    wall.Add(new Position(x, y));
                    wall.Add(new Position(x, y + 1));
                }
            }

            return RoomGeometry.TryApply(grid, room, rooms, wall, CellType.Void);
        }
    }

    /// <summary>Two rows of pillars, three tiles apart, mirrored around the centre column.</summary>
    private static void PlacePillars(TileGrid grid, RoomDraft room, List<RoomDraft> rooms)
    {
        RoomBounds interior = room.Interior;
        Position center = room.Center;
        if (interior.Width < 7 || interior.Height < 5)
        {
            return;
        }

        foreach (int y in (int[])[interior.Y + 1, interior.Bottom - 1])
        {
            for (int offset = 2; offset < interior.Width; offset += 3)
            {
                foreach (int x in (int[])[center.X - offset, center.X + offset])
                {
                    Position tile = new(x, y);
                    bool insideMargin = x > interior.X && x < interior.Right;
                    if (insideMargin && y != center.Y)
                    {
                        RoomGeometry.TryApply(grid, room, rooms, [tile], CellType.Pillar);
                    }
                }
            }
        }
    }

    /// <summary>A fence all around the centre, open towards the south.</summary>
    private static bool PlaceVault(TileGrid grid, RoomDraft room, List<RoomDraft> rooms)
    {
        RoomBounds interior = room.Interior;
        Position center = room.Center;
        bool fits =
            center.X - 2 >= interior.X
            && center.X + 2 <= interior.Right
            && center.Y - 2 >= interior.Y
            && center.Y + 2 <= interior.Bottom;
        if (!fits)
        {
            return false;
        }

        List<Position> fence = [];
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                bool isCenter = dx == 0 && dy == 0;
                bool isOpening = dx == 0 && dy == 1;
                if (!isCenter && !isOpening)
                {
                    fence.Add(new Position(center.X + dx, center.Y + dy));
                }
            }
        }

        return RoomGeometry.TryApply(grid, room, rooms, fence, CellType.Fence);
    }

    /// <summary>Barrels and jars piled against the walls, in some of the corners.</summary>
    private static void PlaceStorage(
        DeterministicRandom random,
        TileGrid grid,
        RoomDraft room,
        List<RoomDraft> rooms
    )
    {
        RoomBounds interior = room.Interior;
        (int X, int Y, int Dx, int Dy)[] corners =
        [
            (interior.X, interior.Y, 1, 1),
            (interior.Right, interior.Y, -1, 1),
            (interior.X, interior.Bottom, 1, -1),
            (interior.Right, interior.Bottom, -1, -1),
        ];

        foreach ((int x, int y, int dx, int dy) in corners)
        {
            if (!random.Chance(StorageCornerChancePercent))
            {
                continue;
            }

            Position[] pile =
            [
                new(x, y),
                new(x + dx, y),
                new(x, y + dy),
                new(x + 2 * dx, y),
            ];

            int size = random.NextInt(2, pile.Length + 1);
            foreach (Position tile in pile.Take(size))
            {
                RoomGeometry.TryApply(grid, room, rooms, [tile], CellType.Obstacle);
            }
        }
    }

    private static void ScatterObstacles(
        DeterministicRandom random,
        TileGrid grid,
        RoomDraft room,
        List<RoomDraft> rooms,
        int tilesPerObstacle
    )
    {
        int target = random.NextInt(room.Interior.Area / tilesPerObstacle + 1);
        List<Position> candidates = RoomGeometry
            .InteriorTiles(room)
            .Where(tile =>
                tile.X != room.Center.X
                && tile.Y != room.Center.Y
                && grid[tile.X, tile.Y] == CellType.Floor
            )
            .ToList();
        random.Shuffle(candidates);

        int placed = 0;
        foreach (Position tile in candidates)
        {
            if (placed == target)
            {
                break;
            }

            if (RoomGeometry.TryApply(grid, room, rooms, [tile], CellType.Obstacle))
            {
                placed++;
            }
        }
    }

    private static List<Position> Rectangle(int x0, int y0, int width, int height)
    {
        List<Position> tiles = new(width * height);
        for (int y = y0; y < y0 + height; y++)
        {
            for (int x = x0; x < x0 + width; x++)
            {
                tiles.Add(new Position(x, y));
            }
        }

        return tiles;
    }
}
