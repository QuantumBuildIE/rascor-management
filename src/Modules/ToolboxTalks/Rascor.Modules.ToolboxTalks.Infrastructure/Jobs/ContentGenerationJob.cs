using System.Diagnostics;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Rascor.Modules.ToolboxTalks.Application.Services;
using Rascor.Modules.ToolboxTalks.Infrastructure.Hubs;

namespace Rascor.Modules.ToolboxTalks.Infrastructure.Jobs;

/// <summary>
/// Hangfire background job for orchestrating AI content generation for toolbox talks.
/// This job extracts content from video/PDF, generates sections and quiz questions using AI,
/// and reports real-time progress via SignalR.
/// </summary>
public class ContentGenerationJob
{
    private readonly IContentGenerationService _generationService;
    private readonly IHubContext<ContentGenerationHub> _hubContext;
    private readonly ILogger<ContentGenerationJob> _logger;

    public ContentGenerationJob(
        IContentGenerationService generationService,
        IHubContext<ContentGenerationHub> hubContext,
        ILogger<ContentGenerationJob> logger)
    {
        _generationService = generationService;
        _hubContext = hubContext;
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

            // Send completion notification
            await SendCompletionNotificationAsync(toolboxTalkId, connectionId, result);

            if (result.Success)
            {
                _logger.LogInformation(
                    "========== CONTENT GENERATION JOB COMPLETED SUCCESSFULLY ==========\n" +
                    "ToolboxTalkId: {ToolboxTalkId}\n" +
                    "Duration: {Duration}ms ({DurationSeconds:F1}s)\n" +
                    "Sections Generated: {Sections}\n" +
                    "Questions Generated: {Questions}\n" +
                    "Has Final Portion Question: {HasFinalQuestion}\n" +
                    "Total Tokens Used: {Tokens}\n" +
                    "Warnings: {WarningCount}",
                    toolboxTalkId,
                    stopwatch.ElapsedMilliseconds,
                    stopwatch.ElapsedMilliseconds / 1000.0,
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
                SectionsGenerated: 0,
                QuestionsGenerated: 0,
                HasFinalPortionQuestion: false,
                Errors: new List<string> { "Job was cancelled" },
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
                SectionsGenerated: result?.SectionsGenerated ?? 0,
                QuestionsGenerated: result?.QuestionsGenerated ?? 0,
                HasFinalPortionQuestion: result?.HasFinalPortionQuestion ?? false,
                Errors: new List<string> { $"Unexpected error: {ex.Message}" },
                Warnings: result?.Warnings ?? new List<string>(),
                TotalTokensUsed: result?.TotalTokensUsed ?? 0));

            throw;
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
                sectionsGenerated = result.SectionsGenerated,
                questionsGenerated = result.QuestionsGenerated,
                hasFinalPortionQuestion = result.HasFinalPortionQuestion,
                errors = result.Errors,
                warnings = result.Warnings,
                totalTokensUsed = result.TotalTokensUsed
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
