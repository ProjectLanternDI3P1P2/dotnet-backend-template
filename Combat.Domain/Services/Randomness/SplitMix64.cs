namespace Combat.Domain.Services.Randomness;

/// <summary>
/// SplitMix64 (Steele, Lea and Flood, 2014). Used only to expand one 64-bit seed into the
/// 256-bit state of <see cref="DeterministicRandom"/> and to derive independent streams.
/// </summary>
public static class SplitMix64
{
    private const ulong GoldenGamma = 0x9E3779B97F4A7C15;

    /// <summary>Advances <paramref name="state"/> and returns the next output.</summary>
    public static ulong Next(ref ulong state)
    {
        unchecked
        {
            state += GoldenGamma;
            return Mix(state);
        }
    }

    /// <summary>The SplitMix64 finaliser: a bijective, well-avalanched 64-bit hash.</summary>
    public static ulong Mix(ulong value)
    {
        unchecked
        {
            ulong z = value;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EB;
            return z ^ (z >> 31);
        }
    }
}
