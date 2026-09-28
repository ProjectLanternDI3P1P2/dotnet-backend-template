using Combat.Domain.Entities;
using Combat.Domain.ValueObjects;

namespace Combat.Application.Ports;

/// <summary>
/// Returns the dungeon a seed produces. The implementation may cache: the result depends
/// only on its three arguments.
/// </summary>
public interface IDungeonProvider
{
    Dungeon Get(Seed seed, DungeonSettings settings, int generatorVersion);
}
