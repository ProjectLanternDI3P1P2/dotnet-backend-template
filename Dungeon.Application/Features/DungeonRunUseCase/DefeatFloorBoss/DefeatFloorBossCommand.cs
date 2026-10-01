using Dungeon.Application.Abstractions;

namespace Dungeon.Application.Features.DungeonRunUseCase.DefeatFloorBoss;

/// <summary>
/// Records that the party defeated the boss of its current floor: the gate down
/// opens, and the run is won on the last floor. Meant to be sent by Combat when the fight is
/// won; the hero must stand in the boss room.
/// </summary>
public record DefeatFloorBossCommand(Guid RunId) : ICommand;
