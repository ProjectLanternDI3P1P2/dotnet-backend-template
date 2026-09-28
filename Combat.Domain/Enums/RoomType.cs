namespace Combat.Domain.Enums;

public enum RoomType
{
    /// <summary>Where the party arrives on a floor.</summary>
    Start,
    Combat,
    Treasure,

    /// <summary>A quiet room: no enemy, no loot. Gives the exploration some rhythm.</summary>
    Empty,

    /// <summary>The final room of the last floor. Holds the final boss.</summary>
    Boss,

    /// <summary>The exit of a floor that is not the last one. Holds the stairs down.</summary>
    Stairs,
}
