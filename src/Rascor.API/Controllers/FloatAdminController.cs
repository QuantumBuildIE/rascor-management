using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Infrastructure.Data;
using Rascor.Core.Infrastructure.Float;
using Rascor.Core.Infrastructure.Float.Jobs;

namespace Rascor.API.Controllers;

/// <summary>
/// Admin controller for Float integration management.
/// Provides endpoints for monitoring status, syncing data, and manually triggering SPA checks.
/// </summary>
[ApiController]
[Route("api/admin/float")]
[Authorize(Policy = "Core.Admin")]
public class FloatAdminController : ControllerBase
{
    private readonly IFloatSpaCheckService _spaCheckService;
    private readonly IFloatMatchingService _matchingService;
    private readonly IFloatApiClient _floatApiClient;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _dbContext;
    private readonly FloatSettings _floatSettings;
    private readonly ILogger<FloatAdminController> _logger;

    public FloatAdminController(
        IFloatSpaCheckService spaCheckService,
        IFloatMatchingService matchingService,
        IFloatApiClient floatApiClient,
        ICurrentUserService currentUserService,
        ApplicationDbContext dbContext,
        IOptions<FloatSettings> floatSettings,
        ILogger<FloatAdminController> logger)
    {
        _spaCheckService = spaCheckService;
        _matchingService = matchingService;
        _floatApiClient = floatApiClient;
        _currentUserService = currentUserService;
        _dbContext = dbContext;
        _floatSettings = floatSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Get Float integration status and connection test results.
    /// </summary>
    /// <returns>Integration status including configuration and connection test</returns>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken ct)
    {
        var status = new FloatStatusResponse
        {
            IsEnabled = _floatSettings.Enabled,
            IsConfigured = _floatApiClient.IsConfigured,
            SpaCheckCronExpression = _floatSettings.SpaCheckCronExpression,
            SpaCheckGracePeriodMinutes = _floatSettings.SpaCheckGracePeriodMinutes
        };

        if (_floatApiClient.IsConfigured && _floatSettings.Enabled)
        {
            try
            {
                var people = await _floatApiClient.GetPeopleAsync(ct);
                var projects = await _floatApiClient.GetProjectsAsync(ct);

                status.ConnectionTest = new ConnectionTestResult
                {
                    Success = true,
                    PeopleCount = people.Count,
                    ProjectsCount = projects.Count,
                    TestedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Float API connection test failed");
                status.ConnectionTest = new ConnectionTestResult
                {
                    Success = false,
                    Error = ex.Message,
                    TestedAt = DateTime.UtcNow
                };
            }
        }

        return Ok(status);
    }

    /// <summary>
    /// Manually trigger Float sync to match people and projects with local entities.
    /// </summary>
    /// <returns>Sync results including matched and unmatched counts</returns>
    [HttpPost("sync")]
    public async Task<IActionResult> TriggerSync(CancellationToken ct)
    {
        if (!_floatSettings.Enabled)
        {
            return BadRequest(new { Error = "Float integration is not enabled" });
        }

        if (!_floatApiClient.IsConfigured)
        {
            return BadRequest(new { Error = "Float API is not configured" });
        }

        var tenantId = _currentUserService.TenantId;

        _logger.LogInformation(
            "Manual Float sync triggered by user {UserId} for tenant {TenantId}",
            _currentUserService.UserId,
            tenantId);

        try
        {
            var result = await _matchingService.SyncAndMatchAllAsync(tenantId, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Float sync failed for tenant {TenantId}", tenantId);
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Manually trigger SPA check for the current tenant.
    /// Immediately executes the check (blocking call).
    /// </summary>
    /// <param name="date">Optional date to check (default: today)</param>
    /// <returns>SPA check results including reminders sent</returns>
    [HttpPost("spa-check")]
    public async Task<IActionResult> TriggerSpaCheck([FromQuery] DateOnly? date, CancellationToken ct)
    {
        if (!_floatSettings.Enabled)
        {
            return BadRequest(new { Error = "Float integration is not enabled" });
        }

        if (!_floatApiClient.IsConfigured)
        {
            return BadRequest(new { Error = "Float API is not configured" });
        }

        var tenantId = _currentUserService.TenantId;
        var checkDate = date ?? DateOnly.FromDateTime(DateTime.Today);

        _logger.LogInformation(
            "Manual Float SPA check triggered by user {UserId} for tenant {TenantId}, date {Date}",
            _currentUserService.UserId,
            tenantId,
            checkDate);

        try
        {
            var result = await _spaCheckService.RunSpaCheckAsync(tenantId, checkDate, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Float SPA check failed for tenant {TenantId}", tenantId);
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Enqueue SPA check as a background job (non-blocking).
    /// Returns immediately while the job processes in the background.
    /// </summary>
    /// <returns>Job enqueue confirmation</returns>
    [HttpPost("spa-check/enqueue")]
    public IActionResult EnqueueSpaCheck()
    {
        if (!_floatSettings.Enabled)
        {
            return BadRequest(new { Error = "Float integration is not enabled" });
        }

        var tenantId = _currentUserService.TenantId;

        _logger.LogInformation(
            "Enqueuing Float SPA check job by user {UserId} for tenant {TenantId}",
            _currentUserService.UserId,
            tenantId);

        // Enqueue as background job
        var jobId = BackgroundJob.Enqueue<FloatSpaCheckJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        return Ok(new
        {
            Message = "SPA check job enqueued",
            JobId = jobId,
            TenantId = tenantId
        });
    }

    /// <summary>
    /// Get Float people list from the API (for debugging/verification).
    /// </summary>
    /// <returns>List of Float people</returns>
    [HttpGet("people")]
    public async Task<IActionResult> GetFloatPeople(CancellationToken ct)
    {
        if (!_floatApiClient.IsConfigured)
        {
            return BadRequest(new { Error = "Float API is not configured" });
        }

        try
        {
            var people = await _floatApiClient.GetPeopleAsync(ct);
            return Ok(new { Count = people.Count, People = people });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Float people");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get Float projects list from the API (for debugging/verification).
    /// </summary>
    /// <returns>List of Float projects</returns>
    [HttpGet("projects")]
    public async Task<IActionResult> GetFloatProjects(CancellationToken ct)
    {
        if (!_floatApiClient.IsConfigured)
        {
            return BadRequest(new { Error = "Float API is not configured" });
        }

        try
        {
            var projects = await _floatApiClient.GetProjectsAsync(ct);
            return Ok(new { Count = projects.Count, Projects = projects });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Float projects");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get Float scheduled tasks for a specific date (for debugging/verification).
    /// </summary>
    /// <param name="date">Date to check (default: today)</param>
    /// <returns>List of scheduled Float tasks</returns>
    [HttpGet("tasks")]
    public async Task<IActionResult> GetFloatTasks([FromQuery] DateOnly? date, CancellationToken ct)
    {
        if (!_floatApiClient.IsConfigured)
        {
            return BadRequest(new { Error = "Float API is not configured" });
        }

        var checkDate = date ?? DateOnly.FromDateTime(DateTime.Today);

        try
        {
            var tasks = await _floatApiClient.GetTasksForDateAsync(checkDate, ct);
            return Ok(new { Date = checkDate, Count = tasks.Count, Tasks = tasks });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Float tasks for date {Date}", checkDate);
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get summary of unmatched items.
    /// </summary>
    /// <returns>Count of pending unmatched people and projects</returns>
    [HttpGet("unmatched/summary")]
    public async Task<IActionResult> GetUnmatchedSummary(CancellationToken ct)
    {
        var tenantId = _currentUserService.TenantId;

        var summary = await _dbContext.FloatUnmatchedItems
            .IgnoreQueryFilters()
            .Where(u => u.TenantId == tenantId && !u.IsDeleted && u.Status == "Pending")
            .GroupBy(u => u.ItemType)
            .Select(g => new { ItemType = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return Ok(new FloatUnmatchedSummaryDto
        {
            PendingPeople = summary.FirstOrDefault(s => s.ItemType == "Person")?.Count ?? 0,
            PendingProjects = summary.FirstOrDefault(s => s.ItemType == "Project")?.Count ?? 0
        });
    }

    /// <summary>
    /// Get list of unmatched items.
    /// </summary>
    /// <param name="itemType">Filter by item type: Person or Project</param>
    /// <param name="status">Filter by status (default: Pending)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Paginated list of unmatched items</returns>
    [HttpGet("unmatched")]
    public async Task<IActionResult> GetUnmatchedItems(
        [FromQuery] string? itemType,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var tenantId = _currentUserService.TenantId;

        var query = _dbContext.FloatUnmatchedItems
            .IgnoreQueryFilters()
            .Where(u => u.TenantId == tenantId && !u.IsDeleted);

        if (!string.IsNullOrEmpty(itemType))
        {
            query = query.Where(u => u.ItemType == itemType);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(u => u.Status == status);
        }
        else
        {
            // Default to pending only
            query = query.Where(u => u.Status == "Pending");
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new FloatUnmatchedItemDto
            {
                Id = u.Id,
                ItemType = u.ItemType,
                FloatId = u.FloatId,
                FloatName = u.FloatName,
                FloatEmail = u.FloatEmail,
                SuggestedMatchId = u.SuggestedMatchId,
                SuggestedMatchName = u.SuggestedMatchName,
                MatchConfidence = u.MatchConfidence,
                Status = u.Status,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(new
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    /// <summary>
    /// Link a Float person to an employee.
    /// </summary>
    /// <param name="id">Unmatched item ID</param>
    /// <param name="request">Target employee ID</param>
    /// <returns>Success message</returns>
    [HttpPost("unmatched/{id}/link-person")]
    public async Task<IActionResult> LinkPerson(
        Guid id,
        [FromBody] LinkFloatItemRequest request,
        CancellationToken ct)
    {
        var tenantId = _currentUserService.TenantId;
        var userName = User.Identity?.Name ?? "Unknown";

        var unmatchedItem = await _dbContext.FloatUnmatchedItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId && !u.IsDeleted, ct);

        if (unmatchedItem == null)
            return NotFound(new { Error = "Unmatched item not found" });

        if (unmatchedItem.ItemType != "Person")
            return BadRequest(new { Error = "This item is not a person" });

        try
        {
            await _matchingService.LinkPersonToEmployeeAsync(
                tenantId,
                unmatchedItem.FloatId,
                request.TargetId,
                userName,
                ct);

            return Ok(new { Message = "Person linked successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Link a Float project to a site.
    /// </summary>
    /// <param name="id">Unmatched item ID</param>
    /// <param name="request">Target site ID</param>
    /// <returns>Success message</returns>
    [HttpPost("unmatched/{id}/link-project")]
    public async Task<IActionResult> LinkProject(
        Guid id,
        [FromBody] LinkFloatItemRequest request,
        CancellationToken ct)
    {
        var tenantId = _currentUserService.TenantId;
        var userName = User.Identity?.Name ?? "Unknown";

        var unmatchedItem = await _dbContext.FloatUnmatchedItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId && !u.IsDeleted, ct);

        if (unmatchedItem == null)
            return NotFound(new { Error = "Unmatched item not found" });

        if (unmatchedItem.ItemType != "Project")
            return BadRequest(new { Error = "This item is not a project" });

        try
        {
            await _matchingService.LinkProjectToSiteAsync(
                tenantId,
                unmatchedItem.FloatId,
                request.TargetId,
                userName,
                ct);

            return Ok(new { Message = "Project linked successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Ignore an unmatched item.
    /// </summary>
    /// <param name="id">Unmatched item ID</param>
    /// <returns>Success message</returns>
    [HttpPost("unmatched/{id}/ignore")]
    public async Task<IActionResult> IgnoreItem(Guid id, CancellationToken ct)
    {
        var tenantId = _currentUserService.TenantId;
        var userName = User.Identity?.Name ?? "Unknown";

        try
        {
            await _matchingService.IgnoreUnmatchedItemAsync(tenantId, id, userName, ct);
            return Ok(new { Message = "Item ignored" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get employees available for linking (no Float link yet).
    /// </summary>
    /// <param name="search">Optional search term</param>
    /// <returns>List of available employees</returns>
    [HttpGet("available-employees")]
    public async Task<IActionResult> GetAvailableEmployees(
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var tenantId = _currentUserService.TenantId;

        var query = _dbContext.Employees
            .IgnoreQueryFilters()
            .Where(e => e.TenantId == tenantId
                && !e.IsDeleted
                && e.IsActive
                && e.FloatPersonId == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(searchLower) ||
                e.LastName.ToLower().Contains(searchLower) ||
                (e.Email != null && e.Email.ToLower().Contains(searchLower)) ||
                e.EmployeeCode.ToLower().Contains(searchLower));
        }

        var employees = await query
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .Take(50)
            .Select(e => new AvailableEmployeeDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email
            })
            .ToListAsync(ct);

        return Ok(employees);
    }

    /// <summary>
    /// Get sites available for linking (no Float link yet).
    /// </summary>
    /// <param name="search">Optional search term</param>
    /// <returns>List of available sites</returns>
    [HttpGet("available-sites")]
    public async Task<IActionResult> GetAvailableSites(
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var tenantId = _currentUserService.TenantId;

        var query = _dbContext.Sites
            .IgnoreQueryFilters()
            .Where(s => s.TenantId == tenantId
                && !s.IsDeleted
                && s.IsActive
                && s.FloatProjectId == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(s => s.SiteName.ToLower().Contains(searchLower));
        }

        var sites = await query
            .OrderBy(s => s.SiteName)
            .Take(50)
            .Select(s => new AvailableSiteDto
            {
                Id = s.Id,
                Name = s.SiteName,
                Address = s.Address
            })
            .ToListAsync(ct);

        return Ok(sites);
    }
}

/// <summary>
/// Response model for Float integration status.
/// </summary>
public record FloatStatusResponse
{
    public bool IsEnabled { get; init; }
    public bool IsConfigured { get; init; }
    public string SpaCheckCronExpression { get; init; } = string.Empty;
    public int SpaCheckGracePeriodMinutes { get; init; }
    public ConnectionTestResult? ConnectionTest { get; set; }
}

/// <summary>
/// Result of Float API connection test.
/// </summary>
public record ConnectionTestResult
{
    public bool Success { get; init; }
    public int PeopleCount { get; init; }
    public int ProjectsCount { get; init; }
    public string? Error { get; init; }
    public DateTime TestedAt { get; init; }
}

/// <summary>
/// Summary of unmatched Float items.
/// </summary>
public record FloatUnmatchedSummaryDto
{
    public int PendingPeople { get; init; }
    public int PendingProjects { get; init; }
    public int TotalPending => PendingPeople + PendingProjects;
}

/// <summary>
/// DTO for a Float unmatched item.
/// </summary>
public record FloatUnmatchedItemDto
{
    public Guid Id { get; init; }
    public string ItemType { get; init; } = string.Empty;
    public int FloatId { get; init; }
    public string FloatName { get; init; } = string.Empty;
    public string? FloatEmail { get; init; }
    public Guid? SuggestedMatchId { get; init; }
    public string? SuggestedMatchName { get; init; }
    public decimal? MatchConfidence { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Request to link a Float item to a target entity.
/// </summary>
public record LinkFloatItemRequest
{
    public Guid TargetId { get; init; }
}

/// <summary>
/// DTO for an available employee (without Float link).
/// </summary>
public record AvailableEmployeeDto
{
    public Guid Id { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string? Email { get; init; }
}

/// <summary>
/// DTO for an available site (without Float link).
/// </summary>
public record AvailableSiteDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Address { get; init; }
}
