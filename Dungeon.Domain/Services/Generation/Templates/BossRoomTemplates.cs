using Dungeon.Domain.Enums;

namespace Dungeon.Domain.Services.Generation.Templates;

/// <summary>
/// The lairs of the bosses: the boss spot (<c>B</c>), and a walkable tile in the middle of
/// the first row, below the gate down in the north wall.
/// See <see cref="RoomTemplates"/> for the legend.
/// </summary>
internal static class BossRoomTemplates
{
    public static IReadOnlyList<RoomTemplate> All { get; } =
    [
        // Columns frame the gate down; the boss stands before it.
        new(
            "ThroneHall",
            [RoomType.Boss],
            [
                ".......I...I.......",
                "...................",
                ".I...............I.",
                ".........B.........",
                "...................",
                ".I...............I.",
                "...................",
                ".I...............I.",
                "...................",
                ".I...............I.",
                "...................",
                "..ooo.........ooo..",
                "...................",
            ]
        ),
        // Railings, spikes and a grated floor.
        new(
            "Arena",
            [RoomType.Boss],
            [
                ".......I...I.......",
                "...................",
                "..=====.....=====..",
                "...................",
                "...t...........t...",
                ".........B.........",
                ".....ggggggggg.....",
                "...t...........t...",
                "...................",
                "..=====.....=====..",
                "...................",
                ".I...............I.",
                "...................",
            ]
        ),
        // Two deep pits on either side of the fight.
        new(
            "PitArena",
            [RoomType.Boss],
            [
                ".......I...I.......",
                "...................",
                "..     .....     ..",
                "..     ..B..     ..",
                "..     .....     ..",
                "..     .....     ..",
                "..     .....     ..",
                "...................",
                "...................",
                ".I...............I.",
                "...................",
                "..o.............o..",
                "...................",
            ]
        ),
        // The boss waits between pools of lava, by the tombs of those who came before.
        new(
            "LavaForge",
            [RoomType.Boss],
            [
                ".......I...I.......",
                "...................",
                "..^^^^^.....^^^^^..",
                "..^^^^^.....^^^^^..",
                "..^^^^^..B..^^^^^..",
                "...................",
                "...................",
                "..TTT.........TTT..",
                "...................",
                "..^^^^^.....^^^^^..",
                "..^^^^^.....^^^^^..",
                "...................",
                "..o.............o..",
            ]
        ),
    ];
}
