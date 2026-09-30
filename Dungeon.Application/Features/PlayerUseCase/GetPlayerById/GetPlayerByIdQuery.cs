using MediatR;

namespace Dungeon.Application.Features.PlayerUseCase.GetPlayerById;

public record GetPlayerByIdQuery(Guid PlayerId) : IRequest<GetPlayerByIdResult>;
