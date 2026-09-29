using FluentValidation;

namespace Dungeon.Application.Features.DungeonUseCase.GetDungeonMap;

public class GetDungeonMapValidator : AbstractValidator<GetDungeonMapQuery>
{
    public GetDungeonMapValidator()
    {
        RuleFor(query => query.Seed)
            .Must(SeedRules.BeAValidSeed)
            .WithMessage(SeedRules.InvalidSeedMessage);

        RuleFor(query => query.Floor)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Floor must be 0 or greater.");
    }
}
