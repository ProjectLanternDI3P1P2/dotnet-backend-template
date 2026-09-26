using Leaderboard.Presentation.Extensions;
using Leaderboard.Application;
using Leaderboard.Infrastructure;
using Leaderboard.Infrastructure.Persistence.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureApi();

builder.Services
    .AddInfrastructureServices(builder.Configuration)
    .AddApplicationServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.Services.MigrateAndSeedDevelopmentDataAsync();
}

app.ConfigureStart();

await app.RunAsync();
