using Dungeon.Application.Messaging;
using Dungeon.Domain.Entities;
using Dungeon.Domain.Repositories;
using MediatR;

namespace Dungeon.Application.Features.PlayerUseCase.CreatePlayer;

public sealed class CreatePlayerCommandHandler(
    IPlayerRepository playerRepository,
    IMessagePublisher messagePublisher
) : IRequestHandler<CreatePlayerCommand>
{
    public async Task Handle(CreatePlayerCommand request, CancellationToken cancellationToken)
    {
        Player player = new()
        {
            Id = request.Id,
            Name = request.Name,
            Attack = request.Attack,
            Health = request.Health,
            MaxHealth = request.MaxHealth,
        };

        await playerRepository.AddPlayerAsync(player, cancellationToken);

        await messagePublisher.PublishAsync(
            PlayerCreatedMessageFactory.Create(
                player.Id,
                player.Name,
                player.Attack,
                player.Health,
                player.MaxHealth
            ),
            cancellationToken
        );
    }
}
