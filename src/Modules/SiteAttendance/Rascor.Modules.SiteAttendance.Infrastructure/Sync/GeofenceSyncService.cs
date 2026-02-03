using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rascor.Core.Domain.Entities;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Services;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Enums;
using Rascor.Modules.SiteAttendance.Infrastructure.Persistence;
using Rascor.Modules.SiteAttendance.Infrastructure.Sync.Models;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Sync;

/// <summary>
/// Background service that periodically syncs geofence events from the mobile database to Rascor.
/// Runs every 15 minutes (configurable) and processes new events incrementally.
/// </summary>
public class GeofenceSyncService : BackgroundService, IGeofenceSyncService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GeofenceSyncService> _logger;
    private readonly GeofenceSyncSettings _settings;

    public GeofenceSyncService(
        IServiceScopeFactory scopeFactory,
        ILogger<GeofenceSyncService> logger,
        IOptions<GeofenceSyncSettings> settings)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Geofence sync service is disabled");
            return;
        }

        _logger.LogInformation(
            "Geofence sync service started. Interval: {IntervalMinutes} minutes",
            _settings.IntervalMinutes);

        // Initial delay to allow application to fully start
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var tenantId = _settings.GetDefaultTenantGuid();
                _logger.LogInformation("Starting geofence sync for tenant {TenantId}", tenantId);

                var result = await SyncAsync(tenantId, stoppingToken);

                if (result.Success)
                {
                    if (result.SummariesCreated > 0 || result.EventsProcessedForSummaries > 0)
                    {
                        _logger.LogInformation(
                            "Geofence sync completed. Events - Processed: {Processed}, Created: {Created}, Skipped: {Skipped}. Summaries - Updated: {SummariesCreated}, Events processed: {EventsProcessed}",
                            result.RecordsProcessed,
                            result.RecordsCreated,
                            result.RecordsSkipped,
                            result.SummariesCreated,
                            result.EventsProcessedForSummaries);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Geofence sync completed. Processed: {Processed}, Created: {Created}, Skipped: {Skipped}",
                            result.RecordsProcessed,
                            result.RecordsCreated,
                            result.RecordsSkipped);
                    }
                }
                else
                {
                    _logger.LogError(
                        "Geofence sync failed: {Error}",
                        result.ErrorMessage);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Geofence sync service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during geofence sync");
            }

            // Wait for next interval
            await Task.Delay(TimeSpan.FromMinutes(_settings.IntervalMinutes), stoppingToken);
        }

        _logger.LogInformation("Geofence sync service stopped");
    }

    /// <summary>
    /// Executes a sync operation for the specified tenant
    /// </summary>
    public async Task<GeofenceSyncResult> SyncAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var rascorDb = scope.ServiceProvider.GetRequiredService<SiteAttendanceDbContext>();

        GeofenceMobileDbContext? mobileDb = null;
        try
        {
            mobileDb = scope.ServiceProvider.GetService<GeofenceMobileDbContext>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Mobile database context not available - check connection string configuration");
        }

        // Create sync log entry
        var syncLog = GeofenceSyncLog.Start(tenantId);
        rascorDb.GeofenceSyncLogs.Add(syncLog);
        await rascorDb.SaveChangesAsync(cancellationToken);

        try
        {
            // Check if mobile database is available
            if (mobileDb == null)
            {
                const string error = "Mobile database is not configured. Set GeofenceMobileDb connection string.";
                syncLog.Fail(error);
                await rascorDb.SaveChangesAsync(cancellationToken);
                return new GeofenceSyncResult(false, 0, 0, 0, error);
            }

            // Test connectivity
            if (!await CanConnectToMobileDbAsync(mobileDb, cancellationToken))
            {
                const string error = "Unable to connect to mobile geofence database";
                syncLog.Fail(error);
                await rascorDb.SaveChangesAsync(cancellationToken);
                return new GeofenceSyncResult(false, 0, 0, 0, error);
            }

            // Sync device status from mobile DB to local cache (runs regardless of new events)
            var (devicesSynced, devicesOnline) = await SyncDeviceStatusAsync(mobileDb, rascorDb, cancellationToken);
            _logger.LogInformation(
                "Device status sync: {Count} devices, {OnlineCount} online",
                devicesSynced,
                devicesOnline);

            // Get last successful sync timestamp
            var lastSync = await GetLastSuccessfulSyncAsync(rascorDb, tenantId, cancellationToken);
            var syncFromTimestamp = lastSync?.LastEventTimestamp
                ?? DateTime.UtcNow.AddDays(-_settings.InitialSyncDays);

            _logger.LogDebug(
                "Syncing events from {FromTimestamp}. Last sync: {LastSync}",
                syncFromTimestamp,
                lastSync?.SyncCompleted?.ToString() ?? "Never");

            // Load lookup data (employees by GeoTrackerID, sites by SiteCode)
            var employeesByGeoTrackerId = await LoadEmployeeLookupAsync(rascorDb, tenantId, cancellationToken);
            var sitesBySiteCode = await LoadSiteLookupAsync(rascorDb, tenantId, cancellationToken);

            _logger.LogDebug(
                "Loaded {EmployeeCount} employees with GeoTrackerID, {SiteCount} sites",
                employeesByGeoTrackerId.Count,
                sitesBySiteCode.Count);

            // Fetch events from mobile database
            var mobileEvents = await FetchMobileEventsAsync(
                mobileDb,
                syncFromTimestamp,
                _settings.BatchSize,
                cancellationToken);

            if (!mobileEvents.Any())
            {
                _logger.LogDebug("No new events to sync");
                syncLog.UpdateProgress(0, 0, 0, null, syncFromTimestamp);
                syncLog.Complete();
                await rascorDb.SaveChangesAsync(cancellationToken);
                return new GeofenceSyncResult(true, 0, 0, 0, LastEventTimestamp: syncFromTimestamp);
            }

            int processed = 0;
            int created = 0;
            int skipped = 0;
            string? lastEventId = null;
            DateTime? lastEventTimestamp = null;
            var uniqueDates = new HashSet<DateOnly>();

            // Track skip reasons for batch logging (reduces log volume)
            var unmappedEmployees = new Dictionary<string, int>();  // GeoTrackerId -> count
            var unmappedSites = new Dictionary<string, int>();      // SiteCode -> count

            foreach (var mobileEvent in mobileEvents)
            {
                processed++;
                lastEventId = mobileEvent.Id;
                lastEventTimestamp = mobileEvent.Timestamp;

                // Map device_id (UserId field contains EVT####) to Employee
                if (!employeesByGeoTrackerId.TryGetValue(mobileEvent.UserId, out var employee))
                {
                    unmappedEmployees.TryGetValue(mobileEvent.UserId, out var empCount);
                    unmappedEmployees[mobileEvent.UserId] = empCount + 1;
                    skipped++;
                    continue;
                }

                // Map site_id to Site
                if (!sitesBySiteCode.TryGetValue(mobileEvent.SiteId, out var site))
                {
                    unmappedSites.TryGetValue(mobileEvent.SiteId, out var siteCount);
                    unmappedSites[mobileEvent.SiteId] = siteCount + 1;
                    skipped++;
                    continue;
                }

                // Check for duplicate event
                var isDuplicate = await IsDuplicateEventAsync(
                    rascorDb,
                    employee.Id,
                    site.Id,
                    mobileEvent.Timestamp,
                    ParseEventType(mobileEvent.EventType),
                    cancellationToken);

                if (isDuplicate)
                {
                    _logger.LogDebug(
                        "Skipping duplicate event {EventId} for employee {EmployeeId} at site {SiteId}",
                        mobileEvent.Id,
                        employee.Id,
                        site.Id);
                    skipped++;
                    continue;
                }

                // Create AttendanceEvent in Rascor
                // Cast double? to decimal? since mobile DB uses double precision but AttendanceEvent uses decimal
                var attendanceEvent = AttendanceEvent.Create(
                    tenantId,
                    employee.Id,
                    site.Id,
                    ParseEventType(mobileEvent.EventType),
                    mobileEvent.Timestamp,
                    (decimal?)mobileEvent.Latitude,
                    (decimal?)mobileEvent.Longitude,
                    ParseTriggerMethod(mobileEvent.TriggerMethod));

                rascorDb.AttendanceEvents.Add(attendanceEvent);
                created++;

                // Track the date for summary processing
                uniqueDates.Add(DateOnly.FromDateTime(mobileEvent.Timestamp));

                // Save in batches to avoid memory issues
                if (created % 100 == 0)
                {
                    await rascorDb.SaveChangesAsync(cancellationToken);
                    _logger.LogDebug("Saved batch of events. Created so far: {Created}", created);
                }
            }

            // Log summary of skipped events (batch logging to reduce log volume)
            if (unmappedEmployees.Count > 0)
            {
                var topUnmappedEmployees = string.Join(", ", unmappedEmployees
                    .OrderByDescending(x => x.Value)
                    .Take(10)
                    .Select(x => $"{x.Key}({x.Value})"));
                _logger.LogWarning(
                    "Skipped {TotalCount} events for {DeviceCount} unmapped GeoTrackerIDs. Top: {Devices}",
                    unmappedEmployees.Values.Sum(),
                    unmappedEmployees.Count,
                    topUnmappedEmployees);
            }

            if (unmappedSites.Count > 0)
            {
                var topUnmappedSites = string.Join(", ", unmappedSites
                    .OrderByDescending(x => x.Value)
                    .Take(10)
                    .Select(x => $"{x.Key}({x.Value})"));
                _logger.LogWarning(
                    "Skipped {TotalCount} events for {SiteCount} unmapped SiteCodes. Top: {Sites}",
                    unmappedSites.Values.Sum(),
                    unmappedSites.Count,
                    topUnmappedSites);
            }

            // Final save
            syncLog.UpdateProgress(processed, created, skipped, lastEventId, lastEventTimestamp);
            syncLog.Complete();
            await rascorDb.SaveChangesAsync(cancellationToken);

            // Process summaries for affected dates if enabled
            var datesProcessedList = uniqueDates.OrderBy(d => d).ToList();
            var totalSummariesCreated = 0;
            var totalEventsProcessedForSummaries = 0;

            if (_settings.ProcessSummariesAfterSync && datesProcessedList.Count > 0)
            {
                _logger.LogInformation(
                    "Processing summaries for dates: {Dates}",
                    string.Join(", ", datesProcessedList.Select(d => d.ToString("yyyy-MM-dd"))));

                var timeCalculationService = scope.ServiceProvider.GetRequiredService<ITimeCalculationService>();

                foreach (var date in datesProcessedList)
                {
                    try
                    {
                        var (eventsProcessed, summariesCreated) = await timeCalculationService
                            .ProcessDailyAttendanceWithCountsAsync(tenantId, date, cancellationToken);

                        totalEventsProcessedForSummaries += eventsProcessed;
                        totalSummariesCreated += summariesCreated;

                        _logger.LogDebug(
                            "Processed summaries for {Date}: {EventsProcessed} events, {SummariesCreated} summaries",
                            date,
                            eventsProcessed,
                            summariesCreated);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Failed to process summaries for date {Date}. Will be processed by nightly job.",
                            date);
                        // Continue processing other dates even if one fails
                    }
                }

                _logger.LogInformation(
                    "Summary processing completed. Summaries updated: {SummariesCreated}, Events processed: {EventsProcessed}",
                    totalSummariesCreated,
                    totalEventsProcessedForSummaries);
            }

            return new GeofenceSyncResult(
                true,
                processed,
                created,
                skipped,
                LastEventTimestamp: lastEventTimestamp,
                DatesProcessed: datesProcessedList,
                SummariesCreated: totalSummariesCreated,
                EventsProcessedForSummaries: totalEventsProcessedForSummaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during geofence sync");
            syncLog.Fail(ex.Message);
            await rascorDb.SaveChangesAsync(cancellationToken);
            return new GeofenceSyncResult(false, 0, 0, 0, ex.Message);
        }
    }

    /// <summary>
    /// Tests connectivity to the mobile database
    /// </summary>
    private async Task<bool> CanConnectToMobileDbAsync(
        GeofenceMobileDbContext mobileDb,
        CancellationToken cancellationToken)
    {
        try
        {
            await mobileDb.Database.CanConnectAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to mobile geofence database");
            return false;
        }
    }

    /// <summary>
    /// Gets the last successful sync log entry for the tenant
    /// </summary>
    private async Task<GeofenceSyncLog?> GetLastSuccessfulSyncAsync(
        SiteAttendanceDbContext db,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        return await db.GeofenceSyncLogs
            .Where(s => s.TenantId == tenantId
                && s.SyncCompleted != null
                && s.ErrorMessage == null)
            .OrderByDescending(s => s.SyncCompleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Loads employees with GeoTrackerID for the tenant, keyed by GeoTrackerID
    /// </summary>
    private async Task<Dictionary<string, Employee>> LoadEmployeeLookupAsync(
        SiteAttendanceDbContext db,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var employees = await db.Set<Employee>()
            .Where(e => e.TenantId == tenantId
                && e.GeoTrackerID != null
                && !e.IsDeleted)
            .ToListAsync(cancellationToken);

        return employees.ToDictionary(e => e.GeoTrackerID!, e => e);
    }

    /// <summary>
    /// Loads sites for the tenant, keyed by SiteCode
    /// </summary>
    private async Task<Dictionary<string, Site>> LoadSiteLookupAsync(
        SiteAttendanceDbContext db,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var sites = await db.Set<Site>()
            .Where(s => s.TenantId == tenantId && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        return sites.ToDictionary(s => s.SiteCode, s => s);
    }

    /// <summary>
    /// Fetches mobile events after the given timestamp
    /// </summary>
    private async Task<List<MobileGeofenceEvent>> FetchMobileEventsAsync(
        GeofenceMobileDbContext mobileDb,
        DateTime fromTimestamp,
        int batchSize,
        CancellationToken cancellationToken)
    {
        return await mobileDb.GeofenceEvents
            .Where(e => e.Timestamp > fromTimestamp)
            .OrderBy(e => e.Timestamp)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if an equivalent event already exists (duplicate detection)
    /// </summary>
    private async Task<bool> IsDuplicateEventAsync(
        SiteAttendanceDbContext db,
        Guid employeeId,
        Guid siteId,
        DateTime timestamp,
        EventType eventType,
        CancellationToken cancellationToken)
    {
        // Check for exact match within 1 second window (to handle timestamp precision differences)
        var timestampStart = timestamp.AddSeconds(-1);
        var timestampEnd = timestamp.AddSeconds(1);

        return await db.AttendanceEvents
            .AnyAsync(e =>
                e.EmployeeId == employeeId
                && e.SiteId == siteId
                && e.EventType == eventType
                && e.Timestamp >= timestampStart
                && e.Timestamp <= timestampEnd
                && !e.IsDeleted,
                cancellationToken);
    }

    /// <summary>
    /// Parses event type string from mobile database to enum
    /// </summary>
    private static EventType ParseEventType(string eventType)
    {
        return eventType.ToLowerInvariant() switch
        {
            "enter" => EventType.Enter,
            "exit" => EventType.Exit,
            _ => EventType.Enter // Default to Enter if unknown
        };
    }

    /// <summary>
    /// Parses trigger method string from mobile database to enum
    /// </summary>
    private static TriggerMethod ParseTriggerMethod(string triggerMethod)
    {
        return triggerMethod.ToLowerInvariant() switch
        {
            "automatic" => TriggerMethod.Automatic,
            "manual" => TriggerMethod.Manual,
            _ => TriggerMethod.Automatic // Default to Automatic
        };
    }

    /// <summary>
    /// Gets the sync status including recent sync history
    /// </summary>
    public async Task<GeofenceSyncStatusDto> GetSyncStatusAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var rascorDb = scope.ServiceProvider.GetRequiredService<SiteAttendanceDbContext>();

        var last24Hours = DateTime.UtcNow.AddHours(-24);

        var recentSyncs = await rascorDb.GeofenceSyncLogs
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.SyncStarted)
            .Take(10)
            .Select(s => new GeofenceSyncLogDto(
                s.Id,
                s.SyncStarted,
                s.SyncCompleted,
                s.RecordsProcessed,
                s.RecordsCreated,
                s.RecordsSkipped,
                s.LastEventId,
                s.LastEventTimestamp,
                s.ErrorMessage,
                s.IsSuccess))
            .ToListAsync(cancellationToken);

        var syncsLast24Hours = await rascorDb.GeofenceSyncLogs
            .Where(s => s.TenantId == tenantId && s.SyncStarted >= last24Hours)
            .ToListAsync(cancellationToken);

        var lastSuccessfulSync = await rascorDb.GeofenceSyncLogs
            .Where(s => s.TenantId == tenantId && s.SyncCompleted != null && s.ErrorMessage == null)
            .OrderByDescending(s => s.SyncCompleted)
            .Select(s => s.SyncCompleted)
            .FirstOrDefaultAsync(cancellationToken);

        var totalSyncs = syncsLast24Hours.Count;
        var failedSyncs = syncsLast24Hours.Count(s => !string.IsNullOrEmpty(s.ErrorMessage));
        var totalEventsCreated = syncsLast24Hours.Sum(s => s.RecordsCreated);

        // Consider healthy if last successful sync was within 2 hours (accounting for 15-minute interval + buffer)
        var isHealthy = lastSuccessfulSync.HasValue && lastSuccessfulSync.Value > DateTime.UtcNow.AddHours(-2);

        return new GeofenceSyncStatusDto(
            isHealthy,
            lastSuccessfulSync,
            totalSyncs,
            failedSyncs,
            totalEventsCreated,
            recentSyncs);
    }

    /// <summary>
    /// Gets a list of device IDs from the mobile database that don't have matching employees
    /// </summary>
    public async Task<List<UnmappedDeviceDto>> GetUnmappedDevicesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var rascorDb = scope.ServiceProvider.GetRequiredService<SiteAttendanceDbContext>();

        GeofenceMobileDbContext? mobileDb = null;
        try
        {
            mobileDb = scope.ServiceProvider.GetService<GeofenceMobileDbContext>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Mobile database context not available");
        }

        if (mobileDb == null || !await CanConnectToMobileDbAsync(mobileDb, cancellationToken))
        {
            _logger.LogWarning("Cannot connect to mobile database to get unmapped devices");
            return new List<UnmappedDeviceDto>();
        }

        // Get all device IDs from mobile database
        var mobileDevices = await mobileDb.Devices
            .Where(d => d.IsActive)
            .ToListAsync(cancellationToken);

        // Get all GeoTrackerIDs from employees
        var mappedGeoTrackerIds = await rascorDb.Set<Employee>()
            .Where(e => e.TenantId == tenantId && e.GeoTrackerID != null && !e.IsDeleted)
            .Select(e => e.GeoTrackerID!)
            .ToListAsync(cancellationToken);

        var mappedSet = mappedGeoTrackerIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Find unmapped devices
        var unmappedDevices = mobileDevices
            .Where(d => !mappedSet.Contains(d.Id))
            .ToList();

        // Get event counts for unmapped devices
        var unmappedDeviceIds = unmappedDevices.Select(d => d.Id).ToList();
        var eventCounts = await mobileDb.GeofenceEvents
            .Where(e => unmappedDeviceIds.Contains(e.UserId))
            .GroupBy(e => e.UserId)
            .Select(g => new { DeviceId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var eventCountDict = eventCounts.ToDictionary(x => x.DeviceId, x => x.Count);

        return unmappedDevices
            .Select(d => new UnmappedDeviceDto(
                d.Id,
                d.Platform,
                d.Model,
                d.Manufacturer,
                d.RegisteredAt,
                d.LastSeenAt,
                d.IsActive,
                eventCountDict.GetValueOrDefault(d.Id, 0)))
            .OrderByDescending(d => d.EventCount)
            .ToList();
    }

    /// <summary>
    /// Syncs device status from the mobile database to the local DeviceStatusCache table.
    /// Returns the count of devices synced and count of online devices.
    /// </summary>
    private async Task<(int DevicesSynced, int DevicesOnline)> SyncDeviceStatusAsync(
        GeofenceMobileDbContext mobileDb,
        SiteAttendanceDbContext rascorDb,
        CancellationToken cancellationToken)
    {
        // A device is "online" if LastSeenAt is within the last 90 minutes
        var onlineThreshold = DateTime.UtcNow.AddMinutes(-90);

        try
        {
            // Read all active devices from geofence DB
            var mobileDevices = await mobileDb.Devices
                .Where(d => d.IsActive)
                .ToListAsync(cancellationToken);

            if (!mobileDevices.Any())
            {
                _logger.LogDebug("No active devices found in mobile database");
                return (0, 0);
            }

            var syncedAt = DateTime.UtcNow;
            var deviceIds = mobileDevices.Select(d => d.Id).ToList();

            // Get existing cache entries
            var existingCache = await rascorDb.DeviceStatusCaches
                .Where(d => deviceIds.Contains(d.DeviceId))
                .ToDictionaryAsync(d => d.DeviceId, cancellationToken);

            var onlineCount = 0;

            foreach (var device in mobileDevices)
            {
                if (existingCache.TryGetValue(device.Id, out var existing))
                {
                    // Update existing entry
                    existing.Model = device.Model;
                    existing.Platform = device.Platform;
                    existing.IsActive = device.IsActive;
                    existing.LastSeenAt = device.LastSeenAt.HasValue
                        ? DateTime.SpecifyKind(device.LastSeenAt.Value, DateTimeKind.Utc)
                        : null;
                    existing.LastLatitude = (decimal?)device.LastLatitude;
                    existing.LastLongitude = (decimal?)device.LastLongitude;
                    existing.LastAccuracy = (decimal?)device.LastAccuracy;
                    existing.LastBatteryLevel = device.LastBatteryLevel;
                    existing.SyncedAt = syncedAt;
                }
                else
                {
                    // Insert new entry
                    var cacheEntry = new DeviceStatusCache
                    {
                        DeviceId = device.Id,
                        Model = device.Model,
                        Platform = device.Platform,
                        IsActive = device.IsActive,
                        LastSeenAt = device.LastSeenAt.HasValue
                            ? DateTime.SpecifyKind(device.LastSeenAt.Value, DateTimeKind.Utc)
                            : null,
                        LastLatitude = (decimal?)device.LastLatitude,
                        LastLongitude = (decimal?)device.LastLongitude,
                        LastAccuracy = (decimal?)device.LastAccuracy,
                        LastBatteryLevel = device.LastBatteryLevel,
                        SyncedAt = syncedAt
                    };
                    rascorDb.DeviceStatusCaches.Add(cacheEntry);
                }

                // Count online devices
                if (device.LastSeenAt.HasValue && device.LastSeenAt.Value > onlineThreshold)
                {
                    onlineCount++;
                }
            }

            await rascorDb.SaveChangesAsync(cancellationToken);

            return (mobileDevices.Count, onlineCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync device status from mobile database");
            return (0, 0);
        }
    }
}
