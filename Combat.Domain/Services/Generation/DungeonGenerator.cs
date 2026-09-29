using Combat.Domain.Entities;
using Combat.Domain.Exceptions;
using Combat.Domain.Services.Randomness;
using Combat.Domain.ValueObjects;

namespace Combat.Domain.Services.Generation;

/// <summary>
/// Builds a dungeon from a seed. The output depends only on (seed, settings, version):
/// no clock, no <see cref="System.Random"/>, no hash-ordered collection, no floating point.
/// </summary>
public sealed class DungeonGenerator
{
    /// <summary>
    /// Bump this whenever a change alters the output for an existing seed, and keep the old
    /// algorithm reachable if already stored runs must stay replayable.
    /// The golden-master test fails when the output changes without a bump.
    /// </summary>
    /// <remarks>
    /// Version 2: larger and more varied rooms, room layouts, pillars, fences, traps.
    /// Version 3: hand-drawn room templates, a boss on every floor guarding the stairs room.
    /// </remarks>
    public const int CurrentVersion = 3;

    public GeneratedDungeon Generate(Seed seed, DungeonSettings settings)
    {
        return Generate(seed, settings, CurrentVersion);
    }

    public GeneratedDungeon Generate(Seed seed, DungeonSettings settings, int generatorVersion)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (generatorVersion != CurrentVersion)
        {
            throw new UnsupportedGeneratorVersionException(generatorVersion, CurrentVersion);
        }

        List<DungeonFloor> floors = new(settings.FloorCount);
        for (int floorIndex = 0; floorIndex < settings.FloorCount; floorIndex++)
        {
            floors.Add(GenerateFloor(seed, settings, floorIndex));
        }

        GeneratedDungeon dungeon = new(seed, CurrentVersion, settings, floors);

        // Defence in depth: a rule broken here is a generator bug, and it must surface as an
        // error rather than as a room a player can never reach.
        IReadOnlyList<string> violations = DungeonValidator.Validate(dungeon);
        if (violations.Count > 0)
        {
            throw new DungeonGenerationException(seed.ToString(), violations);
        }

        return dungeon;
    }

    private static DungeonFloor GenerateFloor(Seed seed, DungeonSettings settings, int floorIndex)
    {
        bool isFinalFloor = floorIndex == settings.FloorCount - 1;

        List<RoomDraft> rooms = RoomGraphBuilder.Build(
            DeterministicRandom.ForStream(seed, RandomStream.Layout, floorIndex),
            settings.RoomCountForFloor(floorIndex),
            isFinalFloor
        );

        return FloorBuilder.Build(seed, floorIndex, isFinalFloor, rooms);
    }
}
