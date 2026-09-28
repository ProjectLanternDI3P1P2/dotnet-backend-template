using FluentValidation;

namespace Combat.Application.Features.DungeonRunUseCase.TakeStairsDown;

public class TakeStairsDownValidator : AbstractValidator<TakeStairsDownCommand>
{
    public TakeStairsDownValidator()
    {
        RuleFor(command => command.RunId).NotEmpty().WithMessage("RunId is required.");
    }
}
