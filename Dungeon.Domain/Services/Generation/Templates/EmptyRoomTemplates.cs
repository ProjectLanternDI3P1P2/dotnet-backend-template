using Dungeon.Domain.Enums;

namespace Dungeon.Domain.Services.Generation.Templates;

/// <summary>
/// The rooms that are never more than empty: storerooms, wells, galleries. Combat rooms
/// tagged <see cref="RoomType.Empty"/> as well are found with the combat rooms.
/// See <see cref="RoomTemplates"/> for the legend.
/// </summary>
internal static class EmptyRoomTemplates
{
    public static IReadOnlyList<RoomTemplate> All { get; } =
    [
        // A grated well in the middle of a quiet room.
        new(
            "Well",
            [RoomType.Empty],
            [
                ".............",
                ".o.........o.",
                ".............",
                "....ggggg....",
                "....ggggg....",
                "....ggggg....",
                ".............",
                ".o.........o.",
                ".............",
            ]
        ),
        // Barrels and pots piled in the corners.
        new(
            "Storeroom",
            [RoomType.Empty],
            [
                "ooo.....ooo",
                "o.........o",
                "...........",
                "...........",
                "...........",
                "o.........o",
                "oo.......oo",
            ]
        ),
        // A long hall lined with columns.
        new(
            "Gallery",
            [RoomType.Empty],
            [
                ".I...I.....I...I.",
                ".................",
                ".................",
                ".................",
                ".................",
                ".................",
                ".I...I.....I...I.",
            ]
        ),
        // A cistern: still water all around a dry cross.
        new(
            "Cistern",
            [RoomType.Empty],
            [
                "...............",
                "...............",
                "..~~~~~.~~~~~..",
                "..~~~~~.~~~~~..",
                "..~~~~~.~~~~~..",
                "...............",
                "..~~~~~.~~~~~..",
                "..~~~~~.~~~~~..",
                "..~~~~~.~~~~~..",
                "...............",
                "...............",
            ]
        ),
        // A small storeroom crowded with barrels.
        new(
            "Storecloset",
            [RoomType.Empty],
            ["oo...oo", "o.....o", ".......", "o.....o", "oo...oo"]
        ),
        // A round chamber over the sewers.
        new(
            "Rotunda",
            [RoomType.Empty],
            [
                "      .      ",
                "     ...     ",
                "    .....    ",
                "  .........  ",
                " ........... ",
                "....g...g....",
                " ........... ",
                "  .........  ",
                "    .....    ",
                "     ...     ",
                "      .      ",
            ]
        ),
        // An ossuary: tombs in rows.
        new(
            "Ossuary",
            [RoomType.Empty],
            [
                "...........",
                ".TT.....TT.",
                "...........",
                ".TTT...TTT.",
                "...........",
                ".TT.....TT.",
                "...........",
            ],
            RoomRarity.Uncommon
        ),
    ];
}
