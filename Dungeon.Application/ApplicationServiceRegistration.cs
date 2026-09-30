using System.Reflection;
using Dungeon.Application.PipelineBehavior;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Dungeon.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cf =>
        {
            cf.RegisterServicesFromAssemblies(assembly);
            cf.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cf.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        return services.AddValidatorsFromAssembly(assembly);
    }
}
