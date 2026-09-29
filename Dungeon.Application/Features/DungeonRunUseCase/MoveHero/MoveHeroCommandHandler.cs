using Dungeon.Application.Models;
using Dungeon.Application.Ports;
using Dungeon.Domain.Entities;
using Dungeon.Domain.Enums;
using Dungeon.Domain.Repositories;
using MediatR;

namespace Dungeon.Application.Features.DungeonRunUseCase.MoveHero;

public sealed class MoveHeroCommandHandler(
    IDungeonRunRepository dungeonRunRepository,
    IDungeonProvider dungeonProvider
) : IRequestHandler<MoveHeroCommand>
{
    public async Task Handle(MoveHeroCommand request, CancellationToken cancellationToken)
    {
        DungeonRun run =
            await dungeonRunRepository.GetByIdAsync(request.RunId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Dungeon run not found with RunId '{request.RunId}'."
            );

        // The validator has already rejected unknown directions (422).
        DungeonContract.TryParseDirection(request.Direction, out Direction direction);

        GeneratedDungeon dungeon = dungeonProvider.Get(run.Seed, run.Settings, run.GeneratorVersion);

        // The walkability rule lives in the domain: walls, obstacles, the void and anything
        // outside the floor raise InvalidMoveException (409) and leave the run untouched.
        run.MoveHero(direction, dungeon);
    }
}
