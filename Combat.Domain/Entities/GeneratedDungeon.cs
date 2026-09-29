using Combat.Domain.ValueObjects;

namespace Combat.Domain.Entities;

/// <summary>
/// Everything a seed produces. It is never persisted: it is regenerated from
/// (<see cref="Seed"/>, <see cref="Settings"/>, <see cref="GeneratorVersion"/>) on demand.
/// </summary>
public sealed class GeneratedDungeon
{
    public GeneratedDungeon(
        Seed seed,
        int generatorVersion,
        DungeonSettings settings,
        IReadOnlyList<DungeonFloor> floors
    )
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(floors.Count, settings.FloorCount);

        Seed = seed;
        GeneratorVersion = generatorVersion;
        Settings = settings;
        Floors = floors;
    }

    public Seed Seed { get; }

    public int GeneratorVersion { get; }

    public DungeonSettings Settings { get; }

    public IReadOnlyList<DungeonFloor> Floors { get; }

    /// <summary>The rooms of the dungeon, stairs rooms excluded (US-DUNGEON-01).</summary>
    public int RoomCount =>
        Floors.Sum(floor => floor.Rooms.Count(room => room.Type != Enums.RoomType.Stairs));
}
