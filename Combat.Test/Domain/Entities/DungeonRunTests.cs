using Combat.Domain.Entities;
using Combat.Domain.Enums;
using Combat.Domain.Exceptions;
using Combat.Domain.Services.Randomness;
using Combat.Domain.ValueObjects;
using Combat.Test.TestSupport;
using FluentAssertions;

namespace Combat.Test.Domain.Entities;

/// <summary>Hero move command and walkable-tile validation.</summary>
public class DungeonRunTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Start_PlacesTheHeroOnTheEntranceOfTheFirstFloor()
    {
        // Arrange
        Dungeon dungeon = DungeonTestData.Generate(DungeonTestData.ReferenceSeed);

        // Act
        DungeonRun run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), dungeon, Now);

        // Assert
        run.Status.Should().Be(DungeonRunStatus.Active);
        run.Seed.Should().Be(DungeonTestData.ReferenceSeed);
        run.CurrentFloor.Should().Be(0);
        run.HeroPosition.Should().Be(dungeon.Floors[0].Entrance);
        run.Turn.Should().Be(0);
        run.Settings.Should().Be(DungeonSettings.Default);
    }

    [Theory]
    [InlineData(CellType.Floor)]
    [InlineData(CellType.Door)]
    [InlineData(CellType.StairsDown)]
    [InlineData(CellType.StairsUp)]
    public void MoveHero_OntoAWalkableTile_MovesOneTileAndEndsTheTurn(CellType target)
    {
        // Arrange
        Dungeon dungeon = DungeonTestData.SingleRowDungeon(CellType.Floor, target);
        DungeonRun run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), dungeon, Now);

        // Act
        Position reached = run.MoveHero(Direction.East, dungeon);

        // Assert
        reached.Should().Be(new Position(1, 0));
        run.HeroPosition.Should().Be(new Position(1, 0));
        run.Turn.Should().Be(1);
    }

    [Theory]
    [InlineData(CellType.Wall)]
    [InlineData(CellType.Obstacle)]
    [InlineData(CellType.Void)]
    public void MoveHero_OntoABlockingTile_IsRejectedAndChangesNothing(CellType target)
    {
        // Arrange
        Dungeon dungeon = DungeonTestData.SingleRowDungeon(CellType.Floor, target);
        DungeonRun run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), dungeon, Now);

        // Act
        Action act = () => run.MoveHero(Direction.East, dungeon);

        // Assert
        act.Should().Throw<InvalidMoveException>().Which.Target.Should().Be(new Position(1, 0));
        run.HeroPosition.Should().Be(new Position(0, 0));
        run.Turn.Should().Be(0);
    }

    [Theory]
    [InlineData(Direction.North)]
    [InlineData(Direction.South)]
    [InlineData(Direction.West)]
    public void MoveHero_OutOfTheFloor_IsRejected(Direction direction)
    {
        // Arrange
        Dungeon dungeon = DungeonTestData.SingleRowDungeon(CellType.Floor, CellType.Floor);
        DungeonRun run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), dungeon, Now);

        // Act
        Action act = () => run.MoveHero(direction, dungeon);

        // Assert
        act.Should().Throw<InvalidMoveException>().WithMessage("*outside the dungeon*");
        run.HeroPosition.Should().Be(new Position(0, 0));
    }

    [Fact]
    public void MoveHero_TowardsTheWallOfTheStartRoom_StopsAtTheWall()
    {
        // Arrange
        Dungeon dungeon = DungeonTestData.Generate(DungeonTestData.ReferenceSeed);
        DungeonRun run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), dungeon, Now);

        // One row above the centre: doors only sit on the centre row and column.
        run.MoveHero(Direction.North, dungeon);

        // Act
        int steps = 0;
        Action walkWest = () =>
        {
            while (true)
            {
                run.MoveHero(Direction.West, dungeon);
                steps++;
            }
        };

        // Assert
        walkWest.Should().Throw<InvalidMoveException>();
        Room start = dungeon.Floors[0].Rooms[0];
        run.HeroPosition.X.Should().Be(start.Interior.X);
        dungeon.Floors[0].GetCell(run.HeroPosition.Step(Direction.West)).Should().Be(CellType.Wall);
        run.Turn.Should().Be(1 + steps);
    }

    [Fact]
    public void MoveHero_RandomWalk_OnlyEverStandsOnWalkableTiles()
    {
        // Arrange
        Dungeon dungeon = DungeonTestData.Generate(DungeonTestData.ReferenceSeed);
        DungeonFloor floor = dungeon.Floors[0];
        DungeonRun run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), dungeon, Now);
        var random = new DeterministicRandom(99);

        for (int attempt = 0; attempt < 5_000; attempt++)
        {
            // Arrange
            var direction = (Direction)random.NextInt(4);
            Position before = run.HeroPosition;
            int turn = run.Turn;
            bool expectedToSucceed = floor.IsWalkable(before.Step(direction));

            // Act
            Action act = () => run.MoveHero(direction, dungeon);

            // Assert
            if (expectedToSucceed)
            {
                act.Should().NotThrow();
                run.HeroPosition.Should().Be(before.Step(direction));
                run.Turn.Should().Be(turn + 1);
            }
            else
            {
                act.Should().Throw<InvalidMoveException>();
                run.HeroPosition.Should().Be(before);
                run.Turn.Should().Be(turn);
            }
        }
    }

    [Fact]
    public void MoveHero_WithTheDungeonOfAnotherSeed_Throws()
    {
        // Arrange
        Dungeon dungeon = DungeonTestData.Generate(DungeonTestData.ReferenceSeed);
        Dungeon otherDungeon = DungeonTestData.Generate(new Seed(1));
        DungeonRun run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), dungeon, Now);

        // Act
        Action act = () => run.MoveHero(Direction.East, otherDungeon);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TakeStairsDown_OnTheStairs_ArrivesOnTheStairsUpOfTheNextFloor()
    {
        // Arrange
        Dungeon dungeon = DungeonTestData.Generate(
            DungeonTestData.ReferenceSeed,
            new DungeonSettings(DungeonSettings.DefaultRoomCount, 2)
        );
        DungeonFloor firstFloor = dungeon.Floors[0];
        DungeonRun run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), dungeon, Now);
        Position stairs = DungeonTestData.PositionsOf(firstFloor, CellType.StairsDown).Single();

        foreach (Direction direction in DungeonTestData.FindPath(firstFloor, run.HeroPosition, stairs)!)
        {
            run.MoveHero(direction, dungeon);
        }

        // Act
        int floor = run.TakeStairsDown(dungeon);

        // Assert
        floor.Should().Be(1);
        run.CurrentFloor.Should().Be(1);
        run.HeroPosition.Should().Be(dungeon.Floors[1].Entrance);
        dungeon.Floors[1].GetCell(run.HeroPosition).Should().Be(CellType.StairsUp);
    }

    [Fact]
    public void TakeStairsDown_AwayFromTheStairs_IsRejected()
    {
        // Arrange
        Dungeon dungeon = DungeonTestData.Generate(
            DungeonTestData.ReferenceSeed,
            new DungeonSettings(DungeonSettings.DefaultRoomCount, 2)
        );
        DungeonRun run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), dungeon, Now);

        // Act
        Action act = () => run.TakeStairsDown(dungeon);

        // Assert
        act.Should().Throw<InvalidMoveException>();
        run.CurrentFloor.Should().Be(0);
    }
}
