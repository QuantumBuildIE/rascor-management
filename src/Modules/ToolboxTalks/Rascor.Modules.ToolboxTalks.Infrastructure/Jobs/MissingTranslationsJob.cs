using Hangfire;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rascor.Core.Application.Interfaces;
using Rascor.Modules.ToolboxTalks.Application.Commands.GenerateContentTranslations;
using Rascor.Modules.ToolboxTalks.Application.Common.Interfaces;
using Rascor.Modules.ToolboxTalks.Application.Services.Subtitles;
using Rascor.Modules.ToolboxTalks.Infrastructure.Hubs;

namespace Rascor.Modules.ToolboxTalks.Infrastructure.Jobs;

/// <summary>
/// Hangfire background job that detects and generates translations for languages
/// that are missing after content reuse. When content is copied from a source talk
/// that was created before new languages were added, this job fills the gap.
/// </summary>
public class MissingTranslationsJob
{
    private readonly ICoreDbContext _coreDbContext;
    private readonly IToolboxTalksDbContext _toolboxTalksDbContext;
    private readonly ISender _sender;
    private readonly ILanguageCodeService _languageCodeService;
    private readonly IHubContext<ContentGenerationHub> _hubContext;
    private readonly ILogger<MissingTranslationsJob> _logger;

    public MissingTranslationsJob(
        ICoreDbContext coreDbContext,
        IToolboxTalksDbContext toolboxTalksDbContext,
        ISender sender,
        ILanguageCodeService languageCodeService,
        IHubContext<ContentGenerationHub> hubContext,
        ILogger<MissingTranslationsJob> logger)
    {
        _coreDbContext = coreDbContext;
        _toolboxTalksDbContext = toolboxTalksDbContext;
        _sender = sender;
        _languageCodeService = languageCodeService;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Detects missing translations and generates them for a toolbox talk.
    /// Compares existing translations against current employee language preferences
    /// and generates only what's missing.
    /// </summary>
    [AutomaticRetry(Attempts = 1)]
    [Queue("content-generation")]
    public async Task ExecuteAsync(
        Guid toolboxTalkId,
        Guid tenantId,
        string? connectionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "MissingTranslationsJob started for ToolboxTalk {ToolboxTalkId}, TenantId {TenantId}",
            toolboxTalkId, tenantId);

        try
        {
            // Get the source language of the toolbox talk
            var sourceLanguageCode = await _toolboxTalksDbContext.ToolboxTalks
                .IgnoreQueryFilters()
                .Where(t => t.Id == toolboxTalkId && t.TenantId == tenantId && !t.IsDeleted)
                .Select(t => t.SourceLanguageCode)
                .FirstOrDefaultAsync(cancellationToken) ?? "en";

            // Get existing translation language codes for this talk
            var existingLanguageCodes = await _toolboxTalksDbContext.ToolboxTalkTranslations
                .IgnoreQueryFilters()
                .Where(t => t.ToolboxTalkId == toolboxTalkId && t.TenantId == tenantId && !t.IsDeleted)
                .Select(t => t.LanguageCode)
                .Distinct()
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "ToolboxTalk {ToolboxTalkId} has existing translations for: {ExistingLanguages}",
                toolboxTalkId, string.Join(", ", existingLanguageCodes));

            // Get all required languages from employee preferences (excluding source language)
            var requiredLanguageCodes = await _coreDbContext.Employees
                .IgnoreQueryFilters()
                .Where(e => e.TenantId == tenantId && !e.IsDeleted
                    && e.PreferredLanguage != null && e.PreferredLanguage != sourceLanguageCode)
                .Select(e => e.PreferredLanguage!)
                .Distinct()
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Required languages for tenant {TenantId} (excluding source '{Source}'): {RequiredLanguages}",
                tenantId, sourceLanguageCode, string.Join(", ", requiredLanguageCodes));

            // Compute missing languages
            var missingLanguageCodes = requiredLanguageCodes
                .Except(existingLanguageCodes, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (missingLanguageCodes.Count == 0)
            {
                _logger.LogInformation(
                    "No missing translations for ToolboxTalk {ToolboxTalkId}. All required languages are covered.",
                    toolboxTalkId);
                return;
            }

            // Convert codes to language names
            var missingLanguageNames = missingLanguageCodes
                .Select(code => _languageCodeService.GetLanguageName(code))
                .ToList();

            _logger.LogInformation(
                "Generating missing translations for ToolboxTalk {ToolboxTalkId}: {MissingLanguages}",
                toolboxTalkId, string.Join(", ", missingLanguageNames));

            await SendProgressUpdateAsync(toolboxTalkId, connectionId,
                "Translating", 92,
                $"Generating {missingLanguageNames.Count} missing translation(s)...");

            var command = new GenerateContentTranslationsCommand
            {
                ToolboxTalkId = toolboxTalkId,
                TenantId = tenantId,
                TargetLanguages = missingLanguageNames
            };

            var result = await _sender.Send(command, cancellationToken);

            if (result.Success)
            {
                var successCount = result.LanguageResults.Count(r => r.Success);
                var totalCount = result.LanguageResults.Count;

                _logger.LogInformation(
                    "Missing translations generated for ToolboxTalk {ToolboxTalkId}: {SuccessCount}/{TotalCount}",
                    toolboxTalkId, successCount, totalCount);

                foreach (var langResult in result.LanguageResults.Where(r => !r.Success))
                {
                    _logger.LogWarning(
                        "Translation failed for language {Language} ({Code}): {Error}",
                        langResult.Language, langResult.LanguageCode, langResult.ErrorMessage);
                }

                await SendProgressUpdateAsync(toolboxTalkId, connectionId,
                    "Translating", 100,
                    $"Missing translations complete ({successCount}/{totalCount} languages).");
            }
            else
            {
                _logger.LogWarning(
                    "Missing translations generation failed for ToolboxTalk {ToolboxTalkId}: {Error}",
                    toolboxTalkId, result.ErrorMessage);

                await SendProgressUpdateAsync(toolboxTalkId, connectionId,
                    "Translating", 100,
                    "Some translations could not be generated. They can be generated manually.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "MissingTranslationsJob failed for ToolboxTalk {ToolboxTalkId}",
                toolboxTalkId);

            try
            {
                await SendProgressUpdateAsync(toolboxTalkId, connectionId,
                    "Translating", 100,
                    "Translation generation encountered an error. Translations can be generated manually.");
            }
            catch
            {
                // Swallow - we're already in error handling
            }

            throw;
        }
    }

    private async Task SendProgressUpdateAsync(
        Guid toolboxTalkId,
        string? connectionId,
        string stage,
        int percentComplete,
        string message)
    {
        try
        {
            var payload = new
            {
                toolboxTalkId,
                stage,
                percentComplete,
                message
            };

            if (!string.IsNullOrEmpty(connectionId))
            {
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ContentGenerationProgress", payload);
            }

            await _hubContext.Clients.Group($"content-generation-{toolboxTalkId}")
                .SendAsync("ContentGenerationProgress", payload);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to send progress update for toolbox talk {ToolboxTalkId}",
                toolboxTalkId);
        }
    }
}
