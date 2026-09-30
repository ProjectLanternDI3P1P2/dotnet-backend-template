using Dungeon.Domain.Entities;
using Dungeon.Domain.Repositories;

namespace Dungeon.Infrastructure.Persistence.Repositories;

public sealed class PlayerRepository(DungeonDbContext dbContext) : IPlayerRepository
{
    public async Task AddPlayerAsync(Player player, CancellationToken cancellationToken)
    {
        await dbContext.Players.AddAsync(player, cancellationToken);
    }

    public async Task<Player?> GetPlayerByIdAsync(
        Guid playerId,
        CancellationToken cancellationToken
    )
    {
        return await dbContext.Players.FindAsync([playerId], cancellationToken);
    }
}
