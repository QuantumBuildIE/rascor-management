using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Rascor.Modules.Rams.Application;

/// <summary>
/// Dependency injection configuration for the RAMS Application layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers RAMS Application layer services with the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddRamsApplication(this IServiceCollection services)
    {
        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Note: IRamsDocumentService is registered in Infrastructure layer

        return services;
    }
}
