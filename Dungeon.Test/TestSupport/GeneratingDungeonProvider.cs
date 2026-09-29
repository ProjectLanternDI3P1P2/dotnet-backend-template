using Dungeon.Application.Ports;
using Dungeon.Domain.Entities;
using Dungeon.Domain.Services.Generation;
using Dungeon.Domain.ValueObjects;

namespace Dungeon.Test.TestSupport;

/// <summary>
/// The real generator without the cache. Generation is pure and fast, so handler tests use it
/// instead of a mock: they then exercise real dungeons.
/// </summary>
public sealed class GeneratingDungeonProvider : IDungeonProvider
{
    private readonly DungeonGenerator _generator = new();

    public GeneratedDungeon Get(Seed seed, DungeonSettings settings, int generatorVersion)
    {
        return _generator.Generate(seed, settings, generatorVersion);
    }
}
