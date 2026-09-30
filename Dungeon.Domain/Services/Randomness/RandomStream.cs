namespace Dungeon.Domain.Services.Randomness;

/// <summary>
/// Independent random streams derived from one seed. Each generation stage draws from its
/// own stream, so changing how many numbers one stage consumes (placing one more barrel,
/// say) never reshuffles the stages after it.
/// </summary>
public enum RandomStream : uint
{
    Layout = 1,

    // 2 was RoomShapes, no longer drawn from: never reuse it for another stage.
    Content = 3,
    RoomTemplates = 4,
}
