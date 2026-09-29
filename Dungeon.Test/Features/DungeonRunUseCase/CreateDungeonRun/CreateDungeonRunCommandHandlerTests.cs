using Dungeon.Application.Features.DungeonRunUseCase.CreateDungeonRun;
using Dungeon.Domain.Entities;
using Dungeon.Domain.Exceptions;
using Dungeon.Domain.Repositories;
using Dungeon.Domain.Services;
using Dungeon.Domain.ValueObjects;
using Dungeon.Test.TestSupport;
using FluentAssertions;
using Moq;

namespace Dungeon.Test.Features.DungeonRunUseCase.CreateDungeonRun;

public class CreateDungeonRunCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 28, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IDungeonRunRepository> _dungeonRunRepositoryMock = new();
    private readonly Mock<ISeedGenerator> _seedGeneratorMock = new();
    private readonly Mock<IClock> _clockMock = new();
    private readonly CreateDungeonRunCommandHandler _handler;
    private DungeonRun? _storedRun;

    public CreateDungeonRunCommandHandlerTests()
    {
        _clockMock.Setup(clock => clock.UtcNow).Returns(Now);
        _dungeonRunRepositoryMock
            .Setup(repository =>
                repository.AddAsync(It.IsAny<DungeonRun>(), It.IsAny<CancellationToken>())
            )
            .Callback<DungeonRun, CancellationToken>((run, _) => _storedRun = run)
            .Returns(Task.CompletedTask);

        _handler = new CreateDungeonRunCommandHandler(
            _dungeonRunRepositoryMock.Object,
            new GeneratingDungeonProvider(),
            _seedGeneratorMock.Object,
            DungeonSettings.Default,
            _clockMock.Object
        );
    }

    [Fact]
    public async Task Handle_NewExploration_StoresARunOnAFreshSeed()
    {
        // Arrange
        Seed freshSeed = DungeonTestData.ReferenceSeed;
        _seedGeneratorMock.Setup(generator => generator.NewSeed()).Returns(freshSeed);
        var command = new CreateDungeonRunCommand(Guid.NewGuid(), Guid.NewGuid(), null);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _storedRun.Should().NotBeNull();
        _storedRun!.Id.Should().Be(command.RunId);
        _storedRun.GameSessionId.Should().Be(command.GameSessionId);
        _storedRun.Seed.Should().Be(freshSeed);
        _storedRun.RoomCount.Should().Be(40);
        _storedRun.StartedAt.Should().Be(Now);
        _storedRun.HeroPosition.Should().Be(DungeonTestData.Generate(freshSeed).Floors[0].Entrance);
    }

    [Fact]
    public async Task Handle_DrawnSeedAlreadyUsed_DrawsAnotherOne()
    {
        // Arrange
        Seed usedSeed = new(1);
        Seed freshSeed = new(2);
        _seedGeneratorMock
            .SetupSequence(generator => generator.NewSeed())
            .Returns(usedSeed)
            .Returns(freshSeed);
        _dungeonRunRepositoryMock
            .Setup(repository =>
                repository.FindLatestBySeedAsync(usedSeed, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(StartRun(usedSeed, DungeonSettings.Default));

        // Act
        await _handler.Handle(
            new CreateDungeonRunCommand(Guid.NewGuid(), Guid.NewGuid(), null),
            TestContext.Current.CancellationToken
        );

        // Assert
        _storedRun!.Seed.Should().Be(freshSeed);
    }

    [Fact]
    public async Task Handle_ReplayedSeed_ReusesTheSettingsOfTheOriginalRun()
    {
        // Arrange
        DungeonSettings originalSettings = new(40, 2);
        _dungeonRunRepositoryMock
            .Setup(repository =>
                repository.FindLatestBySeedAsync(
                    DungeonTestData.ReferenceSeed,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(StartRun(DungeonTestData.ReferenceSeed, originalSettings));
        var command = new CreateDungeonRunCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DungeonTestData.ReferenceSeed.ToString()
        );

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _storedRun!.Seed.Should().Be(DungeonTestData.ReferenceSeed);
        _storedRun.Settings.Should().Be(originalSettings);
        _seedGeneratorMock.Verify(generator => generator.NewSeed(), Times.Never);
    }

    [Fact]
    public async Task Handle_SameRequestRetried_IsIdempotent()
    {
        // Arrange
        DungeonRun existing = StartRun(DungeonTestData.ReferenceSeed, DungeonSettings.Default);
        _dungeonRunRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(existing);

        // Act
        await _handler.Handle(
            new CreateDungeonRunCommand(existing.Id, existing.GameSessionId, null),
            TestContext.Current.CancellationToken
        );

        // Assert
        _dungeonRunRepositoryMock.Verify(
            repository =>
                repository.AddAsync(It.IsAny<DungeonRun>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_RunIdTakenByAnotherSession_ThrowsConflict()
    {
        // Arrange
        DungeonRun existing = StartRun(DungeonTestData.ReferenceSeed, DungeonSettings.Default);
        _dungeonRunRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(existing);

        // Act
        Func<Task> act = async () =>
            await _handler.Handle(
                new CreateDungeonRunCommand(existing.Id, Guid.NewGuid(), null),
                TestContext.Current.CancellationToken
            );

        // Assert
        await act.Should().ThrowAsync<DungeonRunAlreadyExistsException>();
    }

    private static DungeonRun StartRun(Seed seed, DungeonSettings settings)
    {
        return DungeonRun.Start(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DungeonTestData.Generate(seed, settings),
            Now
        );
    }
}
