using Dungeon.Domain.Entities;
using Dungeon.Domain.ValueObjects;

namespace Dungeon.Domain.Repositories;

public interface IDungeonRunRepository
{
    /// <summary>Returns a tracked run: changes are saved by the command transaction.</summary>
    Task<DungeonRun?> GetByIdAsync(Guid runId, CancellationToken cancellationToken);

    /// <summary>
    /// The most recent run that used this seed. A seed is "known" once a run has used it,
    /// and that run tells which settings and generator version reproduce its dungeon.
    /// </summary>
    Task<DungeonRun?> FindLatestBySeedAsync(Seed seed, CancellationToken cancellationToken);

    Task AddAsync(DungeonRun dungeonRun, CancellationToken cancellationToken);
}
