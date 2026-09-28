namespace Combat.Domain.Enums;

// Byte-sized: a 40-room floor holds about 17,000 tiles and generated dungeons are cached.
public enum CellType : byte
{
    /// <summary>Outside the dungeon: never rendered as a room, never walkable.</summary>
    Void,
    Floor,
    Wall,
    Door,

    /// <summary>A pillar, barrel or rubble inside a room. Blocks movement.</summary>
    Obstacle,
    StairsDown,
    StairsUp,
}

public static class CellTypeExtensions
{
    public static bool IsWalkable(this CellType cellType)
    {
        return cellType
            is CellType.Floor
                or CellType.Door
                or CellType.StairsDown
                or CellType.StairsUp;
    }
}
