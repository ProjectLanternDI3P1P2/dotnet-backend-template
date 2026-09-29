using Dungeon.Domain.Services.Randomness;
using Dungeon.Domain.ValueObjects;
using FluentAssertions;

namespace Dungeon.Test.Domain.Services.Randomness;

/// <summary>
/// The expected values come from an independent implementation of the published reference
/// algorithms (SplitMix64 and xoshiro256**). If they ever change, every dungeon changes.
/// </summary>
public class DeterministicRandomTests
{
    [Fact]
    public void SplitMix64_FromZero_MatchesTheReferenceAlgorithm()
    {
        // Arrange
        ulong state = 0;

        // Act
        ulong[] outputs =
        [
            SplitMix64.Next(ref state),
            SplitMix64.Next(ref state),
            SplitMix64.Next(ref state),
        ];

        // Assert
        outputs.Should().Equal(0xE220A8397B1DCDAF, 0x6E789E6AA1B965F4, 0x06C45D188009454F);
    }

    [Fact]
    public void NextUInt64_SeededWithZero_MatchesTheReferenceAlgorithm()
    {
        // Arrange
        var random = new DeterministicRandom(0);

        // Act
        ulong[] outputs = Enumerable.Range(0, 4).Select(_ => random.NextUInt64()).ToArray();

        // Assert
        outputs
            .Should()
            .Equal(0x99EC5F36CB75F2B4, 0xBF6E1F784956452A, 0x1A5F849D4933E6E0, 0x6AA594F1262D2D2C);
    }

    [Fact]
    public void NextInt_Seeded_MatchesTheReferenceAlgorithm()
    {
        // Arrange
        var random = new DeterministicRandom(42);

        // Act
        int[] outputs = Enumerable.Range(0, 12).Select(_ => random.NextInt(6)).ToArray();

        // Assert
        outputs.Should().Equal(0, 2, 4, 5, 5, 4, 4, 5, 4, 3, 4, 1);
    }

    [Fact]
    public void SameSeed_ProducesTheSameSequence()
    {
        // Arrange
        var first = new DeterministicRandom(123456789);
        var second = new DeterministicRandom(123456789);

        // Act
        ulong[] firstSequence = Enumerable.Range(0, 1000).Select(_ => first.NextUInt64()).ToArray();
        ulong[] secondSequence = Enumerable.Range(0, 1000).Select(_ => second.NextUInt64()).ToArray();

        // Assert
        firstSequence.Should().Equal(secondSequence);
    }

    [Fact]
    public void ForStream_DifferentStagesOrFloors_ProduceIndependentSequences()
    {
        // Arrange
        Seed seed = new(987654321);

        // Act
        ulong layout = DeterministicRandom.ForStream(seed, RandomStream.Layout, 0).NextUInt64();
        ulong content = DeterministicRandom.ForStream(seed, RandomStream.Content, 0).NextUInt64();
        ulong lowerFloor = DeterministicRandom.ForStream(seed, RandomStream.Layout, 1).NextUInt64();

        // Assert
        new[] { layout, content, lowerFloor }.Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(7)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void NextInt_AlwaysStaysWithinItsRange(int maxExclusive)
    {
        // Arrange
        var random = new DeterministicRandom(7);

        // Act
        int[] values = Enumerable.Range(0, 10_000).Select(_ => random.NextInt(maxExclusive)).ToArray();

        // Assert
        values.Should().OnlyContain(value => value >= 0 && value < maxExclusive);
    }

    [Fact]
    public void NextInt_IsUniformEnoughForGeneration()
    {
        // Arrange
        var random = new DeterministicRandom(2026);
        int[] buckets = new int[6];

        // Act
        for (int draw = 0; draw < 60_000; draw++)
        {
            buckets[random.NextInt(6)]++;
        }

        // Assert
        buckets.Should().OnlyContain(count => count > 9_500 && count < 10_500);
    }

    [Fact]
    public void Shuffle_ReturnsAPermutationOfTheItems()
    {
        // Arrange
        var random = new DeterministicRandom(5);
        List<int> items = Enumerable.Range(0, 50).ToList();

        // Act
        random.Shuffle(items);

        // Assert
        items.Order().Should().Equal(Enumerable.Range(0, 50));
        items.Should().NotEqual(Enumerable.Range(0, 50));
    }
}
