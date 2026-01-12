using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Application.Services;

namespace Rascor.API.Controllers;

[ApiController]
[Route("api/rams/dashboard")]
[Authorize(Policy = "Rams.View")]
public class RamsDashboardController : ControllerBase
{
    private readonly IRamsDashboardService _dashboardService;
    private readonly ILogger<RamsDashboardController> _logger;

    public RamsDashboardController(
        IRamsDashboardService dashboardService,
        ILogger<RamsDashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard data including summary stats, charts, and lists
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        try
        {
            var dashboard = await _dashboardService.GetDashboardAsync(cancellationToken);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving RAMS dashboard data");
            return StatusCode(500, new { message = "Error retrieving dashboard data" });
        }
    }

    /// <summary>
    /// Get all pending approvals
    /// </summary>
    [HttpGet("pending-approvals")]
    public async Task<IActionResult> GetPendingApprovals(CancellationToken cancellationToken)
    {
        try
        {
            var pendingApprovals = await _dashboardService.GetPendingApprovalsAsync(cancellationToken);
            return Ok(pendingApprovals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending approvals");
            return StatusCode(500, new { message = "Error retrieving pending approvals" });
        }
    }

    /// <summary>
    /// Get all overdue documents
    /// </summary>
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdueDocuments(CancellationToken cancellationToken)
    {
        try
        {
            var overdueDocuments = await _dashboardService.GetOverdueDocumentsAsync(cancellationToken);
            return Ok(overdueDocuments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue documents");
            return StatusCode(500, new { message = "Error retrieving overdue documents" });
        }
    }

    /// <summary>
    /// Export RAMS data to Excel
    /// </summary>
    [HttpPost("export")]
    public async Task<IActionResult> ExportToExcel(
        [FromBody] RamsExportRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var excelBytes = await _dashboardService.ExportToExcelAsync(request, cancellationToken);
            var fileName = $"RAMS_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

            _logger.LogInformation("Generated RAMS Excel export: {FileName}", fileName);

            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export RAMS data to Excel");
            return StatusCode(500, new { message = "Failed to generate export" });
        }
    }
}
