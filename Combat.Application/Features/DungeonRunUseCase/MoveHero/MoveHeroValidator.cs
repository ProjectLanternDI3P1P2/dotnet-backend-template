using Combat.Application.Models;
using FluentValidation;

namespace Combat.Application.Features.DungeonRunUseCase.MoveHero;

public class MoveHeroValidator : AbstractValidator<MoveHeroCommand>
{
    public MoveHeroValidator()
    {
        RuleFor(command => command.RunId).NotEmpty().WithMessage("RunId is required.");

        RuleFor(command => command.Direction)
            .Must(direction => DungeonContract.TryParseDirection(direction, out _))
            .WithMessage("Direction must be one of: north, east, south, west.");
    }
}
