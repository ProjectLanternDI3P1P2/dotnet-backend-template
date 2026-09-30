using Dungeon.Domain.Entities;
using Dungeon.Domain.Enums;
using Dungeon.Domain.Exceptions;
using Dungeon.Domain.Services.Generation;
using Dungeon.Domain.ValueObjects;
using Dungeon.Test.TestSupport;
using FluentAssertions;

namespace Dungeon.Test.Domain.Services.Generation;

/// <summary>US-DUNGEON-02: the same seed always produces the same dungeon.</summary>
public class DungeonGeneratorDeterminismTests
{
    /// <summary>
    /// Golden master. This test fails when a change alters the dungeon of an existing seed.
    /// If the change is intended, bump DungeonGenerator.CurrentVersion and update this value;
    /// otherwise every stored run would silently get a different dungeon.
    /// </summary>
    private const string ReferenceFingerprint = "28a90f5640372808";

    [Theory]
    [MemberData(nameof(DungeonTestData.FiftySeeds), MemberType = typeof(DungeonTestData))]
    public void Generate_SameSeedTwice_ProducesIdenticalDungeons(ulong seedValue)
    {
        // Arrange
        Seed seed = new(seedValue);

        // Act: two generator instances, as two service replicas would do.
        GeneratedDungeon first = new DungeonGenerator().Generate(seed, DungeonSettings.Default);
        GeneratedDungeon second = new DungeonGenerator().Generate(seed, DungeonSettings.Default);

        // Assert
        DungeonTestData.Snapshot(second).Should().Be(DungeonTestData.Snapshot(first));
    }

    [Fact]
    public void Generate_SameSeed_PlacesRoomsObstaclesAndElementsAtTheSamePositions()
    {
        // Act
        DungeonFloor first = DungeonTestData.Generate(DungeonTestData.ReferenceSeed).Floors[0];
        DungeonFloor second = DungeonTestData.Generate(DungeonTestData.ReferenceSeed).Floors[0];

        // Assert
        second
            .Rooms.Select(room => (room.Id, room.Type, room.Interior, room.Center))
            .Should()
            .Equal(first.Rooms.Select(room => (room.Id, room.Type, room.Interior, room.Center)));
        second
            .Rooms.Select(room => string.Join(',', room.ConnectedRoomIds))
            .Should()
            .Equal(first.Rooms.Select(room => string.Join(',', room.ConnectedRoomIds)));
        DungeonTestData
            .PositionsOf(second, CellType.Obstacle)
            .Should()
            .Equal(DungeonTestData.PositionsOf(first, CellType.Obstacle));
        second.Elements.Should().Equal(first.Elements);
        second.Entrance.Should().Be(first.Entrance);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void Generate_SameSeedWithSeveralFloors_IsDeterministicToo(int floorCount)
    {
        // Arrange
        DungeonSettings settings = new(DungeonSettings.DefaultRoomCount, floorCount);

        // Act
        GeneratedDungeon first = DungeonTestData.Generate(DungeonTestData.ReferenceSeed, settings);
        GeneratedDungeon second = DungeonTestData.Generate(DungeonTestData.ReferenceSeed, settings);

        // Assert
        DungeonTestData.Snapshot(second).Should().Be(DungeonTestData.Snapshot(first));
    }

    [Fact]
    public void Generate_ReferenceSeed_MatchesTheGoldenMaster()
    {
        // Act
        GeneratedDungeon dungeon = DungeonTestData.Generate(DungeonTestData.ReferenceSeed);

        // Assert
        DungeonTestData.Fingerprint(dungeon).Should().Be(ReferenceFingerprint);
    }

    [Fact]
    public void Generate_DifferentSeeds_ProduceDifferentDungeons()
    {
        // Act
        List<string> fingerprints = DungeonTestData
            .SampleSeeds(200)
            .Select(seed => DungeonTestData.Fingerprint(DungeonTestData.Generate(seed)))
            .ToList();

        // Assert
        fingerprints.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Generate_LowerFloor_DoesNotDependOnTheFloorAboveIt()
    {
        // Arrange: floor 0 differs (20 or 21 rooms), floor 1 is the same (20 rooms, final).
        DungeonSettings fortyRooms = new(40, 2);
        DungeonSettings fortyOneRooms = new(41, 2);

        // Act
        GeneratedDungeon first = DungeonTestData.Generate(
            DungeonTestData.ReferenceSeed,
            fortyRooms
        );
        GeneratedDungeon second = DungeonTestData.Generate(
            DungeonTestData.ReferenceSeed,
            fortyOneRooms
        );

        // Assert: a lower floor can be generated on demand, when the party reaches it.
        DungeonTestData
            .SnapshotFloor(second.Floors[0])
            .Should()
            .NotBe(DungeonTestData.SnapshotFloor(first.Floors[0]));
        DungeonTestData
            .SnapshotFloor(second.Floors[1])
            .Should()
            .Be(DungeonTestData.SnapshotFloor(first.Floors[1]));
    }

    [Fact]
    public void Generate_UnsupportedVersion_Throws()
    {
        // Act
        Action act = () =>
            new DungeonGenerator().Generate(
                DungeonTestData.ReferenceSeed,
                DungeonSettings.Default,
                DungeonGenerator.CurrentVersion + 1
            );

        // Assert
        act.Should().Throw<UnsupportedGeneratorVersionException>();
    }
}
