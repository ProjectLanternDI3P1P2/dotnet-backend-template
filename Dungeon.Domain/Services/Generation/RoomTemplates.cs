using Dungeon.Domain.Services.Generation.Templates;

namespace Dungeon.Domain.Services.Generation;

/// <summary>
/// The hand-drawn rooms of the dungeon, inspired by the example maps of the tileset. The
/// generator picks one per room, mirrored or not, so that no two rooms of a floor look alike.
/// <para>
/// Legend: <c>.</c> floor, <c>#</c> wall, space: void (a pit), <c>o</c> barrel or jar,
/// <c>I</c> column, <c>=</c> railing, <c>g</c> sewer grate (walkable), <c>~</c> water,
/// <c>^</c> lava, <c>T</c> tomb (two or three side by side), <c>e</c> enemy spot,
/// <c>t</c> spike trap, <c>$</c> treasure spot, <c>B</c> boss.
/// </para>
/// <para>
/// Every template has odd dimensions, a walkable centre, and a walkable tile in the middle
/// of each side, where a corridor may arrive. The party climbs down a ladder in the middle
/// of the start room, so its centre is plain floor; the north wall of a boss room holds the
/// gate down, with a walkable tile below it. <c>RoomTemplatesTests</c> checks all of it.
/// The north side is the one seen from the front, so templates are only mirrored left to
/// right.
/// </para>
/// <para>
/// The templates live in one file per kind of room, in <c>Templates/</c>. A template is
/// listed in the file of its first kind; a combat room that can also stand empty carries
/// both kinds.
/// </para>
/// </summary>
public static class RoomTemplates
{
    /// <summary>Every template, in a fixed order: the generator's picks depend on it.</summary>
    public static IReadOnlyList<RoomTemplate> All { get; } =
    [
        .. StartRoomTemplates.All,
        .. CombatRoomTemplates.All,
        .. TreasureRoomTemplates.All,
        .. EmptyRoomTemplates.All,
        .. BossRoomTemplates.All,
    ];
}
