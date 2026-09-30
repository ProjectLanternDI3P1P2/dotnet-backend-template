using Dungeon.Domain.ValueObjects;

namespace Dungeon.Infrastructure.Services;

/// <summary>
/// Shape of the dungeons created from now on. Existing runs keep the settings they were
/// created with, so changing this never alters a dungeon already being played or shared.
/// </summary>
public sealed class DungeonGenerationOptions
{
    public const string SectionName = "Dungeon:Generation";

    public int RoomCount { get; init; } = DungeonSettings.DefaultRoomCount;

    /// <summary>
    /// 4 by default: 10 rooms per floor, each floor ending with a boss that guards the stairs.
    /// </summary>
    public int FloorCount { get; init; } = DungeonSettings.DefaultFloorCount;
}
