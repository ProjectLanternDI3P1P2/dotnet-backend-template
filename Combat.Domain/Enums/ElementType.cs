namespace Combat.Domain.Enums;

/// <summary>
/// Abstract elements placed by the generator. Their concrete content (which monster,
/// which loot) belongs to Combat and Inventory, which read these positions by seed.
/// </summary>
public enum ElementType
{
    Enemy,
    Boss,
    Item,
}
