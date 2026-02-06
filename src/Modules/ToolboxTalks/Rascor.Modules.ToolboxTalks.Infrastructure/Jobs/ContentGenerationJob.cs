using System.Diagnostics;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rascor.Core.Application.Interfaces;
using Rascor.Modules.ToolboxTalks.Application.Commands.GenerateContentTranslations;
using Rascor.Modules.ToolboxTalks.Application.Services;
using Rascor.Modules.ToolboxTalks.Application.Services.Subtitles;
using Rascor.Modules.ToolboxTalks.Infrastructure.Hubs;

namespace Rascor.Modules.ToolboxTalks.Infrastructure.Jobs;

/// <summary>
/// Hangfire background job for orchestrating AI content generation for toolbox talks.
/// This job extracts content from video/PDF, generates sections and quiz questions using AI,
/// and reports real-time progress via SignalR.
/// After successful content generation, automatically generates translations for all
/// languages used by employees in the tenant.
/// </summary>
public class ContentGenerationJob
{
    private readonly IContentGenerationService _generationService;
    private readonly IHubContext<ContentGenerationHub> _hubContext;
    private readonly ICoreDbContext _coreDbContext;
    private readonly ISender _sender;
    private readonly ILanguageCodeService _languageCodeService;
    private readonly ILogger<ContentGenerationJob> _logger;

    public ContentGenerationJob(
        IContentGenerationService generationService,
        IHubContext<ContentGenerationHub> hubContext,
        ICoreDbContext coreDbContext,
        ISender sender,
        ILanguageCodeService languageCodeService,
        ILogger<ContentGenerationJob> logger)
    {
        _generationService = generationService;
        _hubContext = hubContext;
        _coreDbContext = coreDbContext;
        _sender = sender;
        _languageCodeService = languageCodeService;
        _logger = logger;
    }

    /// <summary>
    /// Executes the content generation job.
    /// </summary>
    /// <param name="toolboxTalkId">The toolbox talk to generate content for</param>
    /// <param name="options">Generation options</param>
    /// <param name="connectionId">Optional SignalR connection ID for direct client updates</param>
    /// <param name="tenantId">The tenant ID (passed explicitly since Hangfire jobs run outside HTTP context)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [AutomaticRetry(Attempts = 1)] // Don't retry AI generation - it's expensive
    [Queue("content-generation")] // Use dedicated queue for resource-intensive jobs
    public async Task ExecuteAsync(
        Guid toolboxTalkId,
        ContentGenerationOptions options,
        string? connectionId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "========== CONTENT GENERATION JOB STARTED ==========\n" +
            "ToolboxTalkId: {ToolboxTalkId}\n" +
            "TenantId: {TenantId}\n" +
            "ConnectionId: {ConnectionId}\n" +
            "Options: IncludeVideo={IncludeVideo}, IncludePdf={IncludePdf}, " +
            "MinSections={MinSections}, MinQuestions={MinQuestions}, " +
            "PassThreshold={PassThreshold}, ReplaceExisting={ReplaceExisting}",
            toolboxTalkId,
            tenantId,
            connectionId ?? "none",
            options.IncludeVideo,
            options.IncludePdf,
            options.MinimumSections,
            options.MinimumQuestions,
            options.PassThreshold,
            options.ReplaceExisting);

        ContentGenerationResult? result = null;

        try
        {
            // Create progress reporter that sends updates via SignalR
            var progress = new Progress<ContentGenerationProgress>(async update =>
            {
                _logger.LogInformation(
                    "[Progress] ToolboxTalk {ToolboxTalkId}: Stage={Stage}, Percent={Percent}%, Message={Message}",
                    toolboxTalkId, update.Stage, update.PercentComplete, update.Message);

                await SendProgressUpdateAsync(toolboxTalkId, connectionId, update);
            });

            _logger.LogInformation(
                "[Step 1/4] Starting content extraction for toolbox talk {ToolboxTalkId}...",
                toolboxTalkId);

            // Execute the content generation
            result = await _generationService.GenerateContentAsync(
                toolboxTalkId,
                options,
                tenantId,
                progress,
                cancellationToken);

            stopwatch.Stop();

            // Auto-generate translations for employee languages if content generation succeeded
            if (result.Success)
            {
                _logger.LogInformation(
                    "[DEBUG] Content generation complete for ToolboxTalk {ToolboxTalkId}. " +
                    "Sections created: {SectionCount}, Questions created: {QuestionCount}, " +
                    "HasFinalPortionQuestion: {HasFinalQuestion}. Now starting auto-translation...",
                    toolboxTalkId, result.SectionsGenerated, result.QuestionsGenerated, result.HasFinalPortionQuestion);

                await AutoGenerateTranslationsAsync(toolboxTalkId, tenantId, connectionId, cancellationToken);
            }
            else
            {
                _logger.LogWarning(
                    "[DEBUG] Content generation FAILED for ToolboxTalk {ToolboxTalkId}. " +
                    "Skipping auto-translation. Errors: {Errors}",
                    toolboxTalkId, string.Join("; ", result.Errors));
            }

            // Send completion notification
            await SendCompletionNotificationAsync(toolboxTalkId, connectionId, result);

            if (result.Success)
            {
                var successType = result.PartialSuccess ? "WITH WARNINGS" : "SUCCESSFULLY";
                _logger.LogInformation(
                    "========== CONTENT GENERATION JOB COMPLETED {SuccessType} ==========\n" +
                    "ToolboxTalkId: {ToolboxTalkId}\n" +
                    "Duration: {Duration}ms ({DurationSeconds:F1}s)\n" +
                    "Partial Success: {PartialSuccess}\n" +
                    "Sections Generated: {Sections}\n" +
                    "Questions Generated: {Questions}\n" +
                    "Has Final Portion Question: {HasFinalQuestion}\n" +
                    "Total Tokens Used: {Tokens}\n" +
                    "Warnings: {WarningCount}",
                    successType,
                    toolboxTalkId,
                    stopwatch.ElapsedMilliseconds,
                    stopwatch.ElapsedMilliseconds / 1000.0,
                    result.PartialSuccess,
                    result.SectionsGenerated,
                    result.QuestionsGenerated,
                    result.HasFinalPortionQuestion,
                    result.TotalTokensUsed,
                    result.Warnings.Count);

                if (result.Warnings.Count > 0)
                {
                    _logger.LogWarning(
                        "Content generation warnings for toolbox talk {ToolboxTalkId}: {Warnings}",
                        toolboxTalkId, string.Join("; ", result.Warnings));
                }
            }
            else
            {
                _logger.LogError(
                    "========== CONTENT GENERATION JOB FAILED ==========\n" +
                    "ToolboxTalkId: {ToolboxTalkId}\n" +
                    "Duration: {Duration}ms ({DurationSeconds:F1}s)\n" +
                    "Sections Generated: {Sections}\n" +
                    "Questions Generated: {Questions}\n" +
                    "Errors: {Errors}",
                    toolboxTalkId,
                    stopwatch.ElapsedMilliseconds,
                    stopwatch.ElapsedMilliseconds / 1000.0,
                    result.SectionsGenerated,
                    result.QuestionsGenerated,
                    string.Join("; ", result.Errors));
            }
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            _logger.LogWarning(
                "========== CONTENT GENERATION JOB CANCELLED ==========\n" +
                "ToolboxTalkId: {ToolboxTalkId}\n" +
                "Duration before cancellation: {Duration}ms",
                toolboxTalkId, stopwatch.ElapsedMilliseconds);

            // Send failure notification to client
            await SendCompletionNotificationAsync(toolboxTalkId, connectionId, new ContentGenerationResult(
                Success: false,
                PartialSuccess: false,
                SectionsGenerated: 0,
                QuestionsGenerated: 0,
                HasFinalPortionQuestion: false,
                Errors: new List<string> { "Content generation was cancelled" },
                Warnings: new List<string>(),
                TotalTokensUsed: 0));

            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "========== CONTENT GENERATION JOB EXCEPTION ==========\n" +
                "ToolboxTalkId: {ToolboxTalkId}\n" +
                "Duration before error: {Duration}ms\n" +
                "Exception Type: {ExceptionType}\n" +
                "Exception Message: {ExceptionMessage}\n" +
                "Stack Trace: {StackTrace}",
                toolboxTalkId,
                stopwatch.ElapsedMilliseconds,
                ex.GetType().FullName,
                ex.Message,
                ex.StackTrace);

            // Send failure notification to client
            await SendCompletionNotificationAsync(toolboxTalkId, connectionId, new ContentGenerationResult(
                Success: false,
                PartialSuccess: false,
                SectionsGenerated: result?.SectionsGenerated ?? 0,
                QuestionsGenerated: result?.QuestionsGenerated ?? 0,
                HasFinalPortionQuestion: result?.HasFinalPortionQuestion ?? false,
                Errors: new List<string> { "An unexpected error occurred. Please try again." },
                Warnings: result?.Warnings ?? new List<string>(),
                TotalTokensUsed: result?.TotalTokensUsed ?? 0));

            throw;
        }
    }

    /// <summary>
    /// Generates a user-friendly completion message based on the result.
    /// </summary>
    private static string GetCompletionMessage(ContentGenerationResult result)
    {
        if (!result.Success)
        {
            return "Content generation failed";
        }

        if (result.PartialSuccess)
        {
            return $"Content generated with some limitations ({result.SectionsGenerated} sections, {result.QuestionsGenerated} questions)";
        }

        return $"Content generated successfully ({result.SectionsGenerated} sections, {result.QuestionsGenerated} questions)";
    }

    /// <summary>
    /// Automatically generates content translations for all non-English languages
    /// used by employees in the tenant. Failures are logged but do not affect
    /// the overall content generation result.
    /// </summary>
    private async Task AutoGenerateTranslationsAsync(
        Guid toolboxTalkId,
        Guid tenantId,
        string? connectionId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[DEBUG] AutoGenerateTranslationsAsync called for ToolboxTalk {ToolboxTalkId}, TenantId {TenantId}",
                toolboxTalkId, tenantId);

            // NOTE: We use IgnoreQueryFilters() because this method runs in a background job context
            // where ICurrentUserService.TenantId may not be set correctly for the global tenant filter.
            // Since we explicitly filter by tenantId and IsDeleted, this is safe.
            var employeeLanguageCodes = await _coreDbContext.Employees
                .IgnoreQueryFilters()
                .Where(e => e.TenantId == tenantId && !e.IsDeleted
                    && e.PreferredLanguage != null && e.PreferredLanguage != "en")
                .Select(e => e.PreferredLanguage!)
                .Distinct()
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "[DEBUG] Employee languages query returned {Count} languages: {Languages}",
                employeeLanguageCodes.Count, string.Join(", ", employeeLanguageCodes));

            if (employeeLanguageCodes.Count == 0)
            {
                _logger.LogInformation(
                    "[DEBUG] No non-English employee languages found for tenant {TenantId}. Skipping auto-translation.",
                    tenantId);
                return;
            }

            // Convert language codes (e.g. "pl") to language names (e.g. "Polish")
            // as the translation command expects language names
            var languageNames = employeeLanguageCodes
                .Select(code => _languageCodeService.GetLanguageName(code))
                .ToList();

            _logger.LogInformation(
                "[DEBUG] Converted to language names: {Names} (from codes: {Codes})",
                string.Join(", ", languageNames), string.Join(", ", employeeLanguageCodes));

            _logger.LogInformation(
                "Auto-generating translations for ToolboxTalk {ToolboxTalkId} in {Count} languages: {Languages}",
                toolboxTalkId, languageNames.Count, string.Join(", ", languageNames));

            await SendProgressUpdateAsync(toolboxTalkId, connectionId,
                new ContentGenerationProgress(
                    "Translating", 92,
                    $"Generating translations for {languageNames.Count} language(s)..."));

            _logger.LogInformation(
                "[DEBUG] Dispatching GenerateContentTranslationsCommand for {Count} languages: {Languages}",
                languageNames.Count, string.Join(", ", languageNames));

            var command = new GenerateContentTranslationsCommand
            {
                ToolboxTalkId = toolboxTalkId,
                TenantId = tenantId,
                TargetLanguages = languageNames
            };

            var translationResult = await _sender.Send(command, cancellationToken);

            _logger.LogInformation(
                "[DEBUG] GenerateContentTranslationsCommand completed. Success: {Success}, " +
                "LanguageResults count: {Count}, Error: {Error}",
                translationResult.Success, translationResult.LanguageResults.Count,
                translationResult.ErrorMessage ?? "none");

            if (translationResult.Success)
            {
                var successCount = translationResult.LanguageResults.Count(r => r.Success);
                var totalCount = translationResult.LanguageResults.Count;

                _logger.LogInformation(
                    "Auto-translation completed for ToolboxTalk {ToolboxTalkId}. " +
                    "Success: {SuccessCount}/{TotalCount} languages",
                    toolboxTalkId, successCount, totalCount);

                foreach (var langResult in translationResult.LanguageResults.Where(r => !r.Success))
                {
                    _logger.LogWarning(
                        "Auto-translation failed for language {Language} ({Code}): {Error}",
                        langResult.Language, langResult.LanguageCode, langResult.ErrorMessage);
                }

                await SendProgressUpdateAsync(toolboxTalkId, connectionId,
                    new ContentGenerationProgress(
                        "Translating", 98,
                        $"Translations complete ({successCount}/{totalCount} languages)."));
            }
            else
            {
                _logger.LogWarning(
                    "Auto-translation failed for ToolboxTalk {ToolboxTalkId}: {Error}",
                    toolboxTalkId, translationResult.ErrorMessage);

                await SendProgressUpdateAsync(toolboxTalkId, connectionId,
                    new ContentGenerationProgress(
                        "Translating", 98,
                        "Translation generation encountered errors. Translations can be generated manually."));
            }
        }
        catch (Exception ex)
        {
            // Translation failure should never fail the content generation job
            _logger.LogError(ex,
                "Auto-translation failed unexpectedly for ToolboxTalk {ToolboxTalkId}. " +
                "Content generation was successful. Translations can be generated manually.",
                toolboxTalkId);

            try
            {
                await SendProgressUpdateAsync(toolboxTalkId, connectionId,
                    new ContentGenerationProgress(
                        "Translating", 98,
                        "Auto-translation skipped due to an error. Translations can be generated manually."));
            }
            catch
            {
                // Swallow - we're already in error handling
            }
        }
    }

    /// <summary>
    /// Sends a progress update to the client via SignalR.
    /// </summary>
    private async Task SendProgressUpdateAsync(
        Guid toolboxTalkId,
        string? connectionId,
        ContentGenerationProgress update)
    {
        try
        {
            var payload = new
            {
                toolboxTalkId,
                stage = update.Stage,
                percentComplete = update.PercentComplete,
                message = update.Message
            };

            if (!string.IsNullOrEmpty(connectionId))
            {
                // Send to specific client
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ContentGenerationProgress", payload);
            }

            // Also send to the group (for multiple listeners)
            await _hubContext.Clients.Group($"content-generation-{toolboxTalkId}")
                .SendAsync("ContentGenerationProgress", payload);

            _logger.LogDebug(
                "Progress update sent for toolbox talk {ToolboxTalkId}: {Stage} - {Percent}%",
                toolboxTalkId, update.Stage, update.PercentComplete);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to send progress update for toolbox talk {ToolboxTalkId}",
                toolboxTalkId);
        }
    }

    /// <summary>
    /// Sends a completion notification to the client via SignalR.
    /// </summary>
    private async Task SendCompletionNotificationAsync(
        Guid toolboxTalkId,
        string? connectionId,
        ContentGenerationResult result)
    {
        try
        {
            var payload = new
            {
                toolboxTalkId,
                success = result.Success,
                partialSuccess = result.PartialSuccess,
                sectionsGenerated = result.SectionsGenerated,
                questionsGenerated = result.QuestionsGenerated,
                hasFinalPortionQuestion = result.HasFinalPortionQuestion,
                errors = result.Errors,
                warnings = result.Warnings,
                totalTokensUsed = result.TotalTokensUsed,
                message = GetCompletionMessage(result)
            };

            if (!string.IsNullOrEmpty(connectionId))
            {
                // Send to specific client
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ContentGenerationComplete", payload);
            }

            // Also send to the group (for multiple listeners)
            await _hubContext.Clients.Group($"content-generation-{toolboxTalkId}")
                .SendAsync("ContentGenerationComplete", payload);

            _logger.LogDebug(
                "Completion notification sent for toolbox talk {ToolboxTalkId}: Success={Success}",
                toolboxTalkId, result.Success);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to send completion notification for toolbox talk {ToolboxTalkId}",
                toolboxTalkId);
        }
    }
}
