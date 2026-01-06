using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using Rascor.Modules.Proposals.Application.Services;

namespace Rascor.Modules.Proposals.Application;

/// <summary>
/// Dependency injection configuration for the Proposals Application layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Proposals Application layer services with the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddProposalsApplication(this IServiceCollection services)
    {
        // Configure QuestPDF license (Community license for open source / small business)
        QuestPDF.Settings.License = LicenseType.Community;

        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Register application services
        services.AddScoped<IProposalService, ProposalService>();
        services.AddScoped<IProposalCalculationService, ProposalCalculationService>();
        services.AddScoped<IProposalWorkflowService, ProposalWorkflowService>();
        services.AddScoped<IProposalPdfService, ProposalPdfService>();
        services.AddScoped<IProposalReportsService, ProposalReportsService>();
        services.AddScoped<IProposalConversionService, ProposalConversionService>();

        return services;
    }
}
