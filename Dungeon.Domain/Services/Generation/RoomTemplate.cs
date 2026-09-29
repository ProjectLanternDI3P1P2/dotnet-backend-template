using Dungeon.Domain.Enums;

namespace Dungeon.Domain.Services.Generation;

/// <summary>
/// A hand-drawn room: one character per tile, see <see cref="RoomTemplates"/> for the legend.
/// </summary>
public sealed class RoomTemplate
{
    public const char FloorTile = '.';
    public const char WallTile = '#';
    public const char VoidTile = ' ';
    public const char ObstacleTile = 'o';
    public const char PillarTile = 'I';
    public const char FenceTile = '=';
    public const char GrateTile = 'g';
    public const char EnemySpot = 'e';
    public const char TrapSpot = 't';
    public const char TreasureSpot = '$';
    public const char BossSpot = 'B';
    public const char StairsDownTile = '>';
    public const char ArrivalSpot = '<';

    public RoomTemplate(string name, IReadOnlyList<RoomType> roomTypes, IReadOnlyList<string> rows)
    {
        ArgumentOutOfRangeException.ThrowIfZero(rows.Count);

        Name = name;
        RoomTypes = roomTypes;
        Rows = rows;
        Width = rows[0].Length;
        Height = rows.Count;
    }

    public string Name { get; }

    /// <summary>The kinds of room this template can be used for.</summary>
    public IReadOnlyList<RoomType> RoomTypes { get; }

    public IReadOnlyList<string> Rows { get; }

    public int Width { get; }

    public int Height { get; }

    /// <summary>The character at (x, y), read right to left when the template is mirrored.</summary>
    public char At(int x, int y, bool mirrored)
    {
        return Rows[y][mirrored ? Width - 1 - x : x];
    }

    /// <summary>What the tile is made of; the spots for elements are floor tiles.</summary>
    public static CellType CellTypeOf(char tile, bool hasStairsUp)
    {
        return tile switch
        {
            WallTile => CellType.Wall,
            VoidTile => CellType.Void,
            ObstacleTile => CellType.Obstacle,
            PillarTile => CellType.Pillar,
            FenceTile => CellType.Fence,
            GrateTile => CellType.Grate,
            StairsDownTile => CellType.StairsDown,
            ArrivalSpot => hasStairsUp ? CellType.StairsUp : CellType.Floor,
            FloorTile or EnemySpot or TrapSpot or TreasureSpot or BossSpot => CellType.Floor,
            _ => throw new ArgumentOutOfRangeException(
                nameof(tile),
                tile,
                "Unknown template tile."
            ),
        };
    }
}
