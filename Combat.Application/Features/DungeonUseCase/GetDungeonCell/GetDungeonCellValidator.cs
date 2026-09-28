using FluentValidation;

namespace Combat.Application.Features.DungeonUseCase.GetDungeonCell;

public class GetDungeonCellValidator : AbstractValidator<GetDungeonCellQuery>
{
    public GetDungeonCellValidator()
    {
        RuleFor(query => query.Seed)
            .Must(SeedRules.BeAValidSeed)
            .WithMessage(SeedRules.InvalidSeedMessage);

        RuleFor(query => query.Floor)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Floor must be 0 or greater.");

        RuleFor(query => query.X)
            .NotNull()
            .WithMessage("X is required.")
            .GreaterThanOrEqualTo(0)
            .WithMessage("X must be 0 or greater.");

        RuleFor(query => query.Y)
            .NotNull()
            .WithMessage("Y is required.")
            .GreaterThanOrEqualTo(0)
            .WithMessage("Y must be 0 or greater.");
    }
}
