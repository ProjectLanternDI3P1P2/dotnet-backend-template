using FluentValidation;

namespace Dungeon.Application.Features.DungeonRunUseCase.TakeStairsDown;

public class TakeStairsDownValidator : AbstractValidator<TakeStairsDownCommand>
{
    public TakeStairsDownValidator()
    {
        RuleFor(command => command.RunId).NotEmpty().WithMessage("RunId is required.");
    }
}
