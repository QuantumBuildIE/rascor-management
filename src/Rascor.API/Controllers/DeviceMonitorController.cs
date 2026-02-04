using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Infrastructure.Data;
using Rascor.Core.Infrastructure.Float;
using Rascor.Core.Infrastructure.Float.Models;
using Rascor.Modules.SiteAttendance.Infrastructure.Persistence;

namespace Rascor.API.Controllers;

/// <summary>
/// Admin controller for device monitoring.
/// Provides real-time status of all geofence devices with employee and schedule data.
/// </summary>
[ApiController]
[Route("api/admin/device-monitor")]
[Authorize(Policy = "Core.Admin")]
public class DeviceMonitorController : ControllerBase
{
    private readonly SiteAttendanceDbContext _siteAttendanceDbContext;
    private readonly ApplicationDbContext _appDbContext;
    private readonly IFloatApiClient _floatApiClient;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeviceMonitorController> _logger;

    // Thresholds for device status
    private const int OnlineThresholdMinutes = 90;
    private const int StaleThresholdHours = 4;

    // Distance thresholds for location matching (in meters)
    private const double OnSiteDistanceMeters = 500;
    private const double NearDistanceMeters = 2000;

    public DeviceMonitorController(
        SiteAttendanceDbContext siteAttendanceDbContext,
        ApplicationDbContext appDbContext,
        IFloatApiClient floatApiClient,
        ICurrentUserService currentUserService,
        ILogger<DeviceMonitorController> logger)
    {
        _siteAttendanceDbContext = siteAttendanceDbContext;
        _appDbContext = appDbContext;
        _floatApiClient = floatApiClient;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get device monitoring data with employee and schedule information.
    /// </summary>
    /// <returns>List of devices with their current status and location</returns>
    [HttpGet]
    public async Task<IActionResult> GetDeviceMonitor(CancellationToken ct)
    {
        var tenantId = _currentUserService.TenantId;
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.Today);

        try
        {
            // 1. Get all device status cache records
            var deviceStatuses = await _siteAttendanceDbContext.DeviceStatusCaches
                .ToListAsync(ct);

            if (!deviceStatuses.Any())
            {
                return Ok(new List<DeviceMonitorDto>());
            }

            // 2. Get employees with GeoTrackerID for this tenant
            var employees = await _appDbContext.Employees
                .IgnoreQueryFilters()
                .Where(e => e.TenantId == tenantId && !e.IsDeleted && e.GeoTrackerID != null)
                .ToListAsync(ct);

            var employeesByGeoTrackerId = employees.ToDictionary(e => e.GeoTrackerID!, e => e);

            // 3. Get today's Float tasks if Float is configured
            var floatTasks = new List<FloatTask>();
            var floatProjects = new List<FloatProject>();

            if (_floatApiClient.IsConfigured)
            {
                try
                {
                    floatTasks = await _floatApiClient.GetTasksForDateAsync(today, ct);
                    floatProjects = await _floatApiClient.GetProjectsAsync(ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch Float tasks/projects for device monitor");
                }
            }

            var projectsDict = floatProjects.ToDictionary(p => p.ProjectId);

            // 4. Get sites with Float project links for this tenant
            var sites = await _appDbContext.Sites
                .IgnoreQueryFilters()
                .Where(s => s.TenantId == tenantId && !s.IsDeleted && s.FloatProjectId != null)
                .ToListAsync(ct);

            var sitesByFloatProjectId = sites.ToDictionary(s => s.FloatProjectId!.Value);

            // Build task-to-site lookup by FloatPersonId (exclude tasks without PeopleId)
            var tasksByPersonId = floatTasks
                .Where(t => t.PeopleId.HasValue)
                .GroupBy(t => t.PeopleId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 5. Build the result DTOs
            var results = new List<DeviceMonitorDto>();

            foreach (var device in deviceStatuses)
            {
                var dto = new DeviceMonitorDto
                {
                    DeviceId = device.DeviceId,
                    DeviceModel = device.Model,
                    Status = GetDeviceStatus(device.LastSeenAt, now),
                    LastSeenAt = device.LastSeenAt,
                    BatteryLevel = device.LastBatteryLevel,
                    LastLatitude = device.LastLatitude,
                    LastLongitude = device.LastLongitude
                };

                // Try to match to employee
                if (employeesByGeoTrackerId.TryGetValue(device.DeviceId, out var employee))
                {
                    dto.EmployeeId = employee.Id;
                    dto.EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim();

                    // Try to get scheduled site from Float
                    if (employee.FloatPersonId.HasValue &&
                        tasksByPersonId.TryGetValue(employee.FloatPersonId.Value, out var employeeTasks))
                    {
                        // Find the first task that maps to a known site
                        foreach (var task in employeeTasks)
                        {
                            if (!task.ProjectId.HasValue)
                                continue;

                            if (sitesByFloatProjectId.TryGetValue(task.ProjectId.Value, out var site))
                            {
                                dto.ScheduledSiteName = site.SiteName;
                                dto.ScheduledSiteLatitude = site.Latitude;
                                dto.ScheduledSiteLongitude = site.Longitude;
                                break;
                            }
                            else if (projectsDict.TryGetValue(task.ProjectId.Value, out var project))
                            {
                                // Site not linked but we have the project name
                                dto.ScheduledSiteName = project.Name;
                            }
                        }
                    }
                }

                // Calculate location match if we have both device and site coordinates
                dto.LocationMatch = CalculateLocationMatch(
                    device.LastLatitude, device.LastLongitude,
                    dto.ScheduledSiteLatitude, dto.ScheduledSiteLongitude);

                results.Add(dto);
            }

            // Default sort: Offline first, then Stale, then Online
            results = results
                .OrderBy(r => r.Status switch
                {
                    "Offline" => 0,
                    "Stale" => 1,
                    "Online" => 2,
                    _ => 3
                })
                .ThenBy(r => r.EmployeeName ?? "")
                .ToList();

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get device monitor data for tenant {TenantId}", tenantId);
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get summary statistics for device monitoring.
    /// </summary>
    /// <returns>Summary with counts by status and location</returns>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var tenantId = _currentUserService.TenantId;
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.Today);

        try
        {
            // Get all device statuses
            var deviceStatuses = await _siteAttendanceDbContext.DeviceStatusCaches
                .ToListAsync(ct);

            if (!deviceStatuses.Any())
            {
                return Ok(new DeviceMonitorSummaryDto
                {
                    TotalDevices = 0,
                    Online = 0,
                    Stale = 0,
                    Offline = 0,
                    OnSite = 0,
                    LastSyncedAt = null
                });
            }

            // Count by status
            var onlineThreshold = now.AddMinutes(-OnlineThresholdMinutes);
            var staleThreshold = now.AddHours(-StaleThresholdHours);

            var online = 0;
            var stale = 0;
            var offline = 0;
            var onSite = 0;
            DateTime? lastSyncedAt = null;

            // Get employees and Float data for location matching
            var employees = await _appDbContext.Employees
                .IgnoreQueryFilters()
                .Where(e => e.TenantId == tenantId && !e.IsDeleted && e.GeoTrackerID != null)
                .ToListAsync(ct);

            var employeesByGeoTrackerId = employees.ToDictionary(e => e.GeoTrackerID!, e => e);

            // Get Float tasks and sites for location matching
            var floatTasks = new List<FloatTask>();
            if (_floatApiClient.IsConfigured)
            {
                try
                {
                    floatTasks = await _floatApiClient.GetTasksForDateAsync(today, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch Float tasks for device monitor summary");
                }
            }

            var tasksByPersonId = floatTasks
                .Where(t => t.PeopleId.HasValue)
                .GroupBy(t => t.PeopleId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var sites = await _appDbContext.Sites
                .IgnoreQueryFilters()
                .Where(s => s.TenantId == tenantId && !s.IsDeleted && s.FloatProjectId != null)
                .ToListAsync(ct);

            var sitesByFloatProjectId = sites.ToDictionary(s => s.FloatProjectId!.Value);

            foreach (var device in deviceStatuses)
            {
                // Update last synced time
                if (!lastSyncedAt.HasValue || device.SyncedAt > lastSyncedAt.Value)
                {
                    lastSyncedAt = device.SyncedAt;
                }

                // Count status
                var status = GetDeviceStatus(device.LastSeenAt, now);
                switch (status)
                {
                    case "Online":
                        online++;
                        break;
                    case "Stale":
                        stale++;
                        break;
                    case "Offline":
                        offline++;
                        break;
                }

                // Check if device is on site
                if (device.LastLatitude.HasValue && device.LastLongitude.HasValue &&
                    employeesByGeoTrackerId.TryGetValue(device.DeviceId, out var employee) &&
                    employee.FloatPersonId.HasValue &&
                    tasksByPersonId.TryGetValue(employee.FloatPersonId.Value, out var employeeTasks))
                {
                    foreach (var task in employeeTasks)
                    {
                        if (task.ProjectId.HasValue &&
                            sitesByFloatProjectId.TryGetValue(task.ProjectId.Value, out var site) &&
                            site.Latitude.HasValue && site.Longitude.HasValue)
                        {
                            var distance = CalculateHaversineDistance(
                                (double)device.LastLatitude.Value, (double)device.LastLongitude.Value,
                                (double)site.Latitude.Value, (double)site.Longitude.Value);

                            if (distance <= OnSiteDistanceMeters)
                            {
                                onSite++;
                                break;
                            }
                        }
                    }
                }
            }

            return Ok(new DeviceMonitorSummaryDto
            {
                TotalDevices = deviceStatuses.Count,
                Online = online,
                Stale = stale,
                Offline = offline,
                OnSite = onSite,
                LastSyncedAt = lastSyncedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get device monitor summary for tenant {TenantId}", tenantId);
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Determines device status based on LastSeenAt timestamp.
    /// </summary>
    private static string GetDeviceStatus(DateTime? lastSeenAt, DateTime now)
    {
        if (!lastSeenAt.HasValue)
            return "Offline";

        var timeSinceLastSeen = now - lastSeenAt.Value;

        if (timeSinceLastSeen.TotalMinutes <= OnlineThresholdMinutes)
            return "Online";

        if (timeSinceLastSeen.TotalHours <= StaleThresholdHours)
            return "Stale";

        return "Offline";
    }

    /// <summary>
    /// Calculates location match between device and scheduled site.
    /// </summary>
    private static string CalculateLocationMatch(
        decimal? deviceLat, decimal? deviceLon,
        decimal? siteLat, decimal? siteLon)
    {
        // No GPS data from device
        if (!deviceLat.HasValue || !deviceLon.HasValue)
            return "Unknown";

        // No scheduled site coordinates
        if (!siteLat.HasValue || !siteLon.HasValue)
            return "Unknown";

        var distance = CalculateHaversineDistance(
            (double)deviceLat.Value, (double)deviceLon.Value,
            (double)siteLat.Value, (double)siteLon.Value);

        if (distance <= OnSiteDistanceMeters)
            return "OnSite";

        if (distance <= NearDistanceMeters)
            return "Near";

        return "Away";
    }

    /// <summary>
    /// Calculates the distance between two GPS coordinates using the Haversine formula.
    /// </summary>
    /// <param name="lat1">Latitude of point 1 (degrees)</param>
    /// <param name="lon1">Longitude of point 1 (degrees)</param>
    /// <param name="lat2">Latitude of point 2 (degrees)</param>
    /// <param name="lon2">Longitude of point 2 (degrees)</param>
    /// <returns>Distance in meters</returns>
    private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double EarthRadiusMeters = 6371000; // Earth's radius in meters

        var lat1Rad = lat1 * Math.PI / 180;
        var lat2Rad = lat2 * Math.PI / 180;
        var deltaLat = (lat2 - lat1) * Math.PI / 180;
        var deltaLon = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;
    }
}

/// <summary>
/// DTO for device monitoring data.
/// </summary>
public class DeviceMonitorDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string? DeviceModel { get; set; }
    public string? EmployeeName { get; set; }
    public Guid? EmployeeId { get; set; }
    public string Status { get; set; } = "Unknown";  // "Online", "Stale", "Offline"
    public DateTime? LastSeenAt { get; set; }
    public int? BatteryLevel { get; set; }
    public decimal? LastLatitude { get; set; }
    public decimal? LastLongitude { get; set; }
    public string? ScheduledSiteName { get; set; }  // From Float
    public decimal? ScheduledSiteLatitude { get; set; }
    public decimal? ScheduledSiteLongitude { get; set; }
    public string? LocationMatch { get; set; }  // "OnSite", "Near", "Away", "Unknown"
}

/// <summary>
/// Summary statistics for device monitoring.
/// </summary>
public class DeviceMonitorSummaryDto
{
    public int TotalDevices { get; set; }
    public int Online { get; set; }
    public int Stale { get; set; }
    public int Offline { get; set; }
    public int OnSite { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}
