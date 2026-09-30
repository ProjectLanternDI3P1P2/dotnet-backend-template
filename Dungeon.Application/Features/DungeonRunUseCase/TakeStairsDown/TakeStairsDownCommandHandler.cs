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
        DungeonRun run = await dungeonRunRepository.GetRequiredAsync(
            request.RunId,
            cancellationToken
        );

        // Refused with InvalidMoveException (409) when the hero is not on the stairs.
        run.TakeStairsDown(dungeonProvider.Get(run));
    }
}
