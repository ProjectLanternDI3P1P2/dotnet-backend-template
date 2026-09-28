using Combat.Domain.ValueObjects;

namespace Combat.Domain.Entities;

/// <summary>
/// Everything a seed produces. It is never persisted: it is regenerated from
/// (<see cref="Seed"/>, <see cref="Settings"/>, <see cref="GeneratorVersion"/>) on demand.
/// </summary>
public sealed class Dungeon
{
    public Dungeon(
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

    public int TotalRoomCount => Floors.Sum(floor => floor.Rooms.Count);
}
