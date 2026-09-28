using Combat.Application.Models;
using Combat.Application.Ports;
using Combat.Domain.Entities;
using Combat.Domain.Repositories;
using MediatR;

namespace Combat.Application.Features.DungeonRunUseCase.GetDungeonRunById;

public sealed class GetDungeonRunByIdQueryHandler(
    IDungeonRunRepository dungeonRunRepository,
    IDungeonProvider dungeonProvider
) : IRequestHandler<GetDungeonRunByIdQuery, GetDungeonRunByIdResult>
{
    public async Task<GetDungeonRunByIdResult> Handle(
        GetDungeonRunByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        DungeonRun run =
            await dungeonRunRepository.GetByIdAsync(request.RunId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Dungeon run not found with RunId '{request.RunId}'."
            );

        DungeonFloor floor = dungeonProvider
            .Get(run.Seed, run.Settings, run.GeneratorVersion)
            .Floors[run.CurrentFloor];

        return new GetDungeonRunByIdResult
        {
            Id = run.Id,
            GameSessionId = run.GameSessionId,
            Seed = run.Seed.ToString(),
            GeneratorVersion = run.GeneratorVersion,
            Status = DungeonContract.Name(run.Status),
            FloorCount = run.FloorCount,
            CurrentFloor = run.CurrentFloor,
            Hero = PositionResult.From(run.HeroPosition),
            Turn = run.Turn,
            FloorBossDefeated = run.IsFloorBossDefeated,
            CurrentRoomId = floor.GetRoomId(run.HeroPosition),
            ElementsHere = floor
                .GetElementsAt(run.HeroPosition)
                .Select(DungeonElementResult.From)
                .ToList(),
            StartedAt = run.StartedAt,
        };
    }
}
