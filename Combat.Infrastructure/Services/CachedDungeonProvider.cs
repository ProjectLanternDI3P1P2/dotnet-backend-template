using Combat.Application.Ports;
using Combat.Domain.Entities;
using Combat.Domain.Services.Generation;
using Combat.Domain.ValueObjects;
using Microsoft.Extensions.Caching.Memory;

namespace Combat.Infrastructure.Services;

/// <summary>
/// Generating a 40-room dungeon takes about a millisecond, but every move and every cell read
/// needs it. Because a dungeon depends only on its key, it is cached without any invalidation.
/// A dedicated cache instance keeps the size limit from constraining other IMemoryCache users.
/// </summary>
public sealed class CachedDungeonProvider(DungeonGenerator dungeonGenerator)
    : IDungeonProvider,
        IDisposable
{
    private const int MaximumCachedDungeons = 256;
    private static readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(30);

    private readonly MemoryCache _cache = new(
        new MemoryCacheOptions { SizeLimit = MaximumCachedDungeons }
    );

    public GeneratedDungeon Get(Seed seed, DungeonSettings settings, int generatorVersion)
    {
        var key = (seed, settings.RoomCount, settings.FloorCount, generatorVersion);

        return _cache.GetOrCreate(
            key,
            entry =>
            {
                entry.Size = 1;
                entry.SlidingExpiration = SlidingExpiration;
                return dungeonGenerator.Generate(seed, settings, generatorVersion);
            }
        )!;
    }

    public void Dispose()
    {
        _cache.Dispose();
    }
}
