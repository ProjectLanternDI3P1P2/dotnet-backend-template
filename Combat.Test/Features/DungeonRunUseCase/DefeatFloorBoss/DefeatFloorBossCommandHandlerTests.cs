using Combat.Application.Features.DungeonRunUseCase.DefeatFloorBoss;
using Combat.Domain.Entities;
using Combat.Domain.Enums;
using Combat.Domain.Exceptions;
using Combat.Domain.Repositories;
using Combat.Domain.ValueObjects;
using Combat.Test.TestSupport;
using FluentAssertions;
using Moq;

namespace Combat.Test.Features.DungeonRunUseCase.DefeatFloorBoss;

public class DefeatFloorBossCommandHandlerTests
{
    private readonly Mock<IDungeonRunRepository> _dungeonRunRepositoryMock = new();
    private readonly DefeatFloorBossCommandHandler _handler;
    private readonly GeneratedDungeon _dungeon = DungeonTestData.Generate(DungeonTestData.ReferenceSeed);
    private readonly DungeonRun _run;

    public DefeatFloorBossCommandHandlerTests()
    {
        _run = DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), _dungeon, DateTimeOffset.UtcNow);
        _dungeonRunRepositoryMock
            .Setup(repository => repository.GetByIdAsync(_run.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_run);

        _handler = new DefeatFloorBossCommandHandler(
            _dungeonRunRepositoryMock.Object,
            new GeneratingDungeonProvider()
        );
    }

    [Fact]
    public async Task Handle_HeroInTheBossRoom_OpensTheGate()
    {
        // Arrange
        DungeonFloor floor = _dungeon.Floors[0];
        Position boss = floor.Elements.Single(element => element.Type == ElementType.Boss).Position;
        foreach (Direction direction in DungeonTestData.FindPath(floor, _run.HeroPosition, boss)!)
        {
            _run.MoveHero(direction, _dungeon);
        }

        // Act
        await _handler.Handle(
            new DefeatFloorBossCommand(_run.Id),
            TestContext.Current.CancellationToken
        );

        // Assert
        _run.IsFloorBossDefeated.Should().BeTrue();
        _run.Status.Should().Be(DungeonRunStatus.Active);
    }

    [Fact]
    public async Task Handle_HeroAwayFromTheBoss_ThrowsBossNotInReachException()
    {
        // Act
        Func<Task> act = async () =>
            await _handler.Handle(
                new DefeatFloorBossCommand(_run.Id),
                TestContext.Current.CancellationToken
            );

        // Assert
        await act.Should().ThrowAsync<BossNotInReachException>();
        _run.IsFloorBossDefeated.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_UnknownRun_ThrowsKeyNotFoundException()
    {
        // Act
        Func<Task> act = async () =>
            await _handler.Handle(
                new DefeatFloorBossCommand(Guid.NewGuid()),
                TestContext.Current.CancellationToken
            );

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
