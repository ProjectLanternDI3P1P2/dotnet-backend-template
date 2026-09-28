namespace Combat.Domain.Enums;

/// <summary>
/// Abstract elements placed by the generator. Their concrete content (which monster,
/// which loot, how much damage) belongs to Combat and Inventory, which read these
/// positions by seed.
/// </summary>
public enum ElementType
{
    Enemy,

    /// <summary>One per floor, in the boss room. The one of the last floor is the final boss.</summary>
    Boss,
    Item,

    /// <summary>Floor spikes. The tile stays walkable; the effect is decided elsewhere.</summary>
    Trap,
}
