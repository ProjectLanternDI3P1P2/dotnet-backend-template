using Dungeon.Application.Ports;
using Dungeon.Domain.Entities;
using Dungeon.Domain.Repositories;

namespace Dungeon.Application.Features.DungeonRunUseCase;

/// <summary>Shared by every use case that acts on an existing run.</summary>
internal static class DungeonRunLookup
{
    public static async Task<DungeonRun> GetRequiredAsync(
        this IDungeonRunRepository dungeonRunRepository,
        Guid runId,
        CancellationToken cancellationToken
    ) =>
        await dungeonRunRepository.GetByIdAsync(runId, cancellationToken)
        ?? throw new KeyNotFoundException($"Dungeon run not found with RunId '{runId}'.");

    /// <summary>The dungeon of a run, rebuilt from the settings and version it was created with.</summary>
    public static GeneratedDungeon Get(this IDungeonProvider dungeonProvider, DungeonRun run) =>
        dungeonProvider.Get(run.Seed, run.Settings, run.GeneratorVersion);
}
