using MediatR;

namespace Combat.Application.Features.DungeonRunUseCase.GetDungeonRunById;

public record GetDungeonRunByIdQuery(Guid RunId) : IRequest<GetDungeonRunByIdResult>;
