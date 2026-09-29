using Dungeon.Application.Ports;
using Dungeon.Domain.Entities;
using Dungeon.Domain.Repositories;
using MediatR;

namespace Dungeon.Application.Features.DungeonRunUseCase.TakeStairsDown;

public sealed class TakeStairsDownCommandHandler(
    IDungeonRunRepository dungeonRunRepository,
    IDungeonProvider dungeonProvider
) : IRequestHandler<TakeStairsDownCommand>
{
    public async Task Handle(TakeStairsDownCommand request, CancellationToken cancellationToken)
    {
        DungeonRun run =
            await dungeonRunRepository.GetByIdAsync(request.RunId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Dungeon run not found with RunId '{request.RunId}'."
            );

        GeneratedDungeon dungeon = dungeonProvider.Get(run.Seed, run.Settings, run.GeneratorVersion);

        // Refused with InvalidMoveException (409) when the hero is not on the stairs.
        run.TakeStairsDown(dungeon);
    }
}
