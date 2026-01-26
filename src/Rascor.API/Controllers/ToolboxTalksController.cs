using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.ToolboxTalks.Application.Commands.CreateToolboxTalk;
using Rascor.Modules.ToolboxTalks.Application.Commands.DeleteToolboxTalk;
using Rascor.Modules.ToolboxTalks.Application.Commands.UpdateToolboxTalk;
using Rascor.Modules.ToolboxTalks.Application.DTOs;
using Rascor.Modules.ToolboxTalks.Application.DTOs.Reports;
using Rascor.Modules.ToolboxTalks.Application.Queries.GetToolboxTalkById;
using Rascor.Modules.ToolboxTalks.Application.Queries.GetToolboxTalkDashboard;
using Rascor.Modules.ToolboxTalks.Application.Queries.GetToolboxTalks;
using Rascor.Modules.ToolboxTalks.Application.Queries.GetToolboxTalkSettings;
using Rascor.Modules.ToolboxTalks.Application.Services;
using Rascor.Modules.ToolboxTalks.Domain.Enums;
using Rascor.Modules.ToolboxTalks.Infrastructure.Jobs;
using System.ComponentModel.DataAnnotations;

namespace Rascor.API.Controllers;

/// <summary>
/// Controller for managing toolbox talk templates
/// </summary>
[ApiController]
[Route("api/toolbox-talks")]
[Authorize(Policy = "ToolboxTalks.View")]
public class ToolboxTalksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IToolboxTalkReportsService _reportsService;
    private readonly IToolboxTalkExportService _exportService;
    private readonly IContentExtractionService _contentExtractionService;
    private readonly ILogger<ToolboxTalksController> _logger;

    public ToolboxTalksController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        IToolboxTalkReportsService reportsService,
        IToolboxTalkExportService exportService,
        IContentExtractionService contentExtractionService,
        ILogger<ToolboxTalksController> logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _reportsService = reportsService;
        _exportService = exportService;
        _contentExtractionService = contentExtractionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all toolbox talks with pagination and filtering
    /// </summary>
    /// <param name="searchTerm">Optional search term for title or description</param>
    /// <param name="frequency">Optional filter by frequency</param>
    /// <param name="isActive">Optional filter by active status</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of toolbox talks</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<ToolboxTalkListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? searchTerm = null,
        [FromQuery] ToolboxTalkFrequency? frequency = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = new GetToolboxTalksQuery
            {
                TenantId = _currentUserService.TenantId,
                SearchTerm = searchTerm,
                Frequency = frequency,
                IsActive = isActive,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(Result.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving toolbox talks");
            return StatusCode(500, Result.Fail("Error retrieving toolbox talks"));
        }
    }

    /// <summary>
    /// Get a toolbox talk by ID
    /// </summary>
    /// <param name="id">Toolbox talk ID</param>
    /// <returns>Toolbox talk details with sections and questions</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ToolboxTalkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var query = new GetToolboxTalkByIdQuery
            {
                TenantId = _currentUserService.TenantId,
                Id = id
            };

            var result = await _mediator.Send(query);
            if (result == null)
            {
                return NotFound(new { message = "Toolbox talk not found" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving toolbox talk {ToolboxTalkId}", id);
            return StatusCode(500, new { message = "Error retrieving toolbox talk" });
        }
    }

    /// <summary>
    /// Create a new toolbox talk
    /// </summary>
    /// <param name="command">Toolbox talk creation data</param>
    /// <returns>Created toolbox talk</returns>
    [HttpPost]
    [Authorize(Policy = "ToolboxTalks.Create")]
    [ProducesResponseType(typeof(ToolboxTalkDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateToolboxTalkCommand command)
    {
        try
        {
            var commandWithTenant = command with { TenantId = _currentUserService.TenantId };
            var result = await _mediator.Send(commandWithTenant);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating toolbox talk");
            return StatusCode(500, new { message = "Error creating toolbox talk" });
        }
    }

    /// <summary>
    /// Update an existing toolbox talk
    /// </summary>
    /// <param name="id">Toolbox talk ID</param>
    /// <param name="command">Updated toolbox talk data</param>
    /// <returns>Updated toolbox talk</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ToolboxTalks.Edit")]
    [ProducesResponseType(typeof(ToolboxTalkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateToolboxTalkCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var commandWithTenant = command with { TenantId = _currentUserService.TenantId };
            var result = await _mediator.Send(commandWithTenant);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating toolbox talk {ToolboxTalkId}", id);
            return StatusCode(500, new { message = "Error updating toolbox talk" });
        }
    }

    /// <summary>
    /// Delete a toolbox talk (soft delete)
    /// </summary>
    /// <param name="id">Toolbox talk ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ToolboxTalks.Delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteToolboxTalkCommand
            {
                TenantId = _currentUserService.TenantId,
                Id = id
            };

            await _mediator.Send(command);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting toolbox talk {ToolboxTalkId}", id);
            return StatusCode(500, new { message = "Error deleting toolbox talk" });
        }
    }

    /// <summary>
    /// Get toolbox talks dashboard with KPIs and statistics
    /// </summary>
    /// <returns>Dashboard data with completion rates and overdue counts</returns>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ToolboxTalkDashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var query = new GetToolboxTalkDashboardQuery
            {
                TenantId = _currentUserService.TenantId
            };

            var result = await _mediator.Send(query);
            return Ok(Result.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving toolbox talks dashboard");
            return StatusCode(500, Result.Fail("Error retrieving toolbox talks dashboard"));
        }
    }

    /// <summary>
    /// Get toolbox talk settings for the current tenant
    /// </summary>
    /// <returns>Toolbox talk settings</returns>
    [HttpGet("settings")]
    [Authorize(Policy = "ToolboxTalks.View")]
    [ProducesResponseType(typeof(ToolboxTalkSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings()
    {
        try
        {
            var query = new GetToolboxTalkSettingsQuery
            {
                TenantId = _currentUserService.TenantId
            };

            var result = await _mediator.Send(query);
            return Ok(Result.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving toolbox talk settings");
            return StatusCode(500, Result.Fail("Error retrieving toolbox talk settings"));
        }
    }

    /// <summary>
    /// Update toolbox talk settings for the current tenant
    /// </summary>
    /// <param name="dto">Updated settings</param>
    /// <returns>Updated settings</returns>
    [HttpPut("settings")]
    [Authorize(Policy = "ToolboxTalks.Admin")]
    [ProducesResponseType(typeof(ToolboxTalkSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateToolboxTalkSettingsDto dto)
    {
        try
        {
            // TODO: Implement UpdateToolboxTalkSettingsCommand when available
            // For now, return the current settings as a placeholder
            var query = new GetToolboxTalkSettingsQuery
            {
                TenantId = _currentUserService.TenantId
            };

            var result = await _mediator.Send(query);
            return Ok(Result.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating toolbox talk settings");
            return StatusCode(500, Result.Fail("Error updating toolbox talk settings"));
        }
    }

    #region Content Extraction

    /// <summary>
    /// Extracts content from video transcript and/or PDF document for AI generation preview.
    /// This endpoint is for testing and previewing what content will be used for AI generation.
    /// </summary>
    /// <param name="id">Toolbox talk ID</param>
    /// <param name="request">Content extraction options</param>
    /// <returns>Extraction result with combined content and metadata</returns>
    [HttpPost("{id:guid}/extract-content")]
    [Authorize(Policy = "ToolboxTalks.Admin")]
    [ProducesResponseType(typeof(ContentExtractionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExtractContent(
        Guid id,
        [FromBody] ExtractContentRequest request)
    {
        try
        {
            var result = await _contentExtractionService.ExtractContentAsync(
                id,
                request.IncludeVideo,
                request.IncludePdf,
                _currentUserService.TenantId);

            if (!result.Success && result.Errors.Any())
            {
                // If the toolbox talk wasn't found, return 404
                if (result.Errors.Any(e => e.Contains("not found")))
                {
                    return NotFound(new { message = result.Errors.First(), errors = result.Errors, warnings = result.Warnings });
                }

                return BadRequest(new { message = "Content extraction failed", errors = result.Errors, warnings = result.Warnings });
            }

            return Ok(Result.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting content for toolbox talk {ToolboxTalkId}", id);
            return StatusCode(500, Result.Fail("Error extracting content"));
        }
    }

    #endregion

    #region Content Generation

    /// <summary>
    /// Starts AI content generation for a toolbox talk.
    /// This queues a background job that extracts content from video/PDF and generates
    /// sections and quiz questions using AI. Progress updates are sent via SignalR.
    /// </summary>
    /// <param name="id">Toolbox talk ID</param>
    /// <param name="request">Content generation options</param>
    /// <returns>Job information including the Hangfire job ID</returns>
    [HttpPost("{id:guid}/generate")]
    [Authorize(Policy = "ToolboxTalks.Admin")]
    [ProducesResponseType(typeof(GenerateContentResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenerateContentResponse>> GenerateContent(
        Guid id,
        [FromBody] GenerateContentRequest request)
    {
        try
        {
            // First, verify the toolbox talk exists by trying to get it
            var query = new GetToolboxTalkByIdQuery
            {
                TenantId = _currentUserService.TenantId,
                Id = id
            };

            var toolboxTalk = await _mediator.Send(query);
            if (toolboxTalk == null)
            {
                return NotFound(new { error = "Toolbox Talk not found" });
            }

            // Validate we have content to generate from
            if (request.IncludeVideo && string.IsNullOrEmpty(toolboxTalk.VideoUrl))
            {
                return BadRequest(new { error = "Video generation requested but no video URL is set" });
            }

            if (request.IncludePdf && string.IsNullOrEmpty(toolboxTalk.PdfUrl))
            {
                return BadRequest(new { error = "PDF generation requested but no PDF is uploaded" });
            }

            if (!request.IncludeVideo && !request.IncludePdf)
            {
                return BadRequest(new { error = "Must include at least one content source (video or PDF)" });
            }

            // Create generation options
            var options = new ContentGenerationOptions(
                IncludeVideo: request.IncludeVideo,
                IncludePdf: request.IncludePdf,
                MinimumSections: request.MinimumSections ?? 7,
                MinimumQuestions: request.MinimumQuestions ?? 5,
                PassThreshold: request.PassThreshold ?? 80,
                ReplaceExisting: request.ReplaceExisting ?? true);

            // Queue the background job with tenant context
            var tenantId = _currentUserService.TenantId;
            var jobId = BackgroundJob.Enqueue<ContentGenerationJob>(
                job => job.ExecuteAsync(id, options, request.ConnectionId, tenantId, CancellationToken.None));

            _logger.LogInformation(
                "Queued content generation job {JobId} for toolbox talk {ToolboxTalkId}",
                jobId, id);

            return Accepted(new GenerateContentResponse(
                JobId: jobId,
                Message: "Content generation started. Connect to the SignalR hub for real-time progress updates.",
                ToolboxTalkId: id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting content generation for toolbox talk {ToolboxTalkId}", id);
            return StatusCode(500, new { error = "Error starting content generation" });
        }
    }

    #endregion

    #region Reports

    /// <summary>
    /// Get compliance report with breakdowns by department and talk
    /// </summary>
    /// <param name="dateFrom">Optional start date filter</param>
    /// <param name="dateTo">Optional end date filter</param>
    /// <param name="siteId">Optional site/department filter</param>
    /// <returns>Compliance report with metrics and breakdowns</returns>
    [HttpGet("reports/compliance")]
    [ProducesResponseType(typeof(ComplianceReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComplianceReport(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] Guid? siteId = null)
    {
        try
        {
            var report = await _reportsService.GetComplianceReportAsync(
                _currentUserService.TenantId,
                dateFrom,
                dateTo,
                siteId);
            return Ok(Result.Ok(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating compliance report");
            return StatusCode(500, Result.Fail("Error generating compliance report"));
        }
    }

    /// <summary>
    /// Get list of overdue toolbox talk assignments
    /// </summary>
    /// <param name="siteId">Optional site/department filter</param>
    /// <param name="toolboxTalkId">Optional toolbox talk filter</param>
    /// <returns>List of overdue items</returns>
    [HttpGet("reports/overdue")]
    [ProducesResponseType(typeof(List<OverdueItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverdueReport(
        [FromQuery] Guid? siteId = null,
        [FromQuery] Guid? toolboxTalkId = null)
    {
        try
        {
            var report = await _reportsService.GetOverdueReportAsync(
                _currentUserService.TenantId,
                siteId,
                toolboxTalkId);
            return Ok(Result.Ok(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating overdue report");
            return StatusCode(500, Result.Fail("Error generating overdue report"));
        }
    }

    /// <summary>
    /// Get detailed completion records with pagination
    /// </summary>
    /// <param name="dateFrom">Optional start date filter</param>
    /// <param name="dateTo">Optional end date filter</param>
    /// <param name="toolboxTalkId">Optional toolbox talk filter</param>
    /// <param name="siteId">Optional site/department filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated list of completion details</returns>
    [HttpGet("reports/completions")]
    [ProducesResponseType(typeof(PaginatedList<CompletionDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompletionsReport(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] Guid? toolboxTalkId = null,
        [FromQuery] Guid? siteId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var report = await _reportsService.GetCompletionReportAsync(
                _currentUserService.TenantId,
                dateFrom,
                dateTo,
                toolboxTalkId,
                siteId,
                pageNumber,
                pageSize);
            return Ok(Result.Ok(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating completions report");
            return StatusCode(500, Result.Fail("Error generating completions report"));
        }
    }

    /// <summary>
    /// Export overdue report as Excel file
    /// </summary>
    /// <param name="siteId">Optional site/department filter</param>
    /// <param name="toolboxTalkId">Optional toolbox talk filter</param>
    /// <returns>Excel file</returns>
    [HttpGet("reports/overdue/export")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportOverdueReport(
        [FromQuery] Guid? siteId = null,
        [FromQuery] Guid? toolboxTalkId = null)
    {
        try
        {
            var report = await _reportsService.GetOverdueReportAsync(
                _currentUserService.TenantId,
                siteId,
                toolboxTalkId);

            var fileBytes = await _exportService.GenerateOverdueReportExcelAsync(report);

            if (fileBytes.Length == 0)
            {
                return BadRequest(Result.Fail("Export functionality is not yet implemented. Coming in Phase 2."));
            }

            var fileName = $"OverdueToolboxTalks_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting overdue report");
            return StatusCode(500, Result.Fail("Error exporting overdue report"));
        }
    }

    /// <summary>
    /// Export completions report as Excel file
    /// </summary>
    /// <param name="dateFrom">Optional start date filter</param>
    /// <param name="dateTo">Optional end date filter</param>
    /// <param name="toolboxTalkId">Optional toolbox talk filter</param>
    /// <param name="siteId">Optional site/department filter</param>
    /// <returns>Excel file</returns>
    [HttpGet("reports/completions/export")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportCompletionsReport(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] Guid? toolboxTalkId = null,
        [FromQuery] Guid? siteId = null)
    {
        try
        {
            // Get all completions (no pagination for export)
            var report = await _reportsService.GetCompletionReportAsync(
                _currentUserService.TenantId,
                dateFrom,
                dateTo,
                toolboxTalkId,
                siteId,
                1,
                10000); // Large page size for export

            var fileBytes = await _exportService.GenerateCompletionsReportExcelAsync(report.Items);

            if (fileBytes.Length == 0)
            {
                return BadRequest(Result.Fail("Export functionality is not yet implemented. Coming in Phase 2."));
            }

            var fileName = $"ToolboxTalkCompletions_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting completions report");
            return StatusCode(500, Result.Fail("Error exporting completions report"));
        }
    }

    /// <summary>
    /// Export compliance report as PDF
    /// </summary>
    /// <param name="dateFrom">Optional start date filter</param>
    /// <param name="dateTo">Optional end date filter</param>
    /// <param name="siteId">Optional site/department filter</param>
    /// <returns>PDF file</returns>
    [HttpGet("reports/compliance/export")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportComplianceReport(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] Guid? siteId = null)
    {
        try
        {
            var report = await _reportsService.GetComplianceReportAsync(
                _currentUserService.TenantId,
                dateFrom,
                dateTo,
                siteId);

            var fileBytes = await _exportService.GenerateComplianceReportPdfAsync(report);

            var fileName = $"ToolboxTalkCompliance_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(fileBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting compliance report");
            return StatusCode(500, Result.Fail("Error exporting compliance report"));
        }
    }

    #endregion
}

/// <summary>
/// DTO for updating toolbox talk settings
/// </summary>
public record UpdateToolboxTalkSettingsDto
{
    public int DefaultDueDays { get; init; } = 7;
    public int ReminderDaysBefore { get; init; } = 3;
    public bool SendEmailReminders { get; init; } = true;
    public bool SendPushReminders { get; init; } = true;
    public int MaxQuizAttempts { get; init; } = 3;
    public bool RequireSignature { get; init; } = true;
    public bool AutoAssignNewEmployees { get; init; } = true;
}

/// <summary>
/// Request DTO for content extraction endpoint
/// </summary>
public record ExtractContentRequest
{
    /// <summary>
    /// Whether to extract and include video transcript content
    /// </summary>
    [Required]
    public bool IncludeVideo { get; init; }

    /// <summary>
    /// Whether to extract and include PDF document content
    /// </summary>
    [Required]
    public bool IncludePdf { get; init; }
}

/// <summary>
/// Request DTO for content generation endpoint
/// </summary>
public record GenerateContentRequest
{
    /// <summary>
    /// Whether to extract and use video transcript content
    /// </summary>
    [Required]
    public bool IncludeVideo { get; init; }

    /// <summary>
    /// Whether to extract and use PDF document content
    /// </summary>
    [Required]
    public bool IncludePdf { get; init; }

    /// <summary>
    /// Minimum number of sections to generate (default: 7)
    /// </summary>
    public int? MinimumSections { get; init; }

    /// <summary>
    /// Minimum number of quiz questions to generate (default: 5)
    /// </summary>
    public int? MinimumQuestions { get; init; }

    /// <summary>
    /// Quiz pass threshold percentage (default: 80)
    /// </summary>
    public int? PassThreshold { get; init; }

    /// <summary>
    /// Whether to replace existing sections and questions (default: true).
    /// If false, new content will be appended to existing.
    /// </summary>
    public bool? ReplaceExisting { get; init; }

    /// <summary>
    /// Optional SignalR connection ID for receiving real-time progress updates.
    /// Clients should connect to the ContentGenerationHub before calling this endpoint.
    /// </summary>
    public string? ConnectionId { get; init; }
}

/// <summary>
/// Response DTO for content generation endpoint
/// </summary>
/// <param name="JobId">The Hangfire background job ID</param>
/// <param name="Message">Status message</param>
/// <param name="ToolboxTalkId">The toolbox talk ID being processed</param>
public record GenerateContentResponse(
    string JobId,
    string Message,
    Guid ToolboxTalkId);
