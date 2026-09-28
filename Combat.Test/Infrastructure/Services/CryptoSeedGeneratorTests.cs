using Combat.Domain.ValueObjects;
using Combat.Infrastructure.Services;
using FluentAssertions;

namespace Combat.Test.Infrastructure.Services;

public class CryptoSeedGeneratorTests
{
    [Fact]
    public void NewSeed_SuccessiveExplorations_GetDifferentSeeds()
    {
        // Arrange
        var generator = new CryptoSeedGenerator();

        // Act
        List<Seed> seeds = Enumerable.Range(0, 10_000).Select(_ => generator.NewSeed()).ToList();

        // Assert
        seeds.Should().OnlyHaveUniqueItems();
    }
}
