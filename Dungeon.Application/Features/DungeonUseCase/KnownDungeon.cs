using Dungeon.Application.Ports;
using Dungeon.Domain.Entities;
using Dungeon.Domain.Repositories;
using Dungeon.Domain.ValueObjects;

namespace Dungeon.Application.Features.DungeonUseCase;

/// <summary>
/// Resolves a seed that a run has already used. An unknown seed is a 404 even though it
/// could technically be generated (US-DUNGEON-02): the settings and generator version that
/// reproduce a dungeon are stored with its run, not in the seed.
/// </summary>
internal static class KnownDungeon
{
    public static async Task<(GeneratedDungeon Dungeon, DungeonFloor Floor)> GetFloorAsync(
        string seedText,
        int floorIndex,
        IDungeonRunRepository dungeonRunRepository,
        IDungeonProvider dungeonProvider,
        CancellationToken cancellationToken
    )
    {
        // The validator has already rejected malformed seeds (422).
        Seed seed = Seed.Parse(seedText);

        DungeonRun run =
            await dungeonRunRepository.FindLatestBySeedAsync(seed, cancellationToken)
            ?? throw new KeyNotFoundException($"Dungeon not found with seed '{seed}'.");

        GeneratedDungeon dungeon = dungeonProvider.Get(seed, run.Settings, run.GeneratorVersion);

        return floorIndex < dungeon.Floors.Count
            ? (dungeon, dungeon.Floors[floorIndex])
            : throw new KeyNotFoundException(
                $"Dungeon '{seed}' has no floor {floorIndex} (floors: {dungeon.Floors.Count})."
            );
    }
}
