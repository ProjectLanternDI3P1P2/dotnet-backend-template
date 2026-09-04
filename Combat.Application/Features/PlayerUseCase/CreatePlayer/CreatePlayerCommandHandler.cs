using Combat.Domain.Entities;
using Combat.Domain.Repositories;
using MediatR;

namespace Combat.Application.Features.PlayerUseCase.CreatePlayer;

public sealed class CreatePlayerCommandHandler(IPlayerRepository playerRepository) : IRequestHandler<CreatePlayerCommand>
{
    public async Task Handle(CreatePlayerCommand request, CancellationToken cancellationToken)
    {
        Player player = new()
        {
            Id = request.Id,
            Name = request.Name,
            Attack = request.Attack,
            Health = request.Health,
            MaxHealth = request.MaxHealth
        };

        await playerRepository.AddPlayerAsync(player, cancellationToken);
    }
}
