using Combat.Domain.Entities;
using Combat.Domain.Enums;
using Combat.Domain.Services.Generation;
using Combat.Domain.ValueObjects;
using Combat.Test.TestSupport;
using FluentAssertions;

namespace Combat.Test.Domain.Services.Generation;

/// <summary>US-DUNGEON-01 business rules, checked on fifty different seeds.</summary>
public class DungeonGeneratorRulesTests
{
    [Theory]
    [MemberData(nameof(DungeonTestData.FiftySeeds), MemberType = typeof(DungeonTestData))]
    public void Generate_AnySeed_CreatesExactlyFortyRooms(ulong seedValue)
    {
        // Act
        Dungeon dungeon = DungeonTestData.Generate(new Seed(seedValue));

        // Assert
        dungeon.Floors.Should().ContainSingle();
        dungeon.Floors[0].Rooms.Should().HaveCount(40);
    }

    [Theory]
    [MemberData(nameof(DungeonTestData.FiftySeeds), MemberType = typeof(DungeonTestData))]
    public void Generate_AnySeed_EveryRoomCanBeReachedFromTheEntrance(ulong seedValue)
    {
        // Arrange
        DungeonFloor floor = DungeonTestData.Generate(new Seed(seedValue)).Floors[0];

        // Act: walk the tiles one step at a time, as the hero does.
        HashSet<Position> reachable = DungeonTestData.ReachableFrom(floor, floor.Entrance);

        // Assert
        floor.Rooms.Should().OnlyContain(room => reachable.Contains(room.Center));
        floor.Elements.Should().OnlyContain(element => reachable.Contains(element.Position));
        reachable.Count.Should().Be(CountWalkableTiles(floor));
    }

    [Theory]
    [MemberData(nameof(DungeonTestData.FiftySeeds), MemberType = typeof(DungeonTestData))]
    public void Generate_AnySeed_RoomConnectionsAreMutualAndConnected(ulong seedValue)
    {
        // Arrange
        DungeonFloor floor = DungeonTestData.Generate(new Seed(seedValue)).Floors[0];

        // Act
        HashSet<int> visited = [0];
        Queue<int> queue = new([0]);
        while (queue.Count > 0)
        {
            foreach (int next in floor.Rooms[queue.Dequeue()].ConnectedRoomIds.Where(visited.Add))
            {
                queue.Enqueue(next);
            }
        }

        // Assert
        visited.Should().HaveCount(floor.Rooms.Count);
        floor
            .Rooms.Should()
            .OnlyContain(room =>
                room.ConnectedRoomIds.All(id => floor.Rooms[id].ConnectedRoomIds.Contains(room.Id))
            );
    }

    [Theory]
    [MemberData(nameof(DungeonTestData.FiftySeeds), MemberType = typeof(DungeonTestData))]
    public void Generate_AnySeed_HasExactlyOneFinalBossAtTheEndOfTheLongestBranch(ulong seedValue)
    {
        // Arrange
        DungeonFloor floor = DungeonTestData.Generate(new Seed(seedValue)).Floors[0];

        // Act
        Room bossRoom = floor.Rooms.Single(room => room.Type == RoomType.Boss);
        DungeonElement boss = floor.Elements.Single(element => element.Type == ElementType.Boss);
        int deepestDeadEnd = floor
            .Rooms.Where(room => room.Id != 0 && room.ConnectedRoomIds.Count == 1)
            .Max(room => room.Depth);

        // Assert
        boss.RoomId.Should().Be(bossRoom.Id);
        boss.Position.Should().Be(bossRoom.Center);
        bossRoom.ConnectedRoomIds.Should().ContainSingle();
        bossRoom.Depth.Should().Be(deepestDeadEnd);
    }

    [Theory]
    [MemberData(nameof(DungeonTestData.FiftySeeds), MemberType = typeof(DungeonTestData))]
    public void Generate_AnySeed_AlwaysUsesTheSameMixOfRooms(ulong seedValue)
    {
        // Act
        DungeonFloor floor = DungeonTestData.Generate(new Seed(seedValue)).Floors[0];

        // Assert: fixed quotas, so no dungeon is unlucky with its distribution.
        Dictionary<RoomType, int> counts = floor
            .Rooms.GroupBy(room => room.Type)
            .ToDictionary(group => group.Key, group => group.Count());
        counts[RoomType.Start].Should().Be(1);
        counts[RoomType.Boss].Should().Be(1);
        counts[RoomType.Treasure].Should().Be(4);
        counts[RoomType.Empty].Should().Be(6);
        counts[RoomType.Combat].Should().Be(28);
        floor.Rooms[0].Type.Should().Be(RoomType.Start);
    }

    [Theory]
    [MemberData(nameof(DungeonTestData.FiftySeeds), MemberType = typeof(DungeonTestData))]
    public void Generate_AnySeed_PlacesElementsOnFreeFloorTilesOfTheirRoom(ulong seedValue)
    {
        // Act
        DungeonFloor floor = DungeonTestData.Generate(new Seed(seedValue)).Floors[0];

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

    [Fact]
    public void Generate_ThousandSeeds_NeverBreaksABusinessRule()
    {
        foreach (Seed seed in DungeonTestData.SampleSeeds(1000, origin: 1))
        {
            DungeonValidator.Validate(DungeonTestData.Generate(seed)).Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void Generate_SeveralFloors_SplitsTheFortyRoomsAndLinksFloorsWithStairs(int floorCount)
    {
        // Act
        Dungeon dungeon = DungeonTestData.Generate(
            DungeonTestData.ReferenceSeed,
            new DungeonSettings(DungeonSettings.DefaultRoomCount, floorCount)
        );

        // Assert
        dungeon.Floors.Should().HaveCount(floorCount);
        dungeon.TotalRoomCount.Should().Be(40);
        dungeon
            .Floors.SelectMany(floor => floor.Elements)
            .Count(element => element.Type == ElementType.Boss)
            .Should()
            .Be(1);
        dungeon.Floors[^1].Rooms.Should().ContainSingle(room => room.Type == RoomType.Boss);

        foreach (DungeonFloor floor in dungeon.Floors.Take(floorCount - 1))
        {
            DungeonTestData.PositionsOf(floor, CellType.StairsDown).Should().ContainSingle();
        }

        foreach (DungeonFloor floor in dungeon.Floors.Skip(1))
        {
            floor.GetCell(floor.Entrance).Should().Be(CellType.StairsUp);
        }
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
