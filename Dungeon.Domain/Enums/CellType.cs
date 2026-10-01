namespace Dungeon.Domain.Enums;

// Byte-sized: a 40-room floor holds about 20,000 tiles and generated dungeons are cached.
// The numbers are part of the golden-master snapshot: changing them changes every dungeon.
public enum CellType : byte
{
    /// <summary>Outside the dungeon, or a pit inside a room: never walkable.</summary>
    Void,
    Floor,
    Wall,
    Door,

    /// <summary>A barrel, jar or pot inside a room. Blocks movement.</summary>
    Obstacle,

    /// <summary>A stone column inside a room. Blocks movement.</summary>
    Pillar,

    /// <summary>An iron fence, around a treasure for instance. Blocks movement.</summary>
    Fence,

    /// <summary>
    /// The way down, in the north wall of the boss room of every floor but the last. Locked
    /// until the boss of the floor is defeated; then walking into it takes the party to the
    /// next floor (<see cref="Entities.DungeonRun.MoveHero"/>). Nobody ever stands on it.
    /// </summary>
    Gate,

    /// <summary>A sewer grate in the floor. Walkable.</summary>
    Grate,

    /// <summary>A pool of still water inside a room. Never walkable.</summary>
    Water,

    /// <summary>Molten rock: a pool of lava, in the lair of a boss. Never walkable.</summary>
    Lava,

    /// <summary>
    /// A stone tomb. Tombs lie side by side, two or three tiles long, and block movement.
    /// </summary>
    Tomb,
}

public static class CellTypeExtensions
{
    /// <summary>
    /// Whether the hero can stand on the tile. The gate is not: it is a door in a wall, which
    /// takes the party down once open.
    /// </summary>
    public static bool IsWalkable(this CellType cellType)
    {
        return cellType is CellType.Floor or CellType.Door or CellType.Grate;
    }
}
