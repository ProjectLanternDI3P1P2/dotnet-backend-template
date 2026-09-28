using MediatR;

namespace Combat.Application.Features.DungeonUseCase.GetDungeonMap;

public record GetDungeonMapQuery(string Seed, int Floor) : IRequest<GetDungeonMapResult>;
