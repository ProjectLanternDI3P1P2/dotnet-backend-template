using System.Buffers.Binary;
using System.Security.Cryptography;
using Dungeon.Domain.Services;
using Dungeon.Domain.ValueObjects;

namespace Dungeon.Infrastructure.Services;

/// <summary>
/// New seeds come from the OS cryptographic generator: two instances started at the same
/// millisecond never produce the same seed, and players cannot predict the next dungeon.
/// </summary>
public sealed class CryptoSeedGenerator : ISeedGenerator
{
    public Seed NewSeed()
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        RandomNumberGenerator.Fill(bytes);
        return new Seed(BinaryPrimitives.ReadUInt64LittleEndian(bytes));
    }
}
