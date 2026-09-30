using Dungeon.Application.Ports;
using Dungeon.Domain.Entities;
using Dungeon.Domain.Repositories;
using MediatR;

namespace Dungeon.Application.Features.DungeonRunUseCase.DefeatFloorBoss;

public sealed class DefeatFloorBossCommandHandler(
    IDungeonRunRepository dungeonRunRepository,
    IDungeonProvider dungeonProvider
) : IRequestHandler<DefeatFloorBossCommand>
{
    public async Task Handle(DefeatFloorBossCommand request, CancellationToken cancellationToken)
    {
        DungeonRun run = await dungeonRunRepository.GetRequiredAsync(
            request.RunId,
            cancellationToken
        );

        // Refused with BossNotInReachException (409) when the hero is not in the boss room.
        run.DefeatFloorBoss(dungeonProvider.Get(run));
    }
}
