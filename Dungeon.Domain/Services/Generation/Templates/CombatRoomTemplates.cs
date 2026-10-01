using Dungeon.Domain.Enums;

namespace Dungeon.Domain.Services.Generation.Templates;

/// <summary>
/// The rooms where enemies wait on their spots (<c>e</c>). Those also listed as
/// <see cref="RoomType.Empty"/> serve as empty rooms too: their spots are then left unused.
/// See <see cref="RoomTemplates"/> for the legend.
/// </summary>
internal static class CombatRoomTemplates
{
    public static IReadOnlyList<RoomTemplate> All { get; } =
    [
        // Two rows of columns along the long walls.
        new(
            "Colonnade",
            [RoomType.Combat],
            [
                "...............",
                "..I..I...I..I..",
                ".e...........e.",
                "...............",
                "......e.e......",
                "...............",
                ".e...........e.",
                "..I..I...I..I..",
                "...............",
            ]
        ),
        // Two walled pits; the way across is the middle aisle.
        new(
            "TwinPits",
            [RoomType.Combat, RoomType.Empty],
            [
                ".................",
                ".e.............e.",
                "..     ...     ..",
                "..     ...     ..",
                "..     ...     ..",
                "..     ...     ..",
                "..     ...     ..",
                ".......t.t.......",
                ".e.............e.",
                ".................",
                ".................",
            ]
        ),
        // Cross-shaped hall, jars in the arms.
        new(
            "Cross",
            [RoomType.Combat],
            [
                "    .......    ",
                "    ..o.o..    ",
                "    ...e...    ",
                "...............",
                ".e...........e.",
                "...............",
                ".e...........e.",
                "...............",
                "    ...e...    ",
                "    ..o.o..    ",
                "    .......    ",
            ]
        ),
        // A colonnade splits off a side chamber. Walls one tile thick read as flat slabs in
        // three-quarter view: columns divide the room instead.
        new(
            "SideChamber",
            [RoomType.Combat],
            [
                ".....I...........",
                ".e...........e...",
                ".....I...........",
                "...........ooo...",
                ".....I...........",
                ".................",
                "...e.I.......e...",
                ".................",
                ".....I...........",
                ".o...........t...",
                ".....I.......t...",
            ]
        ),
        // A thick wall closes off a northern cell with a single passage.
        new(
            "NorthCell",
            [RoomType.Combat, RoomType.Empty],
            [
                "...............",
                ".o...........o.",
                "...e.......e...",
                "#######.#######",
                "#######.#######",
                "...............",
                ".e...........e.",
                "...............",
                "......t.t......",
                "...............",
                "oo...........oo",
            ]
        ),
        // Spike traps guard both sides of the room.
        new(
            "SpikeLanes",
            [RoomType.Combat],
            [
                ".................",
                ".e.............e.",
                ".................",
                "..ttt.......ttt..",
                "......e...e......",
                "..ttt.......ttt..",
                ".................",
                ".e.............e.",
                ".................",
            ]
        ),
        // Sewer grates in the middle, four columns around them.
        new(
            "GrateChamber",
            [RoomType.Combat],
            [
                "...............",
                ".e...........e.",
                "...............",
                "...I.......I...",
                "...............",
                ".....ggggg.....",
                ".....ggggg.....",
                ".....ggggg.....",
                "...............",
                "...I.......I...",
                "...............",
                ".e...........e.",
                "...............",
            ]
        ),
        // Supplies stacked along the walls.
        new(
            "Barracks",
            [RoomType.Combat],
            [
                "oo.o.....o.oo",
                ".............",
                ".e.........e.",
                ".............",
                ".....e.e.....",
                ".............",
                ".e.........e.",
                ".............",
                "oo.o.....o.oo",
            ]
        ),
        // The south side is a railing over the void.
        new(
            "Balcony",
            [RoomType.Combat],
            [
                "...............",
                ".I...........I.",
                "...e.......e...",
                "...............",
                "......e.e......",
                "...............",
                "...e.......e...",
                "...............",
                "======...======",
            ]
        ),
        // A small room, two ambushers and a pair of spikes.
        new(
            "Closet",
            [RoomType.Combat],
            [
                "o.......o",
                ".........",
                "..e...e..",
                ".........",
                "...t.t...",
                ".........",
                "o.......o",
            ]
        ),
        // L-shaped room: one quarter has caved in.
        new(
            "Corner",
            [RoomType.Combat],
            [
                ".............",
                ".e.........e.",
                ".............",
                ".....e.......",
                ".............",
                ".............",
                ".e......     ",
                "........     ",
                "..o.....     ",
            ]
        ),
        // A forest of columns.
        new(
            "Hypostyle",
            [RoomType.Combat],
            [
                ".................",
                ".I...I.....I...I.",
                ".................",
                "...e.........e...",
                ".I...I.....I...I.",
                "......e...e......",
                ".I...I.....I...I.",
                "...e.........e...",
                ".................",
                ".I...I.....I...I.",
                ".................",
            ]
        ),
        // Four little chapels of columns on the sides, a guard in each.
        new(
            "Galleries",
            [RoomType.Combat],
            [
                ".................",
                ".I.I.........I.I.",
                "..e.....e.....e..",
                ".I.I.........I.I.",
                ".......ooo.......",
                ".................",
                ".................",
                ".I.I.........I.I.",
                "..e.....e.....e..",
                ".I.I.........I.I.",
                ".................",
            ]
        ),
        // Four pools of still water; the fight goes on around them.
        new(
            "FloodedHall",
            [RoomType.Combat, RoomType.Empty],
            [
                ".................",
                ".e.............e.",
                "..~~~~.....~~~~..",
                "..~~~~.....~~~~..",
                "..~~~~..e..~~~~..",
                ".................",
                "..~~~~.....~~~~..",
                "..~~~~.....~~~~..",
                "..~~~~.....~~~~..",
                ".e.............e.",
                ".................",
            ]
        ),
        // Rows of tombs; something stirs between them.
        new(
            "Crypt",
            [RoomType.Combat, RoomType.Empty],
            [
                "...............",
                ".TTT.TT.TT.TTT.",
                "...............",
                "...e.......e...",
                "...............",
                ".......e.......",
                ".TTT.TT.TT.TTT.",
                "...............",
                "..o.........o..",
            ]
        ),
        // A labyrinth of thick walls; its dead ends hide the guards.
        new(
            "Labyrinth",
            [RoomType.Combat, RoomType.Empty],
            [
                "e##e...............",
                ".#####.########.##.",
                ".#####.########.##.",
                "....##......e##....",
                ".##.##.###########.",
                ".##.##.###########.",
                ".##.##e##....##....",
                ".##.#####.##.##.##.",
                ".##.#####.##.##.##.",
                ".##.......##....##.",
                ".##.##############.",
                ".##.##############.",
                "...................",
            ],
            RoomRarity.Rare
        ),
        // A much larger labyrinth, rarer still, with traps in its turns.
        new(
            "DeepLabyrinth",
            [RoomType.Combat],
            [
                "e##e............##.......",
                ".#####.########.##.#####.",
                ".#####.########.##.#####.",
                ".##....##.......##...e##.",
                ".##.#####.##############.",
                ".##.#####.##############.",
                ".##....##................",
                ".#####.###########.##.##.",
                ".#####.###########.##.##.",
                ".......##.............##.",
                "######.##.#####.##.##.##.",
                "######.##.#####.##.##.##.",
                "e......##.##....##....##.",
                "###.##.##.##.########.##.",
                "###.##.##.##.########.##.",
                "....##e##.##.......##e##.",
                ".########.########.#####.",
                ".########.########.#####.",
                ".........................",
            ],
            RoomRarity.Rare
        ),
        // A hall shaped as a cross, columns at its heart.
        new(
            "CrossHall",
            [RoomType.Combat],
            [
                "       .......       ",
                "       .......       ",
                "       ...e...       ",
                "       .......       ",
                "       .......       ",
                "........I...I........",
                ".....................",
                "...e.............e...",
                ".....................",
                "........I...I........",
                "       .......       ",
                "       .......       ",
                "       ...e...       ",
                "       .......       ",
                "       .......       ",
            ]
        ),
        // A room cut as a diamond: narrow at the ends, wide in the middle.
        new(
            "Diamond",
            [RoomType.Combat, RoomType.Empty],
            [
                "        .        ",
                "       ...       ",
                "     .......     ",
                "    ....e....    ",
                "   ...........   ",
                " ............... ",
                ".....e.....e.....",
                " ............... ",
                "   ...........   ",
                "    ....t....    ",
                "     .......     ",
                "       ...       ",
                "        .        ",
            ],
            RoomRarity.Uncommon
        ),
        // Two moats split the hall; causeways cross them.
        new(
            "MoatHall",
            [RoomType.Combat, RoomType.Empty],
            [
                "...................",
                ".e...............e.",
                "...................",
                "...~~~~~~.~~~~~~...",
                "...~~~~~~.~~~~~~...",
                "...~~~~~~.~~~~~~...",
                "...................",
                "...~~~~~~.~~~~~~...",
                "...~~~~~~.~~~~~~...",
                "...~~~~~~.~~~~~~...",
                "...................",
                ".o...............e.",
                "...................",
            ],
            RoomRarity.Uncommon
        ),
        // Thick walls from either side: the way snakes through the room.
        new(
            "Serpent",
            [RoomType.Combat],
            [
                "...................",
                ".e.................",
                "##########.........",
                "##########.........",
                ".................e.",
                "...................",
                ".e.................",
                ".........##########",
                ".........##########",
                ".................e.",
                "...................",
            ],
            RoomRarity.Uncommon
        ),
        // A forest of columns, open along its two middle lines.
        new(
            "PillarForest",
            [RoomType.Combat],
            [
                ".....................",
                ".I..I..I.....I..I..I.",
                ".....................",
                ".I..I..I.....I..I..I.",
                "....e...........e....",
                ".I..I..I.....I..I..I.",
                ".....................",
                ".....................",
                ".....................",
                ".I..I..I.....I..I..I.",
                "....e...........e....",
                ".I..I..I.....I..I..I.",
                ".....................",
                ".I..I..I.....I..I..I.",
                ".....................",
            ],
            RoomRarity.Uncommon
        ),
        // A small guardroom.
        new(
            "Guardroom",
            [RoomType.Combat],
            ["o.....o", ".e...e.", ".......", ".......", "o.....o"]
        ),
        // A long hall lined with columns.
        new(
            "LongHall",
            [RoomType.Combat, RoomType.Empty],
            [
                ".........................",
                ".I...I...I...I...I...I...",
                ".e.......................",
                ".............e...........",
                "........................e",
                "...I...I...I...I...I...I.",
                ".........................",
            ]
        ),
        // Rows of tombs with alleys between them.
        new(
            "Catacombs",
            [RoomType.Combat, RoomType.Empty],
            [
                "...................",
                ".TTT.TT.TTT.TT.TTT.",
                "...................",
                ".TT.TTT.....TTT.TT.",
                "..e.............e..",
                ".TTT.TT..e..TT.TTT.",
                "...................",
                ".TT.TTT.TTT.TTT.TT.",
                "...................",
            ],
            RoomRarity.Uncommon
        ),
        // A flooded hall: causeways meet on an island in the middle.
        new(
            "SunkenBridges",
            [RoomType.Combat],
            [
                ".....................",
                ".~~~~~~~~~.~~~~~~~~~.",
                ".~~~~~~~~~.~~~~~~~~~.",
                ".~~~~~~~~~.~~~~~~~~~.",
                ".~~~~~~.......~~~~~~.",
                ".~~~~~~.e.....~~~~~~.",
                "...t.............t...",
                ".~~~~~~.....e.~~~~~~.",
                ".~~~~~~.......~~~~~~.",
                ".~~~~~~~~~.~~~~~~~~~.",
                ".~~~~~~~~~.~~~~~~~~~.",
                ".~~~~~~~~~.~~~~~~~~~.",
                ".....................",
            ],
            RoomRarity.Rare
        ),
        // Two wings joined by a broad hall and a passage through the middle.
        new(
            "TwinWings",
            [RoomType.Combat],
            [
                ".....  ...  .....",
                ".e...  ...  ..e..",
                ".....  ...  .....",
                ".................",
                "..o....e....o....",
                ".................",
                ".....  ...  .....",
                "..e..  ...  ...e.",
                ".....  ...  .....",
            ]
        ),
        // Square pillars form alcoves along the north and south walls.
        new(
            "Alcoves",
            [RoomType.Combat, RoomType.Empty],
            [
                ".....................",
                ".##..##..##.##..##..#",
                ".##..##..##.##..##..#",
                ".....................",
                "..e...............e..",
                ".....................",
                "..e...............e..",
                ".....................",
                ".##..##..##.##..##..#",
                ".##..##..##.##..##..#",
                ".....................",
            ]
        ),
    ];
}
