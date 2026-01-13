using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Infrastructure.Sync;

namespace Rascor.API.Controllers;

[ApiController]
[Route("api/admin/geofence-sync")]
[Authorize(Roles = "Admin")]
public class GeofenceSyncController : ControllerBase
{
    private readonly IGeofenceSyncService _syncService;
    private readonly ICurrentUserService _currentUserService;

    public GeofenceSyncController(
        IGeofenceSyncService syncService,
        ICurrentUserService currentUserService)
    {
        _syncService = syncService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Manually trigger a geofence sync operation for testing
    /// </summary>
    /// <returns>Sync results including records processed, created, skipped, and any errors</returns>
    [HttpPost("run")]
    public async Task<IActionResult> RunSync(CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        var stopwatch = Stopwatch.StartNew();

        var result = await _syncService.SyncAsync(tenantId, cancellationToken);

        stopwatch.Stop();

        var dto = new GeofenceSyncResultDto(
            result.Success,
            result.RecordsProcessed,
            result.RecordsCreated,
            result.RecordsSkipped,
            result.ErrorMessage,
            result.LastEventTimestamp,
            stopwatch.Elapsed.TotalMilliseconds,
            result.DatesProcessed,
            result.SummariesCreated,
            result.EventsProcessedForSummaries);

        if (!result.Success)
        {
            return Ok(Result.Fail<GeofenceSyncResultDto>(result.ErrorMessage ?? "Sync failed"));
        }

        return Ok(Result.Ok(dto));
    }

    /// <summary>
    /// Get the current sync status and recent sync history
    /// </summary>
    /// <returns>Sync status including health, last sync time, and recent sync logs</returns>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        var status = await _syncService.GetSyncStatusAsync(tenantId, cancellationToken);

        return Ok(Result.Ok(status));
    }

    /// <summary>
    /// Get a list of device IDs from the mobile database that don't have matching employees
    /// </summary>
    /// <returns>List of unmapped devices with event counts to help prioritize mapping</returns>
    [HttpGet("unmapped")]
    public async Task<IActionResult> GetUnmappedDevices(CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        var unmappedDevices = await _syncService.GetUnmappedDevicesAsync(tenantId, cancellationToken);

        return Ok(Result.Ok(unmappedDevices));
    }
}
