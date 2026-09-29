using Dungeon.Domain.ValueObjects;

namespace Dungeon.Application.Features.DungeonUseCase;

/// <summary>Shared by every validator that receives a seed as text.</summary>
public static class SeedRules
{
    public static readonly string InvalidSeedMessage =
        $"Seed must be {Seed.TextLength} base32 characters.";

    public static bool BeAValidSeed(string? seed) => Seed.TryParse(seed, out _);
}
