namespace Combat.Test.Integration.Dungeons;

public sealed class DungeonEndpointFixture : IAsyncLifetime
{
    private TestDatabase? database;
    private CombatWebApplicationFactory? factory;

    public HttpClient HttpClient =>
        factory?.CreateClient() ?? throw new InvalidOperationException("Fixture not initialized.");

    public async ValueTask InitializeAsync()
    {
        database = await TestDatabase.CreateAsync($"combat_test_dungeons_{Guid.NewGuid():N}");
        factory = new CombatWebApplicationFactory(database.ConnectionString);
        await database.ResetAsync();
    }

    public async ValueTask DisposeAsync()
    {
        factory?.Dispose();
        if (database is not null)
            await database.DisposeAsync();
    }
}
