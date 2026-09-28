using System.Net;
using System.Net.Http.Json;
using Combat.Application.Features.DungeonRunUseCase.GetDungeonRunById;
using Combat.Application.Features.DungeonUseCase.GetDungeonCell;
using Combat.Application.Features.DungeonUseCase.GetDungeonMap;
using Combat.Presentation.DTO;
using FluentAssertions;

namespace Combat.Test.Integration.Dungeons;

/// <summary>US-DUNGEON-01 and US-DUNGEON-02 acceptance criteria, through HTTP and PostgreSQL.</summary>
public sealed class DungeonControllerIntegrationTests(DungeonEndpointFixture fixture)
    : IClassFixture<DungeonEndpointFixture>
{
    [Fact]
    public async Task PostDungeonRun_NewExploration_ReturnsCreatedWithASeedAndTheHeroAtTheEntrance()
    {
        // Act
        HttpResponseMessage response = await PostRunAsync(Guid.NewGuid());
        var run = await response.Content.ReadFromJsonAsync<GetDungeonRunByIdResult>(
            TestContext.Current.CancellationToken
        );
        GetDungeonMapResult map = await GetMapAsync(run!.Seed);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        run.Seed.Should().HaveLength(13);
        run.Status.Should().Be("active");
        run.Hero.Should().Be(map.Entrance);
        map.Rooms.Should().HaveCount(40);
        map.Elements.Should().ContainSingle(element => element.Type == "boss");
    }

    [Fact]
    public async Task PostDungeonRun_TwoExplorations_ProduceDifferentSeeds()
    {
        // Act
        var first = await (await PostRunAsync(Guid.NewGuid())).Content.ReadFromJsonAsync<GetDungeonRunByIdResult>(
            TestContext.Current.CancellationToken
        );
        var second = await (await PostRunAsync(Guid.NewGuid())).Content.ReadFromJsonAsync<GetDungeonRunByIdResult>(
            TestContext.Current.CancellationToken
        );

        // Assert
        second!.Seed.Should().NotBe(first!.Seed);
    }

    [Fact]
    public async Task GetMap_KnownSeedRequestedTwice_ReturnsTheSameImmutableMap()
    {
        // Arrange
        string seed = await CreateRunAndGetSeedAsync();

        // Act
        HttpResponseMessage response = await fixture.HttpClient.GetAsync(
            $"/api/v1/dungeons/{seed}/map",
            TestContext.Current.CancellationToken
        );
        GetDungeonMapResult first = (await response.Content.ReadFromJsonAsync<GetDungeonMapResult>(
            TestContext.Current.CancellationToken
        ))!;
        GetDungeonMapResult second = await GetMapAsync(seed);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.CacheControl!.MaxAge.Should().BeGreaterThan(TimeSpan.FromDays(1));
        second.Rows.Should().Equal(first.Rows);
        second.Elements.Should().Equal(first.Elements);
    }

    [Fact]
    public async Task GetCell_ElementTile_ReturnsItsTypeAndElements()
    {
        // Arrange
        string seed = await CreateRunAndGetSeedAsync();
        GetDungeonMapResult map = await GetMapAsync(seed);
        var boss = map.Elements.Single(element => element.Type == "boss");

        // Act
        var cell = await fixture.HttpClient.GetFromJsonAsync<GetDungeonCellResult>(
            $"/api/v1/dungeons/{seed}/cell?x={boss.X}&y={boss.Y}",
            TestContext.Current.CancellationToken
        );

        // Assert
        cell!.Type.Should().Be("floor");
        cell.Elements.Should().ContainSingle().Which.Type.Should().Be("boss");
    }

    [Theory]
    [InlineData("/api/v1/dungeons/0000000000001/map", HttpStatusCode.NotFound)]
    [InlineData("/api/v1/dungeons/0000000000001/cell?x=1&y=1", HttpStatusCode.NotFound)]
    [InlineData("/api/v1/dungeons/not-a-seed/map", HttpStatusCode.UnprocessableEntity)]
    public async Task GetMap_UnknownOrInvalidSeed_ReturnsAClearErrorAndNoData(
        string url,
        HttpStatusCode expectedStatus
    )
    {
        // Act
        HttpResponseMessage response = await fixture.HttpClient.GetAsync(
            url,
            TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.Should().Be(expectedStatus);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task PostMove_TowardsAFloorTile_MovesTheHeroAndEndsTheTurn()
    {
        // Arrange
        var run = await (await PostRunAsync(Guid.NewGuid())).Content.ReadFromJsonAsync<GetDungeonRunByIdResult>(
            TestContext.Current.CancellationToken
        );

        // Act
        HttpResponseMessage response = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/v1/dungeon-runs/{run!.Id}/moves",
            new MoveHeroDto { Direction = "east" },
            TestContext.Current.CancellationToken
        );
        var moved = await response.Content.ReadFromJsonAsync<GetDungeonRunByIdResult>(
            TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        moved!.Hero.X.Should().Be(run.Hero.X + 1);
        moved.Turn.Should().Be(1);
    }

    [Fact]
    public async Task PostMove_UnknownDirection_Returns422()
    {
        // Arrange
        var run = await (await PostRunAsync(Guid.NewGuid())).Content.ReadFromJsonAsync<GetDungeonRunByIdResult>(
            TestContext.Current.CancellationToken
        );

        // Act
        HttpResponseMessage response = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/v1/dungeon-runs/{run!.Id}/moves",
            new MoveHeroDto { Direction = "up" },
            TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    private Task<HttpResponseMessage> PostRunAsync(Guid gameSessionId)
    {
        return fixture.HttpClient.PostAsJsonAsync(
            "/api/v1/dungeon-runs",
            new CreateDungeonRunDto { GameSessionId = gameSessionId },
            TestContext.Current.CancellationToken
        );
    }

    private async Task<string> CreateRunAndGetSeedAsync()
    {
        var run = await (await PostRunAsync(Guid.NewGuid())).Content.ReadFromJsonAsync<GetDungeonRunByIdResult>(
            TestContext.Current.CancellationToken
        );
        return run!.Seed;
    }

    private async Task<GetDungeonMapResult> GetMapAsync(string seed)
    {
        return (
            await fixture.HttpClient.GetFromJsonAsync<GetDungeonMapResult>(
                $"/api/v1/dungeons/{seed}/map",
                TestContext.Current.CancellationToken
            )
        )!;
    }
}
