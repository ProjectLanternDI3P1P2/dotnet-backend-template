namespace Dungeon.Test.Integration.Players;

public sealed class PlayerEndpointFixture : IAsyncLifetime
{
    private TestDatabase? database;
    private DungeonWebApplicationFactory? factory;

    public HttpClient HttpClient =>
        factory?.CreateClient() ?? throw new InvalidOperationException("Fixture not initialized.");

    public async ValueTask InitializeAsync()
    {
        database = await TestDatabase.CreateAsync($"dungeon_test_players_{Guid.NewGuid():N}");
        factory = new DungeonWebApplicationFactory(database.ConnectionString);
        await database.ResetAsync();
    }

    public async ValueTask DisposeAsync()
    {
        factory?.Dispose();
        if (database is not null)
            await database.DisposeAsync();
    }
}
