using Dungeon.Domain.Enums;

namespace Dungeon.Domain.Services.Generation.Templates;

/// <summary>
/// The start rooms: the party climbs down a ladder in the middle, on plain floor.
/// See <see cref="RoomTemplates"/> for the legend.
/// </summary>
internal static class StartRoomTemplates
{
    public static IReadOnlyList<RoomTemplate> All { get; } =
    [
        // Two pillars on the north wall, barrels by the walls.
        new(
            "Antechamber",
            [RoomType.Start],
            [
                "o...I.I...o",
                "...........",
                "...........",
                "...........",
                "...........",
                "...........",
                "oo.......oo",
            ]
        ),
        // Cut corners and a pillar pair on the north wall.
        new(
            "Octagon",
            [RoomType.Start],
            [
                "  ..I...I..  ",
                " ........... ",
                ".............",
                ".............",
                ".............",
                ".............",
                ".............",
                " ........... ",
                "  o.......o  ",
            ]
        ),
        // Four pillars around the ladder.
        new(
            "Crossing",
            [RoomType.Start],
            [
                ".o.....o.",
                ".........",
                "..I...I..",
                ".........",
                "..I...I..",
                ".........",
                ".o.....o.",
            ]
        ),
    ];
}
