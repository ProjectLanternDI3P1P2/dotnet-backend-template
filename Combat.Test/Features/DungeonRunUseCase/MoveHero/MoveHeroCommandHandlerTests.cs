using Combat.Application.Features.DungeonRunUseCase.MoveHero;
using Combat.Domain.Entities;
using Combat.Domain.Enums;
using Combat.Domain.Exceptions;
using Combat.Domain.Repositories;
using Combat.Domain.ValueObjects;
using Combat.Test.TestSupport;
using FluentAssertions;
using Moq;

namespace Combat.Test.Features.DungeonRunUseCase.MoveHero;

public class MoveHeroCommandHandlerTests
{
    private readonly Mock<IDungeonRunRepository> _dungeonRunRepositoryMock = new();
    private readonly MoveHeroCommandHandler _handler;
    private readonly Dungeon _dungeon = DungeonTestData.Generate(DungeonTestData.ReferenceSeed);
    private readonly DungeonRun _run;

    public MoveHeroCommandHandlerTests()
    {
        _run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), _dungeon, DateTimeOffset.UtcNow);
        _dungeonRunRepositoryMock
            .Setup(repository => repository.GetByIdAsync(_run.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_run);

        _handler = new MoveHeroCommandHandler(
            _dungeonRunRepositoryMock.Object,
            new GeneratingDungeonProvider()
        );
    }

    [Theory]
    [InlineData("east", 1, 0)]
    [InlineData("WEST", -1, 0)]
    [InlineData("North", 0, -1)]
    [InlineData("south", 0, 1)]
    public async Task Handle_TowardsAFloorTile_MovesTheHeroOneTile(
        string direction,
        int deltaX,
        int deltaY
    )
    {
        // Arrange: every neighbour of the start room's centre is floor.
        Position entrance = _dungeon.Floors[0].Entrance;

        // Act
        await _handler.Handle(
            new MoveHeroCommand(_run.Id, direction),
            TestContext.Current.CancellationToken
        );

        // Assert
        _run.HeroPosition.Should().Be(new Position(entrance.X + deltaX, entrance.Y + deltaY));
        _run.Turn.Should().Be(1);
    }

    [Fact]
    public async Task Handle_TowardsAWall_ThrowsAndLeavesTheHeroInPlace()
    {
        // Arrange: walk to the west wall of the start room, one row above its centre.
        Room start = _dungeon.Floors[0].Rooms[0];
        _run.MoveHero(Direction.North, _dungeon);
        while (_run.HeroPosition.X > start.Interior.X)
        {
            _run.MoveHero(Direction.West, _dungeon);
        }

        Position before = _run.HeroPosition;

        // Act
        Func<Task> act = async () =>
            await _handler.Handle(
                new MoveHeroCommand(_run.Id, "west"),
                TestContext.Current.CancellationToken
            );

        // Assert
        await act.Should().ThrowAsync<InvalidMoveException>();
        _run.HeroPosition.Should().Be(before);
    }

    [Fact]
    public async Task Handle_UnknownRun_ThrowsKeyNotFoundException()
    {
        // Arrange
        var runId = Guid.NewGuid();

        // Act
        Func<Task> act = async () =>
            await _handler.Handle(
                new MoveHeroCommand(runId, "east"),
                TestContext.Current.CancellationToken
            );

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{runId}*");
    }
}
