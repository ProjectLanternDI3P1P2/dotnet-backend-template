using Dungeon.Domain.Enums;

namespace Dungeon.Domain.Services.Generation;

/// <summary>
/// The hand-drawn rooms of the dungeon, inspired by the example maps of the tileset. The
/// generator picks one per room, mirrored or not, so that no two rooms of a floor look alike.
/// <para>
/// Legend: <c>.</c> floor, <c>#</c> wall, space: void (a pit), <c>o</c> barrel or jar,
/// <c>I</c> column, <c>=</c> railing, <c>g</c> sewer grate (walkable), <c>e</c> enemy spot,
/// <c>t</c> spike trap, <c>$</c> treasure spot, <c>B</c> boss, <c>&gt;</c> stairs down,
/// <c>&lt;</c> stairs up (the arrival point of the floor).
/// </para>
/// <para>
/// Every template has odd dimensions, a walkable centre, and a walkable tile in the middle
/// of each side, where a corridor may arrive: <c>RoomTemplatesTests</c> checks all of it.
/// The north side is the one seen from the front, so templates are only mirrored left to
/// right.
/// </para>
/// </summary>
public static class RoomTemplates
{
    public static IReadOnlyList<RoomTemplate> All { get; } =
    [
        // Two pillars frame the way in, barrels by the walls.
        new(
            "Antechamber",
            [RoomType.Start],
            [
                "o...I.I...o",
                "...........",
                "...........",
                ".....<.....",
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
                "......<......",
                ".............",
                ".............",
                " ........... ",
                "  o.......o  ",
            ]
        ),
        // Four pillars around the arrival.
        new(
            "Crossing",
            [RoomType.Start],
            [
                ".o.....o.",
                ".........",
                "..I...I..",
                "....<....",
                "..I...I..",
                ".........",
                ".o.....o.",
            ]
        ),
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
        // A wall splits off a side chamber, open on the middle row.
        new(
            "SideChamber",
            [RoomType.Combat],
            [
                ".....#...........",
                ".e...#.......e...",
                ".....#...........",
                ".....#.....ooo...",
                ".....#...........",
                ".................",
                "...e.#.......e...",
                ".....#...........",
                ".....#...........",
                ".o...#.......t...",
                ".....#.......t...",
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
        // Two walled galleries on the sides, entered from the middle.
        new(
            "Galleries",
            [RoomType.Combat],
            [
                ".................",
                ".###.........###.",
                ".#e#....e....#e#.",
                ".#.#.........#.#.",
                ".#.#...ooo...#.#.",
                ".................",
                ".#.#.........#.#.",
                ".#.#....e....#.#.",
                ".#e#.........#e#.",
                ".###.........###.",
                ".................",
            ]
        ),
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
        // Columns frame the gate to the stairs; the boss stands before it.
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
        // Wide stairs down against the north wall, between two columns.
        new(
            "Descent",
            [RoomType.Stairs],
            [
                "...I>>>I...",
                "...........",
                "...........",
                "...........",
                "...........",
                ".o.......o.",
                "...........",
            ]
        ),
        // Stairs down at the far end, behind railings, as on a landing.
        new(
            "Landing",
            [RoomType.Stairs],
            [
                "...........",
                ".I.......I.",
                "...........",
                "...........",
                "...........",
                ".I.......I.",
                "====>>>====",
            ]
        ),
        // Stairs down in the middle of four columns.
        new(
            "WellStairs",
            [RoomType.Stairs],
            [
                ".........",
                ".I.....I.",
                ".........",
                "....>....",
                ".........",
                ".I.....I.",
                ".........",
            ]
        ),
    ];
}
