using Player.Presentation.Extensions;
using Player.Application;
using Player.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureApi();

builder.Services
    .AddInfrastructureServices(builder.Configuration)
    .AddApplicationServices();

var app = builder.Build();

app.ConfigureStart();

await app.RunAsync();
