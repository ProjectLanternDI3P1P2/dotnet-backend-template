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
        DungeonRun run =
            await dungeonRunRepository.GetByIdAsync(request.RunId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Dungeon run not found with RunId '{request.RunId}'."
            );

        GeneratedDungeon dungeon = dungeonProvider.Get(run.Seed, run.Settings, run.GeneratorVersion);

        // Refused with BossNotInReachException (409) when the hero is not in the boss room.
        run.DefeatFloorBoss(dungeon);
    }
}
