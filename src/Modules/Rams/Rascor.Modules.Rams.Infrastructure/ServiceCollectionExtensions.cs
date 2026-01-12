using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rascor.Modules.Rams.Application.Services;
using Rascor.Modules.Rams.Infrastructure.Services;

namespace Rascor.Modules.Rams.Infrastructure;

/// <summary>
/// Dependency injection configuration for the RAMS Infrastructure layer
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers RAMS Infrastructure layer services with the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddRamsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register email template service (no dependencies on other RAMS services)
        services.AddScoped<IRamsEmailTemplateService, RamsEmailTemplateService>();

        // Register dashboard service (needed by notification service)
        services.AddScoped<IRamsDashboardService, RamsDashboardService>();

        // Register notification service (depends on dashboard and email template services)
        services.AddScoped<IRamsNotificationService, RamsNotificationService>();

        // Register document service (depends on notification service)
        services.AddScoped<IRamsDocumentService, RamsDocumentService>();

        // Register other services
        services.AddScoped<IRiskAssessmentService, RiskAssessmentService>();
        services.AddScoped<IMethodStepService, MethodStepService>();
        services.AddScoped<IRamsLibraryService, RamsLibraryService>();
        services.AddScoped<IRamsPdfService, RamsPdfService>();
        services.AddScoped<IRamsAiService, RamsAiService>();

        return services;
    }
}
