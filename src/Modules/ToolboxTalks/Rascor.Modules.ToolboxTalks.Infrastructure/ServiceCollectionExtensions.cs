using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rascor.Modules.ToolboxTalks.Application.Abstractions.Subtitles;
using Rascor.Modules.ToolboxTalks.Application.Services;
using Rascor.Modules.ToolboxTalks.Application.Services.Subtitles;
using Rascor.Modules.ToolboxTalks.Infrastructure.Configuration;
using Rascor.Modules.ToolboxTalks.Infrastructure.Services;
using Rascor.Modules.ToolboxTalks.Infrastructure.Services.Subtitles;

namespace Rascor.Modules.ToolboxTalks.Infrastructure;

/// <summary>
/// Dependency injection configuration for the Toolbox Talks Infrastructure layer
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Toolbox Talks Infrastructure layer services with the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddToolboxTalksInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register email service
        services.AddScoped<IToolboxTalkEmailService, ToolboxTalkEmailService>();

        // Register reports service
        services.AddScoped<IToolboxTalkReportsService, ToolboxTalkReportsService>();

        // Register export service (stub implementation for Phase 2)
        services.AddScoped<IToolboxTalkExportService, ToolboxTalkExportService>();

        // Register subtitle processing configuration
        services.Configure<SubtitleProcessingSettings>(
            configuration.GetSection(SubtitleProcessingSettings.SectionName));

        // Register subtitle processing infrastructure services
        // ElevenLabs transcription can take a long time for large videos (download + transcription)
        services.AddHttpClient<ITranscriptionService, ElevenLabsTranscriptionService>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(10); // 10 minutes for video transcription
        });
        // Claude translation is usually fast but can take time for long subtitle files
        services.AddHttpClient<ITranslationService, ClaudeTranslationService>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5); // 5 minutes for translation
        });

        // Register SRT storage provider based on configuration
        var srtStorageType = configuration
            .GetSection($"{SubtitleProcessingSettings.SectionName}:SrtStorage:Type")
            .Value ?? "CloudflareR2";

        if (srtStorageType.Equals("CloudflareR2", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<ISrtStorageProvider, CloudflareR2SrtStorageProvider>();
        }
        else
        {
            // Fall back to GitHub for backward compatibility
            services.AddHttpClient<ISrtStorageProvider, GitHubSrtStorageProvider>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(2); // 2 minutes for file uploads
            });
        }

        services.AddScoped<IVideoSourceProvider, GoogleDriveVideoSourceProvider>();
        services.AddScoped<ISubtitleProgressReporter, SignalRProgressReporter>();

        // Register subtitle processing orchestrator
        services.AddScoped<ISubtitleProcessingOrchestrator, SubtitleProcessingOrchestrator>();

        // Note: SignalR hub is registered in Program.cs with app.MapHub<SubtitleProcessingHub>()
        // Note: Hangfire background jobs are registered in Program.cs

        return services;
    }
}
