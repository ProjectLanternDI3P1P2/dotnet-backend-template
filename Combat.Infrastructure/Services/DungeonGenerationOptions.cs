using Combat.Domain.ValueObjects;

namespace Combat.Infrastructure.Services;

/// <summary>
/// Shape of the dungeons created from now on. Existing runs keep the settings they were
/// created with, so changing this never alters a dungeon already being played or shared.
/// </summary>
public sealed class DungeonGenerationOptions
{
    public const string SectionName = "Dungeon:Generation";

    public int RoomCount { get; init; } = DungeonSettings.DefaultRoomCount;

    /// <summary>1 by default; more floors split the rooms and link them with stairs.</summary>
    public int FloorCount { get; init; } = 1;
}
