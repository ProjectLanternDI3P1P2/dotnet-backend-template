using Combat.Application.Features.DungeonUseCase.GetDungeonMap;
using Combat.Domain.Entities;
using Combat.Domain.Repositories;
using Combat.Domain.ValueObjects;
using Combat.Test.TestSupport;
using FluentAssertions;
using Moq;

namespace Combat.Test.Features.DungeonUseCase.GetDungeonMap;

public class GetDungeonMapQueryHandlerTests
{
    private readonly Mock<IDungeonRunRepository> _dungeonRunRepositoryMock = new();

    public GetDungeonMapQueryHandlerTests()
    {
        DungeonRun run = DungeonRun.Start(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DungeonTestData.Generate(DungeonTestData.ReferenceSeed),
            DateTimeOffset.UtcNow
        );
        _dungeonRunRepositoryMock
            .Setup(repository =>
                repository.FindLatestBySeedAsync(
                    DungeonTestData.ReferenceSeed,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(run);
    }

    [Fact]
    public async Task Handle_KnownSeed_ReturnsEveryCellAndElementOfTheFloor()
    {
        // Arrange
        DungeonFloor expected = DungeonTestData.Generate(DungeonTestData.ReferenceSeed).Floors[0];

        // Act
        GetDungeonMapResult map = await CreateHandler()
            .Handle(
                new GetDungeonMapQuery(DungeonTestData.ReferenceSeed.ToString(), 0),
                TestContext.Current.CancellationToken
            );

        // Assert
        map.Seed.Should().Be(DungeonTestData.ReferenceSeed.ToString());
        map.Width.Should().Be(expected.Width);
        map.Height.Should().Be(expected.Height);
        map.Rows.Should().HaveCount(expected.Height);
        map.Rows.Should().OnlyContain(row => row.Length == expected.Width);
        map.Legend["#"].Should().Be("wall");
        map.Legend["."].Should().Be("floor");
        map.Rooms.Should().HaveCount(40);
        map.Elements.Should().HaveCount(expected.Elements.Count);
        map.Elements.Should().ContainSingle(element => element.Type == "boss");
        map.Entrance.X.Should().Be(expected.Entrance.X);
        map.Entrance.Y.Should().Be(expected.Entrance.Y);
    }

    [Fact]
    public async Task Handle_SameSeedOnTwoInstances_ReturnsIdenticalMaps()
    {
        // Arrange
        var query = new GetDungeonMapQuery(DungeonTestData.ReferenceSeed.ToString(), 0);

        // Act: two handlers with two providers, as two service replicas.
        GetDungeonMapResult first = await CreateHandler()
            .Handle(query, TestContext.Current.CancellationToken);
        GetDungeonMapResult second = await CreateHandler()
            .Handle(query, TestContext.Current.CancellationToken);

        // Assert
        second.Rows.Should().Equal(first.Rows);
        second.Elements.Should().Equal(first.Elements);
        second.Rooms.Select(room => room.Id).Should().Equal(first.Rooms.Select(room => room.Id));
    }

    [Fact]
    public async Task Handle_UnknownSeed_ThrowsKeyNotFoundException()
    {
        // Arrange
        var unknownSeed = new Seed(424242);

        // Act
        Func<Task> act = async () =>
            await CreateHandler()
                .Handle(
                    new GetDungeonMapQuery(unknownSeed.ToString(), 0),
                    TestContext.Current.CancellationToken
                );

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{unknownSeed}*");
    }

    [Fact]
    public async Task Handle_FloorBeyondTheLastOne_ThrowsKeyNotFoundException()
    {
        // Act
        Func<Task> act = async () =>
            await CreateHandler()
                .Handle(
                    new GetDungeonMapQuery(DungeonTestData.ReferenceSeed.ToString(), 1),
                    TestContext.Current.CancellationToken
                );

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    private GetDungeonMapQueryHandler CreateHandler()
    {
        return new GetDungeonMapQueryHandler(
            _dungeonRunRepositoryMock.Object,
            new GeneratingDungeonProvider()
        );
    }
}
