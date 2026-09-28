using Combat.Application.Features.DungeonUseCase;
using FluentValidation;

namespace Combat.Application.Features.DungeonRunUseCase.CreateDungeonRun;

public class CreateDungeonRunValidator : AbstractValidator<CreateDungeonRunCommand>
{
    public CreateDungeonRunValidator()
    {
        RuleFor(command => command.RunId).NotEmpty().WithMessage("RunId is required.");

        RuleFor(command => command.GameSessionId)
            .NotEmpty()
            .WithMessage("GameSessionId is required.");

        RuleFor(command => command.Seed)
            .Must(SeedRules.BeAValidSeed)
            .When(command => command.Seed is not null)
            .WithMessage(SeedRules.InvalidSeedMessage);
    }
}
