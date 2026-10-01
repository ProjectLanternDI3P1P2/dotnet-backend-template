namespace Dungeon.Domain.Services.Generation;

/// <summary>
/// How often a template comes up among those that fit a room: its weight in the draw. A
/// rare room, a labyrinth say, is something a party remembers finding.
/// </summary>
public enum RoomRarity
{
    Rare = 1,
    Uncommon = 3,
    Common = 8,
}
