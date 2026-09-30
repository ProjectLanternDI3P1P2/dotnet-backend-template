using Dungeon.Domain.Entities;
using Dungeon.Domain.ValueObjects;

namespace Dungeon.Application.Ports;

/// <summary>
/// Returns the dungeon a seed produces. The implementation may cache: the result depends
/// only on its three arguments.
/// </summary>
public interface IDungeonProvider
{
    GeneratedDungeon Get(Seed seed, DungeonSettings settings, int generatorVersion);
}
