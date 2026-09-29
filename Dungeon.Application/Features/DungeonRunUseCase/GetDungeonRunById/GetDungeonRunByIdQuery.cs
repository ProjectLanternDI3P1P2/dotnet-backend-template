using MediatR;

namespace Dungeon.Application.Features.DungeonRunUseCase.GetDungeonRunById;

public record GetDungeonRunByIdQuery(Guid RunId) : IRequest<GetDungeonRunByIdResult>;
