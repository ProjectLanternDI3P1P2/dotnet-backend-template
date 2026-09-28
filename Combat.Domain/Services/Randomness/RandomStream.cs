namespace Combat.Domain.Services.Randomness;

/// <summary>
/// Independent random streams derived from one seed. Each generation stage draws from its
/// own stream, so changing how many numbers one stage consumes (placing one more barrel,
/// say) never reshuffles the stages after it.
/// </summary>
public enum RandomStream : uint
{
    Layout = 1,
    RoomShapes = 2,
    Content = 3,
}
