using System.Numerics;
using Dungeon.Domain.ValueObjects;

namespace Dungeon.Domain.Services.Randomness;

/// <summary>
/// xoshiro256** (Blackman and Vigna, 2018), seeded through SplitMix64.
/// <para>
/// <see cref="System.Random"/> is deliberately not used: its algorithm is not guaranteed
/// to stay the same across .NET versions, which would silently change every dungeon
/// after a runtime upgrade. This implementation uses integer arithmetic only, so it gives
/// the same sequence on every machine, OS, runtime and service instance.
/// </para>
/// </summary>
public sealed class DeterministicRandom
{
    private ulong _s0;
    private ulong _s1;
    private ulong _s2;
    private ulong _s3;

    public DeterministicRandom(ulong seed)
    {
        ulong expansion = seed;
        _s0 = SplitMix64.Next(ref expansion);
        _s1 = SplitMix64.Next(ref expansion);
        _s2 = SplitMix64.Next(ref expansion);
        _s3 = SplitMix64.Next(ref expansion);
    }

    /// <summary>
    /// A generator dedicated to one stage of one floor. Floors never depend on each other,
    /// so a lower floor can be generated without generating the ones above it.
    /// </summary>
    public static DeterministicRandom ForStream(Seed seed, RandomStream stream, int floorIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(floorIndex);

        ulong streamKey = SplitMix64.Mix(((ulong)stream << 32) | (uint)floorIndex);
        return new DeterministicRandom(seed.Value ^ streamKey);
    }

    public ulong NextUInt64()
    {
        unchecked
        {
            ulong result = BitOperations.RotateLeft(_s1 * 5, 7) * 9;
            ulong t = _s1 << 17;

            _s2 ^= _s0;
            _s3 ^= _s1;
            _s1 ^= _s2;
            _s0 ^= _s3;
            _s2 ^= t;
            _s3 = BitOperations.RotateLeft(_s3, 45);

            return result;
        }
    }

    /// <summary>
    /// A uniform integer in [0, <paramref name="maxExclusive"/>), without modulo bias
    /// (Lemire's multiply-and-reject method).
    /// </summary>
    public int NextInt(int maxExclusive)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxExclusive, 1);

        uint range = (uint)maxExclusive;
        ulong product = (ulong)NextUInt32() * range;
        uint low = (uint)product;

        if (low < range)
        {
            uint threshold = unchecked(0u - range) % range;
            while (low < threshold)
            {
                product = (ulong)NextUInt32() * range;
                low = (uint)product;
            }
        }

        return (int)(product >> 32);
    }

    /// <summary>A uniform integer in [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).</summary>
    public int NextInt(int minInclusive, int maxExclusive)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(minInclusive, maxExclusive);

        return minInclusive + NextInt(maxExclusive - minInclusive);
    }

    /// <summary>True with a probability of <paramref name="percent"/> out of 100.</summary>
    public bool Chance(int percent)
    {
        return NextInt(100) < percent;
    }

    public T Pick<T>(IReadOnlyList<T> items)
    {
        ArgumentOutOfRangeException.ThrowIfZero(items.Count);

        return items[NextInt(items.Count)];
    }

    /// <summary>Fisher-Yates shuffle, in place.</summary>
    public void Shuffle<T>(IList<T> items)
    {
        for (int index = items.Count - 1; index > 0; index--)
        {
            int swapIndex = NextInt(index + 1);
            (items[index], items[swapIndex]) = (items[swapIndex], items[index]);
        }
    }

    private uint NextUInt32()
    {
        // The high bits of xoshiro256** are its strongest ones.
        return (uint)(NextUInt64() >> 32);
    }
}
