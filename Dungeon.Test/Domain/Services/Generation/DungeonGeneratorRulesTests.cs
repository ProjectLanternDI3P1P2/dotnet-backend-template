using Dungeon.Domain.Entities;
using Dungeon.Domain.Enums;
using Dungeon.Domain.Services.Generation;
using Dungeon.Domain.ValueObjects;
using Dungeon.Test.TestSupport;
using FluentAssertions;

namespace Dungeon.Test.Domain.Services.Generation;

/// <summary>US-DUNGEON-01 business rules, checked on fifty different seeds.</summary>
public class DungeonGeneratorRulesTests
{
    [Theory]
    [MemberData(nameof(DungeonTestData.FiftySeeds), MemberType = typeof(DungeonTestData))]
    public void Generate_AnySeed_CreatesFortyRoomsOnFourFloorsOfTen(ulong seedValue)
    {
        // Act
        GeneratedDungeon dungeon = DungeonTestData.Generate(new Seed(seedValue));

        // Assert: the stairs rooms, which only hold the way down, are not counted.
        dungeon.RoomCount.Should().Be(40);
        dungeon.Floors.Should().HaveCount(4);
        dungeon
            .Floors.Should()
            .OnlyContain(floor => floor.Rooms.Count(room => room.Type != RoomType.Stairs) == 10);
    }

    [Theory]
    [MemberData(nameof(DungeonTestData.FiftySeeds), MemberType = typeof(DungeonTestData))]
    public void Generate_AnySeed_EveryRoomCanBeReachedFromTheEntrance(ulong seedValue)
    {
        foreach (DungeonFloor floor in DungeonTestData.Generate(new Seed(seedValue)).Floors)
        {
            // Act: walk the tiles one step at a time, as the hero does, gate open.
            HashSet<Position> reachable = DungeonTestData.ReachableFrom(floor, floor.Entrance);

            // Assert
            floor.Rooms.Should().OnlyContain(room => reachable.Contains(room.Center));
            floor.Elements.Should().OnlyContain(element => reachable.Contains(element.Position));
            reachable.Count.Should().Be(CountWalkableTiles(floor));
        }
    }

    [Theory]
    [MemberData(nameof(DungeonTestData.FiftySeeds), MemberType = typeof(DungeonTestData))]
    public void Generate_AnySeed_RoomConnectionsAreMutualAndConnected(ulong seedValue)
    {
        foreach (DungeonFloor floor in DungeonTestData.Generate(new Seed(seedValue)).Floors)
        {
            // Act
            HashSet<int> visited = [0];
            Queue<int> queue = new([0]);
            while (queue.Count > 0)
            {
                foreach (
                    int next in floor.Rooms[queue.Dequeue()].ConnectedRoomIds.Where(visited.Add)
                )
                {
                    queue.Enqueue(next);
                }
            }

            // Assert
            visited.Should().HaveCount(floor.Rooms.Count);
            floor
                .Rooms.Should()
                .OnlyContain(room =>
                    room.ConnectedRoomIds.All(id =>
                        floor.Rooms[id].ConnectedRoomIds.Contains(room.Id)
                    )
                );
        }
    }

    [Theory]
    [MemberData(nameof(DungeonTestData.FiftySeeds), MemberType = typeof(DungeonTestData))]
    public void Generate_AnySeed_EveryFloorHasABossGuardingTheWayDown(ulong seedValue)
    {
        GeneratedDungeon dungeon = DungeonTestData.Generate(new Seed(seedValue));

        foreach (DungeonFloor floor in dungeon.Floors.Where(floor => !floor.IsFinalFloor))
        {
            // Act
            Room bossRoom = floor.Rooms.Single(room => room.Type == RoomType.Boss);
            Room stairsRoom = floor.Rooms.Single(room => room.Type == RoomType.Stairs);
            DungeonElement boss = floor.Elements.Single(element =>
                element.Type == ElementType.Boss
            );
            Position gate = DungeonTestData.PositionsOf(floor, CellType.Gate).Single();
            List<Position> stairs = DungeonTestData
                .PositionsOf(floor, CellType.StairsDown)
                .ToList();

            // Assert: the stairs room lies just north of the boss room, behind the gate.
            boss.RoomId.Should().Be(bossRoom.Id);
            stairsRoom.ConnectedRoomIds.Should().Equal(bossRoom.Id);
            stairsRoom
                .GridCell.Should()
                .Be(new Position(bossRoom.GridCell.X, bossRoom.GridCell.Y - 1));
            gate.Should().Be(new Position(bossRoom.Center.X, bossRoom.Interior.Y - 1));
            stairs
                .Should()
                .NotBeEmpty()
                .And.OnlyContain(tile => floor.GetRoomId(tile) == stairsRoom.Id);
            DungeonTestData
                .ReachableFrom(floor, floor.Entrance, gateIsOpen: false)
                .Should()
                .NotIntersectWith(stairs);
        }
    }

    [Theory]
    [MemberData(nameof(DungeonTestData.FiftySeeds), MemberType = typeof(DungeonTestData))]
    public void Generate_AnySeed_EndsWithOneFinalBossAtTheEndOfTheLongestBranch(ulong seedValue)
    {
        // Arrange
        DungeonFloor floor = DungeonTestData.Generate(new Seed(seedValue)).Floors[^1];

        // Act
        Room bossRoom = floor.Rooms.Single(room => room.Type == RoomType.Boss);
        DungeonElement boss = floor.Elements.Single(element => element.Type == ElementType.Boss);
        int deepestDeadEnd = floor
            .Rooms.Where(room => room.Id != 0 && room.ConnectedRoomIds.Count == 1)
            .Max(room => room.Depth);

        // Assert
        floor.IsFinalFloor.Should().BeTrue();
        boss.RoomId.Should().Be(bossRoom.Id);
        bossRoom.ConnectedRoomIds.Should().ContainSingle();
        bossRoom.Depth.Should().Be(deepestDeadEnd);
        floor.Rooms.Should().NotContain(room => room.Type == RoomType.Stairs);
        DungeonTestData.PositionsOf(floor, CellType.StairsDown).Should().BeEmpty();
        DungeonTestData.PositionsOf(floor, CellType.Gate).Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(DungeonTestData.FiftySeeds), MemberType = typeof(DungeonTestData))]
    public void Generate_AnySeed_AlwaysUsesTheSameMixOfRooms(ulong seedValue)
    {
        foreach (DungeonFloor floor in DungeonTestData.Generate(new Seed(seedValue)).Floors)
        {
            // Act
            Dictionary<RoomType, int> counts = floor
                .Rooms.GroupBy(room => room.Type)
                .ToDictionary(group => group.Key, group => group.Count());

            // Assert: fixed quotas, so no floor is unlucky with its distribution.
            counts[RoomType.Start].Should().Be(1);
            counts[RoomType.Boss].Should().Be(1);
            counts[RoomType.Treasure].Should().Be(1);
            counts[RoomType.Empty].Should().Be(1);
            counts[RoomType.Combat].Should().Be(6);
            counts.GetValueOrDefault(RoomType.Stairs).Should().Be(floor.IsFinalFloor ? 0 : 1);
            floor.Rooms[0].Type.Should().Be(RoomType.Start);
        }
    }

    [Theory]
    [MemberData(nameof(DungeonTestData.FiftySeeds), MemberType = typeof(DungeonTestData))]
    public void Generate_AnySeed_PlacesElementsOnFreeFloorTilesOfTheirRoom(ulong seedValue)
    {
        foreach (DungeonFloor floor in DungeonTestData.Generate(new Seed(seedValue)).Floors)
        {
            // Assert
            floor.Elements.Select(element => element.Position).Should().OnlyHaveUniqueItems();
            floor
                .Elements.Should()
                .OnlyContain(element =>
                    floor.GetCell(element.Position) == CellType.Floor
                    && floor.GetRoomId(element.Position) == element.RoomId
                );
            floor
                .Rooms.Where(room => room.Type == RoomType.Combat)
                .Should()
                .OnlyContain(room =>
                    floor.Elements.Any(element =>
                        element.RoomId == room.Id && element.Type == ElementType.Enemy
                    )
                );
        }
    }

    [Theory]
    [MemberData(nameof(DungeonTestData.FiftySeeds), MemberType = typeof(DungeonTestData))]
    public void Generate_AnySeed_NeverRepeatsARoomOnAFloor(ulong seedValue)
    {
        foreach (DungeonFloor floor in DungeonTestData.Generate(new Seed(seedValue)).Floors)
        {
            // Act: what each room is made of, tile by tile.
            List<string> layouts = floor.Rooms.Select(room => Layout(floor, room)).ToList();

            // Assert
            layouts.Should().OnlyHaveUniqueItems();
        }
    }

    [Fact]
    public void Generate_FiftySeeds_UsesEveryKindOfRoomFeature()
    {
        // Act
        List<DungeonFloor> floors = DungeonTestData
            .SampleSeeds(50)
            .SelectMany(seed => DungeonTestData.Generate(seed).Floors)
            .ToList();

        // Assert: columns, railings, grates, spikes, pits and inner walls all show up.
        floors
            .Should()
            .Contain(floor => DungeonTestData.PositionsOf(floor, CellType.Pillar).Any());
        floors.Should().Contain(floor => DungeonTestData.PositionsOf(floor, CellType.Fence).Any());
        floors.Should().Contain(floor => DungeonTestData.PositionsOf(floor, CellType.Grate).Any());
        floors
            .Should()
            .Contain(floor => floor.Elements.Any(element => element.Type == ElementType.Trap));
        floors
            .Should()
            .Contain(floor => floor.Rooms.Any(room => Holds(floor, room, CellType.Void)));
        floors
            .Should()
            .Contain(floor => floor.Rooms.Any(room => Holds(floor, room, CellType.Wall)));
        floors
            .SelectMany(floor => floor.Rooms)
            .Select(room => (room.Interior.Width, room.Interior.Height))
            .Distinct()
            .Should()
            .HaveCountGreaterThan(10);
    }

    [Fact]
    public void Generate_FiveHundredSeeds_NeverBreaksABusinessRule()
    {
        foreach (Seed seed in DungeonTestData.SampleSeeds(500, origin: 1))
        {
            DungeonValidator.Validate(DungeonTestData.Generate(seed)).Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void Generate_OtherFloorCounts_SplitTheFortyRoomsAndLinkFloorsWithStairs(int floorCount)
    {
        // Act
        GeneratedDungeon dungeon = DungeonTestData.Generate(
            DungeonTestData.ReferenceSeed,
            new DungeonSettings(DungeonSettings.DefaultRoomCount, floorCount)
        );

        // Assert
        dungeon.Floors.Should().HaveCount(floorCount);
        dungeon.RoomCount.Should().Be(40);
        dungeon
            .Floors.Should()
            .OnlyContain(floor =>
                floor.Elements.Count(element => element.Type == ElementType.Boss) == 1
            );

        foreach (DungeonFloor floor in dungeon.Floors.Take(floorCount - 1))
        {
            DungeonTestData.PositionsOf(floor, CellType.StairsDown).Should().NotBeEmpty();
            DungeonTestData.PositionsOf(floor, CellType.Gate).Should().ContainSingle();
        }

        foreach (DungeonFloor floor in dungeon.Floors.Skip(1))
        {
            floor.GetCell(floor.Entrance).Should().Be(CellType.StairsUp);
        }
    }

    /// <summary>The tiles of a room, row by row: two rooms look alike when these are equal.</summary>
    private static string Layout(DungeonFloor floor, Room room)
    {
        IEnumerable<char> tiles =
            from y in Enumerable.Range(room.Interior.Y, room.Interior.Height)
            from x in Enumerable.Range(room.Interior.X, room.Interior.Width)
            select (char)('a' + (int)floor.GetCell(new Position(x, y)));

        return $"{room.Interior.Width}x{room.Interior.Height}:{new string(tiles.ToArray())}";
    }

    private static bool Holds(DungeonFloor floor, Room room, CellType cellType)
    {
        for (int y = room.Interior.Y; y <= room.Interior.Bottom; y++)
        {
            for (int x = room.Interior.X; x <= room.Interior.Right; x++)
            {
                if (floor.GetCell(new Position(x, y)) == cellType)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static int CountWalkableTiles(DungeonFloor floor)
    {
        int count = 0;
        for (int y = 0; y < floor.Height; y++)
        {
            for (int x = 0; x < floor.Width; x++)
            {
                count += floor.IsWalkable(new Position(x, y)) ? 1 : 0;
            }
        }

        return count;
    }
}
