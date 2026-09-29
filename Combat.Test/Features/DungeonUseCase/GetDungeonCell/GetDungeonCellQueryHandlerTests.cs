using Combat.Application.Features.DungeonUseCase.GetDungeonCell;
using Combat.Domain.Entities;
using Combat.Domain.Enums;
using Combat.Domain.Repositories;
using Combat.Domain.ValueObjects;
using Combat.Test.TestSupport;
using FluentAssertions;
using Moq;

namespace Combat.Test.Features.DungeonUseCase.GetDungeonCell;

public class GetDungeonCellQueryHandlerTests
{
    private readonly Mock<IDungeonRunRepository> _dungeonRunRepositoryMock = new();
    private readonly GetDungeonCellQueryHandler _handler;
    private readonly DungeonFloor _floor;

    public GetDungeonCellQueryHandlerTests()
    {
        GeneratedDungeon dungeon = DungeonTestData.Generate(DungeonTestData.ReferenceSeed);
        _floor = dungeon.Floors[0];
        _dungeonRunRepositoryMock
            .Setup(repository =>
                repository.FindLatestBySeedAsync(
                    DungeonTestData.ReferenceSeed,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                DungeonRun.Start(Guid.NewGuid(), Guid.NewGuid(), dungeon, DateTimeOffset.UtcNow)
            );

        _handler = new GetDungeonCellQueryHandler(
            _dungeonRunRepositoryMock.Object,
            new GeneratingDungeonProvider()
        );
    }

    [Fact]
    public async Task Handle_BossTile_ReturnsTheBossAndItsRoom()
    {
        // Arrange
        DungeonElement boss = _floor.Elements.Single(element => element.Type == ElementType.Boss);

        // Act
        GetDungeonCellResult cell = await Query(boss.Position.X, boss.Position.Y);

        // Assert
        cell.Type.Should().Be("floor");
        cell.IsWalkable.Should().BeTrue();
        cell.RoomId.Should().Be(boss.RoomId);
        cell.Elements.Should().ContainSingle().Which.Type.Should().Be("boss");
    }

    [Fact]
    public async Task Handle_WallTile_ReturnsANonWalkableWallWithoutElements()
    {
        // Arrange
        Position wall = DungeonTestData.PositionsOf(_floor, CellType.Wall).First();

        // Act
        GetDungeonCellResult cell = await Query(wall.X, wall.Y);

        // Assert
        cell.Type.Should().Be("wall");
        cell.IsWalkable.Should().BeFalse();
        cell.RoomId.Should().BeNull();
        cell.Elements.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_EveryElement_IsReportedOnItsOwnTile()
    {
        foreach (DungeonElement element in _floor.Elements)
        {
            // Act
            GetDungeonCellResult cell = await Query(element.Position.X, element.Position.Y);

            // Assert: Combat and Inventory see the positions the generator decided.
            cell.Elements.Should().ContainSingle(result => result.Id == element.Id);
        }
    }

    [Fact]
    public async Task Handle_OutsideTheFloor_ThrowsKeyNotFoundException()
    {
        // Act
        Func<Task> act = async () => await Query(_floor.Width, 0);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    private Task<GetDungeonCellResult> Query(int x, int y)
    {
        return _handler.Handle(
            new GetDungeonCellQuery(DungeonTestData.ReferenceSeed.ToString(), 0, x, y),
            TestContext.Current.CancellationToken
        );
    }
}
