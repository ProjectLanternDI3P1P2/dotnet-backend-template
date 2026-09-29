using MediatR;

namespace Dungeon.Application.Features.DungeonUseCase.GetDungeonCell;

/// <summary>X and Y are nullable so that a missing coordinate is a 422, not a silent 0.</summary>
public record GetDungeonCellQuery(string Seed, int Floor, int? X, int? Y)
    : IRequest<GetDungeonCellResult>;
