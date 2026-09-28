namespace Combat.Domain.Enums;

/// <summary>
/// Abstract elements placed by the generator. Their concrete content (which monster,
/// which loot, how much damage) belongs to Combat and Inventory, which read these
/// positions by seed.
/// </summary>
public enum ElementType
{
    Enemy,
    Boss,
    Item,

    /// <summary>Floor spikes. The tile stays walkable; the effect is decided elsewhere.</summary>
    Trap,
}
