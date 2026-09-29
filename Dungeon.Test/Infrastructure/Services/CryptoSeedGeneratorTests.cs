using Dungeon.Domain.ValueObjects;
using Dungeon.Infrastructure.Services;
using FluentAssertions;

namespace Dungeon.Test.Infrastructure.Services;

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
