using Dungeon.Application.Ports;
using Dungeon.Domain.Services;
using Dungeon.Domain.Services.Generation;
using Dungeon.Domain.ValueObjects;
using Dungeon.Infrastructure.Grpc;
using Dungeon.Infrastructure.Messaging;
using Dungeon.Infrastructure.Persistence;
using Dungeon.Infrastructure.PipelineBehavior;
using Dungeon.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Dungeon.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        DatabaseOptions databaseOptions =
            configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? new DatabaseOptions();

        return services
            .AddSingleton(Options.Create(databaseOptions))
            .AddSingleton<IClock, SystemClock>()
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(CommandTransactionBehavior<,>))
            .AddEfConnection()
            .AddRepositories()
            .AddDungeonGeneration(configuration)
            .AddGrpcConfiguration(configuration)
            .AddMessaging(configuration);
    }

    private static IServiceCollection AddDungeonGeneration(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        DungeonGenerationOptions options =
            configuration
                .GetSection(DungeonGenerationOptions.SectionName)
                .Get<DungeonGenerationOptions>()
            ?? new DungeonGenerationOptions();

        // Built here so that an impossible configuration (0 floors, 3 rooms...) stops the
        // service at startup instead of failing on the first exploration.
        DungeonSettings settings = new(options.RoomCount, options.FloorCount);

        return services
            .AddSingleton(Options.Create(options))
            .AddSingleton(settings)
            .AddSingleton<DungeonGenerator>()
            .AddSingleton<IDungeonProvider, CachedDungeonProvider>()
            .AddSingleton<ISeedGenerator, CryptoSeedGenerator>();
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services.Scan(scan =>
            scan.FromAssembliesOf(typeof(InfrastructureServiceRegistration))
                .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Repository")))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );
    }

    private static IServiceCollection AddEfConnection(this IServiceCollection services)
    {
        return services.AddDbContext<DungeonDbContext>(
            (serviceProvider, options) =>
            {
                DatabaseOptions databaseOptions = serviceProvider
                    .GetRequiredService<IOptions<DatabaseOptions>>()
                    .Value;
                string connectionString = GetConnectionString(databaseOptions);

                options.UseNpgsql(connectionString);
            }
        );
    }

    private static string GetConnectionString(DatabaseOptions databaseOptions)
    {
        string connectionString =
            databaseOptions.DefaultConnection
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is missing in the configuration."
            );

        if (string.IsNullOrWhiteSpace(databaseOptions.PasswordFile))
        {
            return connectionString;
        }

        if (!File.Exists(databaseOptions.PasswordFile))
        {
            throw new InvalidOperationException(
                $"The database password file '{databaseOptions.PasswordFile}' does not exist."
            );
        }

        string password = File.ReadAllText(databaseOptions.PasswordFile).TrimEnd('\r', '\n');
        if (string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException(
                $"The database password file '{databaseOptions.PasswordFile}' is empty."
            );
        }

        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Password = password,
        };

        return connectionStringBuilder.ConnectionString;
    }
}
