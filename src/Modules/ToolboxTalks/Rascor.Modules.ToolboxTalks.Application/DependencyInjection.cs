using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Rascor.Modules.ToolboxTalks.Application;

/// <summary>
/// Dependency injection configuration for the Toolbox Talks Application layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Toolbox Talks Application layer services with the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddToolboxTalksApplication(this IServiceCollection services)
    {
        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
