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
    ];
}
