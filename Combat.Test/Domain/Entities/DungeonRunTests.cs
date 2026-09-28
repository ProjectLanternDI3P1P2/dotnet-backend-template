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
    [InlineData(CellType.Grate)]
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
    [InlineData(CellType.Pillar)]
    [InlineData(CellType.Fence)]
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
    public void MoveHero_TowardsTheWallOfTheStartRoom_StopsAtTheFirstBlockingTile()
    {
        // Arrange
        Dungeon dungeon = DungeonTestData.Generate(DungeonTestData.ReferenceSeed);
        DungeonRun run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), dungeon, Now);

        // One row above the arrival: doors only sit on the centre row and column.
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
        start.Interior.Contains(run.HeroPosition).Should().BeTrue();
        dungeon.Floors[0].IsWalkable(run.HeroPosition.Step(Direction.West)).Should().BeFalse();
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
            // The boss still stands: its gate stays locked.
            bool expectedToSucceed =
                floor.IsWalkable(before.Step(direction))
                && floor.GetCell(before.Step(direction)) != CellType.Gate;

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
    public void MoveHero_ThroughTheGateWhileTheBossStands_IsRejected()
    {
        // Arrange
        Dungeon dungeon = DungeonTestData.SingleRowDungeon(CellType.Floor, CellType.Gate);
        DungeonRun run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), dungeon, Now);

        // Act
        Action act = () => run.MoveHero(Direction.East, dungeon);

        // Assert
        act.Should().Throw<InvalidMoveException>().WithMessage("*boss*");
        run.HeroPosition.Should().Be(new Position(0, 0));
    }

    [Fact]
    public void DefeatFloorBoss_InTheBossRoom_OpensTheGateToTheStairs()
    {
        // Arrange
        Dungeon dungeon = DungeonTestData.Generate(DungeonTestData.ReferenceSeed);
        DungeonFloor floor = dungeon.Floors[0];
        DungeonRun run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), dungeon, Now);
        WalkTo(run, dungeon, BossOf(floor));
        Position gate = DungeonTestData.PositionsOf(floor, CellType.Gate).Single();

        // Act
        run.DefeatFloorBoss(dungeon);
        WalkTo(run, dungeon, gate);

        // Assert
        run.IsFloorBossDefeated.Should().BeTrue();
        run.HeroPosition.Should().Be(gate);
        run.Status.Should().Be(DungeonRunStatus.Active);
    }

    [Fact]
    public void DefeatFloorBoss_OutsideTheBossRoom_IsRejected()
    {
        // Arrange
        Dungeon dungeon = DungeonTestData.Generate(DungeonTestData.ReferenceSeed);
        DungeonRun run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), dungeon, Now);

        // Act
        Action act = () => run.DefeatFloorBoss(dungeon);

        // Assert
        act.Should().Throw<BossNotInReachException>();
        run.IsFloorBossDefeated.Should().BeFalse();
        run.Turn.Should().Be(0);
    }

    [Fact]
    public void DefeatFloorBoss_OnTheLastFloor_WinsTheRun()
    {
        // Arrange: a single floor, so its boss is the final boss.
        Dungeon dungeon = DungeonTestData.Generate(
            DungeonTestData.ReferenceSeed,
            new DungeonSettings(10, 1)
        );
        DungeonRun run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), dungeon, Now);
        WalkTo(run, dungeon, BossOf(dungeon.Floors[0]));

        // Act
        run.DefeatFloorBoss(dungeon);

        // Assert
        run.Status.Should().Be(DungeonRunStatus.Won);
        Action move = () => run.MoveHero(Direction.North, dungeon);
        move.Should().Throw<DungeonRunNotActiveException>();
    }

    [Fact]
    public void TakeStairsDown_OnTheStairs_ArrivesOnTheStairsUpOfTheNextFloor()
    {
        // Arrange
        Dungeon dungeon = DungeonTestData.Generate(DungeonTestData.ReferenceSeed);
        DungeonFloor firstFloor = dungeon.Floors[0];
        DungeonRun run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), dungeon, Now);
        WalkTo(run, dungeon, BossOf(firstFloor));
        run.DefeatFloorBoss(dungeon);
        WalkTo(run, dungeon, DungeonTestData.PositionsOf(firstFloor, CellType.StairsDown).First());

        // Act
        int floor = run.TakeStairsDown(dungeon);

        // Assert: the boss of the new floor still guards its gate.
        floor.Should().Be(1);
        run.CurrentFloor.Should().Be(1);
        run.HeroPosition.Should().Be(dungeon.Floors[1].Entrance);
        dungeon.Floors[1].GetCell(run.HeroPosition).Should().Be(CellType.StairsUp);
        run.IsFloorBossDefeated.Should().BeFalse();
    }

    [Fact]
    public void TakeStairsDown_AwayFromTheStairs_IsRejected()
    {
        // Arrange
        Dungeon dungeon = DungeonTestData.Generate(DungeonTestData.ReferenceSeed);
        DungeonRun run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), dungeon, Now);

        // Act
        Action act = () => run.TakeStairsDown(dungeon);

        // Assert
        act.Should().Throw<InvalidMoveException>();
        run.CurrentFloor.Should().Be(0);
    }

    private static Position BossOf(DungeonFloor floor) =>
        floor.Elements.Single(element => element.Type == ElementType.Boss).Position;

    private static void WalkTo(DungeonRun run, Dungeon dungeon, Position target)
    {
        DungeonFloor floor = dungeon.Floors[run.CurrentFloor];
        foreach (Direction direction in DungeonTestData.FindPath(floor, run.HeroPosition, target)!)
        {
            run.MoveHero(direction, dungeon);
        }
    }
}
