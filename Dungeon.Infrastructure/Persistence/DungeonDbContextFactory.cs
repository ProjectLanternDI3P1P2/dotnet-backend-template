using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Dungeon.Infrastructure.Persistence;

/// <summary>Creates the context for EF Core commands without starting the HTTP application.</summary>
public sealed class DungeonDbContextFactory : IDesignTimeDbContextFactory<DungeonDbContext>
{
    public DungeonDbContext CreateDbContext(string[] args)
    {
        string configurationDirectory = FindPresentationConfigurationDirectory();
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(configurationDirectory)
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is required to create EF Core migrations."
            );

        DbContextOptions<DungeonDbContext> options = new DbContextOptionsBuilder<DungeonDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new DungeonDbContext(options);
    }

    private static string FindPresentationConfigurationDirectory()
    {
        for (
            DirectoryInfo? directory = new(Directory.GetCurrentDirectory());
            directory is not null;
            directory = directory.Parent
        )
        {
            string presentationDirectory = Path.Combine(directory.FullName, "Dungeon.Presentation");
            if (File.Exists(Path.Combine(presentationDirectory, "appsettings.json")))
            {
                return presentationDirectory;
            }
        }

        throw new InvalidOperationException(
            "Could not find Dungeon.Presentation/appsettings.json from the current directory."
        );
    }
}
