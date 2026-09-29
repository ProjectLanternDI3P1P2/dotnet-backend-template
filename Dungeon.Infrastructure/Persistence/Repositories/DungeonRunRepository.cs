using Dungeon.Domain.Entities;
using Dungeon.Domain.Repositories;
using Dungeon.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Dungeon.Infrastructure.Persistence.Repositories;

public sealed class DungeonRunRepository(DungeonDbContext dbContext) : IDungeonRunRepository
{
    public async Task<DungeonRun?> GetByIdAsync(Guid runId, CancellationToken cancellationToken)
    {
        return await dbContext.DungeonRuns.FindAsync([runId], cancellationToken);
    }

    public async Task<DungeonRun?> FindLatestBySeedAsync(
        Seed seed,
        CancellationToken cancellationToken
    )
    {
        return await dbContext
            .DungeonRuns.AsNoTracking()
            .Where(run => run.Seed == seed)
            .OrderByDescending(run => run.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(DungeonRun dungeonRun, CancellationToken cancellationToken)
    {
        await dbContext.DungeonRuns.AddAsync(dungeonRun, cancellationToken);
    }
}
