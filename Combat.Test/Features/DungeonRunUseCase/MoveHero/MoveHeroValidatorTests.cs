using Combat.Application.Features.DungeonRunUseCase.MoveHero;
using FluentValidation.TestHelper;

namespace Combat.Test.Features.DungeonRunUseCase.MoveHero;

public class MoveHeroValidatorTests
{
    private readonly MoveHeroValidator _validator = new();

    [Theory]
    [InlineData("north")]
    [InlineData("EAST")]
    [InlineData("South")]
    [InlineData("west")]
    public void Validate_KnownDirection_HasNoValidationErrors(string direction)
    {
        // Act
        var result = _validator.TestValidate(new MoveHeroCommand(Guid.NewGuid(), direction));

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("up")]
    [InlineData("1")]
    [InlineData("north-east")]
    public void Validate_UnknownDirection_HasValidationErrorForDirection(string direction)
    {
        // Act
        var result = _validator.TestValidate(new MoveHeroCommand(Guid.NewGuid(), direction));

        // Assert
        result.ShouldHaveValidationErrorFor(command => command.Direction);
    }

    [Fact]
    public void Validate_EmptyRunId_HasValidationErrorForRunId()
    {
        // Act
        var result = _validator.TestValidate(new MoveHeroCommand(Guid.Empty, "north"));

        // Assert
        result.ShouldHaveValidationErrorFor(command => command.RunId);
    }
}
