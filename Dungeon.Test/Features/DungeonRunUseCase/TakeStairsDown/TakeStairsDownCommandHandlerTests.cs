using Dungeon.Application.Features.DungeonRunUseCase.TakeStairsDown;
using Dungeon.Domain.Entities;
using Dungeon.Domain.Enums;
using Dungeon.Domain.Exceptions;
using Dungeon.Domain.Repositories;
using Dungeon.Domain.ValueObjects;
using Dungeon.Test.TestSupport;
using FluentAssertions;
using Moq;

namespace Dungeon.Test.Features.DungeonRunUseCase.TakeStairsDown;

public class TakeStairsDownCommandHandlerTests
{
    private readonly Mock<IDungeonRunRepository> _dungeonRunRepositoryMock = new();
    private readonly TakeStairsDownCommandHandler _handler;
    private readonly GeneratedDungeon _dungeon = DungeonTestData.Generate(DungeonTestData.ReferenceSeed);
    private readonly DungeonRun _run;

    public TakeStairsDownCommandHandlerTests()
    {
        _run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), _dungeon, DateTimeOffset.UtcNow);
        _dungeonRunRepositoryMock
            .Setup(repository => repository.GetByIdAsync(_run.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_run);

        _handler = new TakeStairsDownCommandHandler(
            _dungeonRunRepositoryMock.Object,
            new GeneratingDungeonProvider()
        );
    }

    [Fact]
    public async Task Handle_HeroOnTheStairs_ArrivesOnTheNextFloor()
    {
        // Arrange: beat the boss of the floor, then go through its gate to the stairs.
        DungeonFloor firstFloor = _dungeon.Floors[0];
        Position boss = firstFloor.Elements.Single(element => element.Type == ElementType.Boss).Position;
        Position stairs = DungeonTestData.PositionsOf(firstFloor, CellType.StairsDown).First();
        WalkTo(firstFloor, boss);
        _run.DefeatFloorBoss(_dungeon);
        WalkTo(firstFloor, stairs);

        // Act
        await _handler.Handle(
            new TakeStairsDownCommand(_run.Id),
            TestContext.Current.CancellationToken
        );

        // Assert
        _run.CurrentFloor.Should().Be(1);
        _run.HeroPosition.Should().Be(_dungeon.Floors[1].Entrance);
    }

    [Fact]
    public async Task Handle_HeroAwayFromTheStairs_ThrowsInvalidMoveException()
    {
        // Act
        Func<Task> act = async () =>
            await _handler.Handle(
                new TakeStairsDownCommand(_run.Id),
                TestContext.Current.CancellationToken
            );

        // Assert
        await act.Should().ThrowAsync<InvalidMoveException>();
        _run.CurrentFloor.Should().Be(0);
    }

    private void WalkTo(DungeonFloor floor, Position target)
    {
        foreach (Direction direction in DungeonTestData.FindPath(floor, _run.HeroPosition, target)!)
        {
            _run.MoveHero(direction, _dungeon);
        }
    }
}
