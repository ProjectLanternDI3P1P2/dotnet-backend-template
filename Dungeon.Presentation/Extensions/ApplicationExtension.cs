using Dungeon.Presentation.Middleware;
using Scalar.AspNetCore;

namespace Dungeon.Presentation.Extensions;

public static class ApplicationExtension
{
    public static WebApplication ConfigureStart(this WebApplication app)
    {
        bool isDevelopment = app.Environment.IsDevelopment();

        // First, so that every response, error responses included, can be compressed.
        app.UseResponseCompression();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (isDevelopment)
        {
            app.UseCors(BuilderExtension.LocalFrontendsCorsPolicy);
        }
        else
        {
            app.UseHttpsRedirection();
        }

        app.MapControllers();
        app.MapGrpcServices();
        app.MapHealthChecks("/health/live");
        app.MapHealthChecks("/health/ready");

        if (isDevelopment)
        {
            app.MapOpenApi();
            app.MapScalarApiReference(opt =>
            {
                opt.Title = "Dungeon API";
                opt.Theme = ScalarTheme.DeepSpace;
                opt.AddApiKeyAuthentication("UserId", scheme => scheme.WithName("X-User-Id"));
                opt.EnablePersistentAuthentication();
            });
        }

        return app;
    }
}
