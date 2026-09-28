namespace Combat.Domain.Enums;

// Byte-sized: a 40-room floor holds about 20,000 tiles and generated dungeons are cached.
// New values are only ever appended: the numbers are part of the golden-master snapshot.
public enum CellType : byte
{
    /// <summary>Outside the dungeon, or a pit inside a room: never walkable.</summary>
    Void,
    Floor,
    Wall,
    Door,

    /// <summary>A barrel, jar or pot inside a room. Blocks movement.</summary>
    Obstacle,
    StairsDown,
    StairsUp,

    /// <summary>A stone column inside a room. Blocks movement.</summary>
    Pillar,

    /// <summary>An iron fence, around a treasure for instance. Blocks movement.</summary>
    Fence,

    /// <summary>
    /// The door between a boss room and the stairs room. Part of the floor's layout like any
    /// door, but the hero may only cross it once the boss of the floor is defeated
    /// (<see cref="Entities.DungeonRun.MoveHero"/>).
    /// </summary>
    Gate,

    /// <summary>A sewer grate in the floor. Walkable.</summary>
    Grate,
}

public static class CellTypeExtensions
{
    /// <summary>
    /// Whether the tile can be walked on, as far as the layout goes. A <see cref="CellType.Gate"/>
    /// counts as walkable here; whether it is open depends on the run.
    /// </summary>
    public static bool IsWalkable(this CellType cellType)
    {
        return cellType
            is CellType.Floor
                or CellType.Door
                or CellType.StairsDown
                or CellType.StairsUp
                or CellType.Grate
                or CellType.Gate;
    }
}
