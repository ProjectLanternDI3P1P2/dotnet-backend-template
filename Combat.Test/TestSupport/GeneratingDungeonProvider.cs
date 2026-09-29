using Combat.Application.Ports;
using Combat.Domain.Entities;
using Combat.Domain.Services.Generation;
using Combat.Domain.ValueObjects;

namespace Combat.Test.TestSupport;

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
