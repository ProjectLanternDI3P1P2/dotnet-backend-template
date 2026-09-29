using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dungeon.Infrastructure.Persistence.Seeding;

public static class DatabaseSeedingExtensions
{
    public static async Task MigrateAndSeedDevelopmentDataAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default
    )
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        DungeonDbContext context = scope.ServiceProvider.GetRequiredService<DungeonDbContext>();

        await context.Database.MigrateAsync(cancellationToken);
        await DataSeeder.SeedAsync(context, cancellationToken);
    }
}
