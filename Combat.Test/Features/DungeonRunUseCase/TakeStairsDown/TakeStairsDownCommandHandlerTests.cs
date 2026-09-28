using Combat.Application.Features.DungeonRunUseCase.TakeStairsDown;
using Combat.Domain.Entities;
using Combat.Domain.Enums;
using Combat.Domain.Exceptions;
using Combat.Domain.Repositories;
using Combat.Domain.ValueObjects;
using Combat.Test.TestSupport;
using FluentAssertions;
using Moq;

namespace Combat.Test.Features.DungeonRunUseCase.TakeStairsDown;

public class TakeStairsDownCommandHandlerTests
{
    private readonly Mock<IDungeonRunRepository> _dungeonRunRepositoryMock = new();
    private readonly TakeStairsDownCommandHandler _handler;
    private readonly Dungeon _dungeon = DungeonTestData.Generate(
        DungeonTestData.ReferenceSeed,
        new DungeonSettings(DungeonSettings.DefaultRoomCount, 2)
    );
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
        // Arrange
        DungeonFloor firstFloor = _dungeon.Floors[0];
        Position stairs = DungeonTestData.PositionsOf(firstFloor, CellType.StairsDown).Single();
        foreach (Direction direction in DungeonTestData.FindPath(firstFloor, _run.HeroPosition, stairs)!)
        {
            _run.MoveHero(direction, _dungeon);
        }

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
}
