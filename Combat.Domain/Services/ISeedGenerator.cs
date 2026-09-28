using Combat.Domain.ValueObjects;

namespace Combat.Domain.Services;

/// <summary>
/// Source of fresh seeds for new explorations. Unlike <c>DeterministicRandom</c>, it must be
/// unpredictable, so it is implemented in Infrastructure over a cryptographic generator.
/// </summary>
public interface ISeedGenerator
{
    Seed NewSeed();
}
