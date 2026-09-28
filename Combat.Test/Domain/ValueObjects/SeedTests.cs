using Combat.Domain.ValueObjects;
using FluentAssertions;

namespace Combat.Test.Domain.ValueObjects;

public class SeedTests
{
    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(0x0123456789ABCDEFUL)]
    [InlineData(ulong.MaxValue)]
    public void ToString_ThenParse_ReturnsTheSameSeed(ulong value)
    {
        // Arrange
        var seed = new Seed(value);

        // Act
        string text = seed.ToString();

        // Assert
        text.Should().HaveLength(Seed.TextLength);
        Seed.Parse(text).Should().Be(seed);
    }

    [Fact]
    public void ToString_LargestValue_UsesThirteenCrockfordCharacters()
    {
        new Seed(ulong.MaxValue).ToString().Should().Be("FZZZZZZZZZZZZ");
    }

    [Theory]
    [InlineData("0kx4-m2t9-qz7pa", "0KX4M2T9QZ7PA")]
    [InlineData("  0KX4M2T9QZ7PA ", "0KX4M2T9QZ7PA")]
    [InlineData("OKX4M2T9QZ7PA", "0KX4M2T9QZ7PA")]
    [InlineData("0KX4M2T9QZ7PL", "0KX4M2T9QZ7P1")]
    public void TryParse_TextTypedByAPlayer_IsNormalised(string input, string expected)
    {
        // Act
        bool parsed = Seed.TryParse(input, out Seed seed);

        // Assert
        parsed.Should().BeTrue();
        seed.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0KX4M2T9QZ7P")]
    [InlineData("0KX4M2T9QZ7PAA")]
    [InlineData("0KX4M2T9QZ7PU")]
    [InlineData("0KX4M2T9QZ7P!")]
    [InlineData("GZZZZZZZZZZZZ")]
    public void TryParse_InvalidText_ReturnsFalse(string? input)
    {
        Seed.TryParse(input, out _).Should().BeFalse();
    }

    [Fact]
    public void Parse_InvalidText_ThrowsFormatException()
    {
        // Act
        Action act = () => Seed.Parse("not-a-seed");

        // Assert
        act.Should().Throw<FormatException>();
    }
}
