using Microsoft.EntityFrameworkCore;
using Rascor.Core.Domain.Entities;
using Rascor.Core.Infrastructure.Float;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Services;
using Rascor.Modules.SiteAttendance.Domain.Enums;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;
using Rascor.Modules.SiteAttendance.Infrastructure.Persistence;

namespace Rascor.API.Services;

/// <summary>
/// Service implementation for fetching site attendance report data.
/// Cross-references Float scheduling data with geofence events and SPA completion.
///
/// This service is implemented in the API layer because it requires access to both
/// IFloatApiClient (Core.Infrastructure) and SiteAttendanceDbContext (SiteAttendance.Infrastructure),
/// and there's a circular dependency constraint between these modules.
/// </summary>
public class SiteAttendanceReportDataService : ISiteAttendanceReportDataService
{
    private readonly SiteAttendanceDbContext _dbContext;
    private readonly IFloatApiClient _floatApiClient;
    private readonly IAttendanceEventRepository _eventRepository;
    private readonly ILogger<SiteAttendanceReportDataService> _logger;

    public SiteAttendanceReportDataService(
        SiteAttendanceDbContext dbContext,
        IFloatApiClient floatApiClient,
        IAttendanceEventRepository eventRepository,
        ILogger<SiteAttendanceReportDataService> logger)
    {
        _dbContext = dbContext;
        _floatApiClient = floatApiClient;
        _eventRepository = eventRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SiteAttendanceReportDto> GetReportAsync(
        Guid tenantId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating site attendance report for tenant {TenantId} on {Date}",
            tenantId, date);

        // Step 1: Get all employees with Float links
        var employees = await _dbContext.Set<Employee>()
            .AsNoTracking()
            .Where(e => e.TenantId == tenantId && !e.IsDeleted && e.FloatPersonId != null)
            .ToListAsync(cancellationToken);

        var employeesByFloatId = employees
            .Where(e => e.FloatPersonId.HasValue)
            .ToDictionary(e => e.FloatPersonId!.Value);

        var employeesById = employees.ToDictionary(e => e.Id);

        _logger.LogDebug("Found {Count} employees with Float links", employees.Count);

        // Step 2: Get all sites with Float links
        var sites = await _dbContext.Set<Site>()
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId && !s.IsDeleted && s.FloatProjectId != null)
            .ToListAsync(cancellationToken);

        var sitesByFloatProjectId = sites
            .Where(s => s.FloatProjectId.HasValue)
            .ToDictionary(s => s.FloatProjectId!.Value);

        var sitesById = sites.ToDictionary(s => s.Id);

        _logger.LogDebug("Found {Count} sites with Float links", sites.Count);

        // Step 3: Get Float tasks for the date
        var floatTasks = await _floatApiClient.GetTasksForDateAsync(date, cancellationToken);
        _logger.LogDebug("Retrieved {Count} Float tasks for {Date}", floatTasks.Count, date);

        // Step 4: Get ALL geofence events for the date (Enter only, non-noise)
        var events = await _eventRepository.GetByDateRangeAsync(
            tenantId,
            date,
            date,
            cancellationToken);

        var enterEvents = events
            .Where(e => e.EventType == EventType.Enter && !e.IsNoise)
            .ToList();

        _logger.LogDebug("Found {Count} Enter events (non-noise) for {Date}", enterEvents.Count, date);

        // Step 5: Build the "Planned" set
        // Key: (EmployeeId, SiteId) -> PlannedArrival (from task.StartDate)
        var plannedSet = new Dictionary<(Guid EmployeeId, Guid SiteId), DateTime>();

        foreach (var task in floatTasks)
        {
            // Skip tasks without a project ID or that don't map to a site
            if (!task.ProjectId.HasValue || !sitesByFloatProjectId.TryGetValue(task.ProjectId.Value, out var site))
            {
                continue;
            }

            // Collect person IDs from BOTH PeopleId (single) and PeopleIds (multiple)
            var personIds = new HashSet<int>();

            if (task.PeopleId.HasValue)
            {
                personIds.Add(task.PeopleId.Value);
            }

            if (task.PeopleIds != null)
            {
                foreach (var pid in task.PeopleIds)
                {
                    personIds.Add(pid);
                }
            }

            // For each person, map to Employee
            foreach (var personId in personIds)
            {
                if (!employeesByFloatId.TryGetValue(personId, out var employee))
                {
                    continue;
                }

                var key = (employee.Id, site.Id);

                // Parse task.StartDate to DateTime (treat as start of day UTC)
                DateTime? plannedArrival = null;
                if (task.StartDateParsed.HasValue)
                {
                    plannedArrival = task.StartDateParsed.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                }

                // Only add if not already present (first task wins)
                if (!plannedSet.ContainsKey(key) && plannedArrival.HasValue)
                {
                    plannedSet[key] = plannedArrival.Value;
                }
            }
        }

        _logger.LogDebug("Built planned set with {Count} entries", plannedSet.Count);

        // Step 6: Build the "Actual" set
        // Group geofence Enter events by (EmployeeId, SiteId), take earliest timestamp
        var actualSet = enterEvents
            .GroupBy(e => (e.EmployeeId, e.SiteId))
            .ToDictionary(
                g => g.Key,
                g => g.Min(e => e.Timestamp)
            );

        _logger.LogDebug("Built actual set with {Count} entries", actualSet.Count);

        // Step 7: Full outer join and assign statuses
        var allKeys = plannedSet.Keys
            .Union(actualSet.Keys)
            .ToHashSet();

        var entries = new List<SiteAttendanceReportEntryDto>();

        // Collect employee/site IDs that might need loading (for unplanned arrivals)
        var missingEmployeeIds = new HashSet<Guid>();
        var missingSiteIds = new HashSet<Guid>();

        foreach (var key in allKeys)
        {
            var (employeeId, siteId) = key;

            if (!employeesById.ContainsKey(employeeId))
            {
                missingEmployeeIds.Add(employeeId);
            }

            if (!sitesById.ContainsKey(siteId))
            {
                missingSiteIds.Add(siteId);
            }
        }

        // Batch load missing employees and sites
        if (missingEmployeeIds.Count > 0)
        {
            var additionalEmployees = await _dbContext.Set<Employee>()
                .AsNoTracking()
                .Where(e => missingEmployeeIds.Contains(e.Id) && e.TenantId == tenantId)
                .ToListAsync(cancellationToken);

            foreach (var emp in additionalEmployees)
            {
                employeesById[emp.Id] = emp;
            }
        }

        if (missingSiteIds.Count > 0)
        {
            var additionalSites = await _dbContext.Set<Site>()
                .AsNoTracking()
                .Where(s => missingSiteIds.Contains(s.Id) && s.TenantId == tenantId)
                .ToListAsync(cancellationToken);

            foreach (var s in additionalSites)
            {
                sitesById[s.Id] = s;
            }
        }

        foreach (var key in allKeys)
        {
            var (employeeId, siteId) = key;

            var inPlanned = plannedSet.TryGetValue(key, out var plannedArrival);
            var inActual = actualSet.TryGetValue(key, out var actualArrival);

            AttendanceReportStatus status;
            if (inPlanned && inActual)
            {
                status = AttendanceReportStatus.Arrived;
            }
            else if (inPlanned && !inActual)
            {
                status = AttendanceReportStatus.Planned;
            }
            else // !inPlanned && inActual
            {
                status = AttendanceReportStatus.Unplanned;
            }

            // Get employee and site details
            string employeeName = string.Empty;
            string siteName = string.Empty;
            string? siteCode = null;

            if (employeesById.TryGetValue(employeeId, out var emp))
            {
                employeeName = emp.FullName;
            }

            if (sitesById.TryGetValue(siteId, out var s))
            {
                siteName = s.SiteName;
                siteCode = s.SiteCode;
            }

            entries.Add(new SiteAttendanceReportEntryDto
            {
                Status = status,
                EmployeeId = employeeId,
                EmployeeName = employeeName,
                SiteId = siteId,
                SiteName = siteName,
                SiteCode = siteCode,
                PlannedArrival = inPlanned ? plannedArrival : null,
                ActualArrival = inActual ? actualArrival : null,
                SpaCompleted = false, // Will be set in step 8
                SpaId = null,
                SpaImageUrl = null
            });
        }

        // Step 8: SPA lookup (batch optimized)
        var spasForDate = await _dbContext.SitePhotoAttendances
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.EventDate == date)
            .ToListAsync(cancellationToken);

        var spaLookup = spasForDate.ToLookup(s => (s.EmployeeId, s.SiteId));

        foreach (var entry in entries)
        {
            var spas = spaLookup[(entry.EmployeeId, entry.SiteId)].ToList();
            if (spas.Count > 0)
            {
                var spa = spas.First(); // Take first if multiple
                entry.SpaCompleted = true;
                entry.SpaId = spa.Id;
                entry.SpaImageUrl = spa.ImageUrl;
            }
        }

        _logger.LogDebug("Completed SPA lookup, found {Count} SPA records", spasForDate.Count);

        // Step 9: Build response
        // Sort entries: Arrived first, then Planned, then Unplanned
        // Within each group, sort by SiteName then EmployeeName
        var sortedEntries = entries
            .OrderBy(e => e.Status switch
            {
                AttendanceReportStatus.Arrived => 0,
                AttendanceReportStatus.Planned => 1,
                AttendanceReportStatus.Unplanned => 2,
                _ => 3
            })
            .ThenBy(e => e.SiteName)
            .ThenBy(e => e.EmployeeName)
            .ToList();

        var result = new SiteAttendanceReportDto
        {
            Date = date,
            Entries = sortedEntries,
            TotalPlanned = sortedEntries.Count(e => e.Status == AttendanceReportStatus.Planned),
            TotalArrived = sortedEntries.Count(e => e.Status == AttendanceReportStatus.Arrived),
            TotalUnplanned = sortedEntries.Count(e => e.Status == AttendanceReportStatus.Unplanned)
        };

        _logger.LogInformation(
            "Generated site attendance report for {Date}: {TotalArrived} arrived, {TotalPlanned} planned (not arrived), {TotalUnplanned} unplanned",
            date, result.TotalArrived, result.TotalPlanned, result.TotalUnplanned);

        return result;
    }
}
