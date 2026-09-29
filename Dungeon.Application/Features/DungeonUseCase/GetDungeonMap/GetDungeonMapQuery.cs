using MediatR;

namespace Dungeon.Application.Features.DungeonUseCase.GetDungeonMap;

public record GetDungeonMapQuery(string Seed, int Floor) : IRequest<GetDungeonMapResult>;
