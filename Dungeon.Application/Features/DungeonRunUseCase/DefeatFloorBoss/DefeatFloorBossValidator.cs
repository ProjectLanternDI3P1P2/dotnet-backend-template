using FluentValidation;

namespace Dungeon.Application.Features.DungeonRunUseCase.DefeatFloorBoss;

public class DefeatFloorBossValidator : AbstractValidator<DefeatFloorBossCommand>
{
    public DefeatFloorBossValidator()
    {
        RuleFor(command => command.RunId).NotEmpty().WithMessage("RunId is required.");
    }
}
