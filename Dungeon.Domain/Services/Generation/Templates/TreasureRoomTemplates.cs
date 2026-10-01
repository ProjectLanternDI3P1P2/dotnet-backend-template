using Dungeon.Domain.Enums;

namespace Dungeon.Domain.Services.Generation.Templates;

/// <summary>
/// The treasure rooms: one treasure spot (<c>$</c>) or more, often guarded by a fence.
/// See <see cref="RoomTemplates"/> for the legend.
/// </summary>
internal static class TreasureRoomTemplates
{
    public static IReadOnlyList<RoomTemplate> All { get; } =
    [
        // The treasure waits in an iron cage open to the south.
        new(
            "Vault",
            [RoomType.Treasure],
            [
                "...........",
                ".o.......o.",
                "...=====...",
                "...=...=...",
                "...=.$.=...",
                "...=...=...",
                "...==.==...",
                "...........",
                "o.........o",
            ]
        ),
        // Columns and jars around the treasure.
        new(
            "Shrine",
            [RoomType.Treasure],
            [
                "..I.....I..",
                "...........",
                "...o...o...",
                ".....$.....",
                "...o...o...",
                "...........",
                "..I.....I..",
            ]
        ),
        // The treasure sits behind a ring of spikes.
        new(
            "Hoard",
            [RoomType.Treasure],
            [
                ".............",
                ".oo.......oo.",
                ".............",
                "....t...t....",
                "......$......",
                "....t...t....",
                ".............",
                ".o.........o.",
                ".............",
            ]
        ),
        // A labyrinth whose dead ends hold the treasure, and traps.
        new(
            "TreasureMaze",
            [RoomType.Treasure],
            [
                "t##.......##t............",
                ".##.##.##.##############.",
                ".##.##.##.##############.",
                ".......##.##....##.......",
                "#########.##.##.##.##.##.",
                "#########.##.##.##.##.##.",
                ".......##....##.##....##.",
                ".#####.########.#####.##.",
                ".#####.########.#####.##.",
                "....##.......##.......##.",
                "###.#####.##.###########.",
                "###.#####.##.###########.",
                "t##....##.##...$##....##.",
                ".##.##.##.########.##.##.",
                ".##.##.##.########.##.##.",
                "....##.##.##.......##....",
                ".#####.##.##.##.##.######",
                ".#####.##.##.##.##.######",
                ".......##....##.........$",
            ],
            RoomRarity.Rare
        ),
        // The treasure waits on an island, reached by a single causeway.
        new(
            "IslandHoard",
            [RoomType.Treasure],
            [
                ".................",
                ".................",
                ".................",
                "...~~~~~~~~~~~...",
                "...~~~~~~~~~~~...",
                "...~~~.....~~~...",
                "...t....$..~~~...",
                "...~~~.....~~~...",
                "...~~~~~~~~~~~...",
                "...~~~~~~~~~~~...",
                ".................",
                ".................",
                ".................",
            ],
            RoomRarity.Uncommon
        ),
        // A spiral: from the outer walkway, the only way in runs once around the room
        // between thick walls, past two traps, before it opens onto the treasure chamber.
        new(
            "SpiralRoom",
            [RoomType.Treasure],
            [
                ".........................",
                ".##.####################.",
                ".##.####################.",
                ".##.##................##.",
                ".##.##.##############.##.",
                ".##.##.##############.##.",
                ".##.##.I.........I.##.##.",
                ".##.##..TT.....TT..##.##.",
                ".##.##......$......##.##.",
                ".##.##.............##t##.",
                ".##.##...$.....$...##.##.",
                ".##.##..TT.....TT..##.##.",
                ".##.##.I.........I.##.##.",
                ".##.#################.##.",
                ".##.#################.##.",
                ".##.........t.........##.",
                ".#######################.",
                ".#######################.",
                ".........................",
            ],
            RoomRarity.Uncommon
        ),
        // A small reliquary between four tombs.
        new(
            "Reliquary",
            [RoomType.Treasure],
            [".........", ".TT...TT.", "....$....", ".TT...TT.", "........."]
        ),
        // Two fenced hoards on either side of the hall.
        new(
            "GildedHall",
            [RoomType.Treasure],
            [
                "...................",
                ".I.=====...=====.I.",
                "...=$..=...=..$=...",
                "...=...=...=...=...",
                "...==.==...==.==...",
                ".........t.........",
                ".I...............I.",
            ]
        ),
    ];
}
