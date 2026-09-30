using System.IO.Compression;
using Dungeon.Presentation.Extensions.LogExtension;
using Dungeon.Presentation.Grpc.Interceptors;
using Dungeon.Presentation.Middleware;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;

namespace Dungeon.Presentation.Extensions;

public static class BuilderExtension
{
    /// <summary>
    /// Lets a frontend dev server on another port call the service directly. Browsers
    /// normally reach services through the API Gateway, which owns CORS (ADR-GLOB-003):
    /// the policy is only applied in Development, for the origins in Cors:AllowedOrigins.
    /// </summary>
    public const string LocalFrontendsCorsPolicy = "LocalFrontends";

    public static WebApplicationBuilder ConfigureApi(this WebApplicationBuilder builder)
    {
        string[] allowedOrigins =
            builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        builder.Services.AddCors(options =>
            options.AddPolicy(
                LocalFrontendsCorsPolicy,
                policy =>
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("X-Correlation-Id")
            )
        );

        // A dungeon floor is ~13 KB of JSON, mostly repeated wall and void characters:
        // compressed, it weighs ~1.3 KB. Responses hold no secret next to attacker-controlled
        // input, so compressing over HTTPS does not open a BREACH-style leak.
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat([
                "application/problem+json",
            ]);
        });
        // Optimal instead of the default Fastest: 30% smaller for well under a millisecond.
        builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
            options.Level = CompressionLevel.Optimal
        );
        builder.Services.Configure<GzipCompressionProviderOptions>(options =>
            options.Level = CompressionLevel.Optimal
        );

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();
        builder.Services.AddHealthChecks();
        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<CorrelationIdInterceptor>();
            options.Interceptors.Add<GrpcExceptionInterceptor>();
        });

        ConfigureLogger(builder);

        builder.Services.AddTransient<ExceptionHandlingMiddleware>();
        builder.Services.AddTransient<CorrelationIdInterceptor>();
        builder.Services.AddTransient<GrpcExceptionInterceptor>();
        builder.Services.AddHttpClient();

        return builder;
    }

    private static void ConfigureLogger(WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog(
            (context, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.With<LowercaseLevelEnricher>()
                    .Destructure.With<IgnoreLoggingDestructuringPolicy>();
            },
            preserveStaticLogger: true
        );
    }
}
