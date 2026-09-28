namespace Combat.Domain.Enums;

public enum RoomType
{
    /// <summary>Where the party arrives on a floor.</summary>
    Start,
    Combat,
    Treasure,

    /// <summary>A quiet room: no enemy, no loot. Gives the exploration some rhythm.</summary>
    Empty,

    /// <summary>
    /// The last room of every floor. Its boss guards the gate to the stairs room; on the last
    /// floor, it is the final boss.
    /// </summary>
    Boss,

    /// <summary>
    /// Behind the boss room of every floor but the last: holds the stairs down. Not counted
    /// in the 40 rooms of a dungeon.
    /// </summary>
    Stairs,
}
