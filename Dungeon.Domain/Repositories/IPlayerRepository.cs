using Dungeon.Domain.Entities;

namespace Dungeon.Domain.Repositories;

public interface IPlayerRepository
{
    Task<Player?> GetPlayerByIdAsync(Guid playerId, CancellationToken cancellationToken);
    Task AddPlayerAsync(Player player, CancellationToken cancellationToken);
}
