using Microsoft.EntityFrameworkCore;
using Rascor.Core.Domain.Entities;
using Rascor.Modules.SiteAttendance.Application.Services;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Enums;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;
using Rascor.Modules.SiteAttendance.Domain.ValueObjects;
using Rascor.Modules.SiteAttendance.Infrastructure.Persistence;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Services;

public class GeofenceService : IGeofenceService
{
    private readonly SiteAttendanceDbContext _dbContext;
    private readonly IAttendanceSettingsRepository _settingsRepository;
    private readonly IAttendanceEventRepository _eventRepository;

    private const double EarthRadiusMeters = 6371000;

    public GeofenceService(
        SiteAttendanceDbContext dbContext,
        IAttendanceSettingsRepository settingsRepository,
        IAttendanceEventRepository eventRepository)
    {
        _dbContext = dbContext;
        _settingsRepository = settingsRepository;
        _eventRepository = eventRepository;
    }

    public async Task<(Site? Site, double Distance)> FindNearestSiteAsync(
        Guid tenantId,
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken = default)
    {
        // Get all active sites for the tenant that have geolocation configured
        var sites = await _dbContext.Set<Site>()
            .Where(s => s.TenantId == tenantId && s.IsActive && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!sites.Any())
            return (null, double.MaxValue);

        // Filter to sites with coordinates and find the nearest one
        var sitesWithCoordinates = sites
            .Where(s => s.Latitude.HasValue && s.Longitude.HasValue)
            .ToList();

        if (!sitesWithCoordinates.Any())
        {
            // No sites have coordinates configured - return first active site with max distance
            return (sites.First(), double.MaxValue);
        }

        Site? nearestSite = null;
        var nearestDistance = double.MaxValue;

        foreach (var site in sitesWithCoordinates)
        {
            var distance = CalculateDistance(
                site.Latitude!.Value,
                site.Longitude!.Value,
                latitude,
                longitude);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestSite = site;
            }
        }

        return (nearestSite, nearestDistance);
    }

    public async Task<bool> IsWithinGeofenceAsync(
        Guid siteId,
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken = default)
    {
        var site = await _dbContext.Set<Site>()
            .FirstOrDefaultAsync(s => s.Id == siteId, cancellationToken);

        if (site == null)
            return false;

        // If site doesn't have coordinates configured, allow check-in (backwards compatibility)
        if (!site.Latitude.HasValue || !site.Longitude.HasValue)
            return true;

        // Use site-specific radius if set, otherwise fall back to tenant settings
        var settings = await _settingsRepository.GetByTenantAsync(site.TenantId, cancellationToken);
        var geofenceRadius = site.GeofenceRadiusMeters ?? settings?.GeofenceRadiusMeters ?? 100;

        // Calculate distance from site to provided coordinates
        var distance = CalculateDistance(
            site.Latitude.Value,
            site.Longitude.Value,
            latitude,
            longitude);

        return distance <= geofenceRadius;
    }

    /// <summary>
    /// Calculates the distance between two geographic coordinates using the Haversine formula.
    /// </summary>
    /// <param name="lat1">Latitude of point 1 in decimal degrees</param>
    /// <param name="lon1">Longitude of point 1 in decimal degrees</param>
    /// <param name="lat2">Latitude of point 2 in decimal degrees</param>
    /// <param name="lon2">Longitude of point 2 in decimal degrees</param>
    /// <returns>Distance in meters</returns>
    public double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        // Convert decimal degrees to radians
        var lat1Rad = (double)lat1 * Math.PI / 180.0;
        var lat2Rad = (double)lat2 * Math.PI / 180.0;
        var deltaLat = ((double)lat2 - (double)lat1) * Math.PI / 180.0;
        var deltaLon = ((double)lon2 - (double)lon1) * Math.PI / 180.0;

        // Haversine formula
        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;
    }

    public async Task<(bool IsNoise, decimal? Distance)> CheckForNoiseAsync(
        AttendanceEvent newEvent,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        // Only check noise for entry events after the first one
        if (newEvent.EventType != EventType.Enter)
            return (false, null);

        var settings = await _settingsRepository.GetByTenantAsync(tenantId, cancellationToken);
        var noiseThresholdMeters = settings?.NoiseThresholdMeters ?? 150;

        // Get the first entry event of the day for this employee/site
        var eventDate = DateOnly.FromDateTime(newEvent.Timestamp);
        var firstEntry = await GetFirstEntryOfDayAsync(
            tenantId,
            newEvent.EmployeeId,
            newEvent.SiteId,
            eventDate,
            newEvent.Id,
            cancellationToken);

        if (firstEntry == null)
            return (false, null); // This is the first entry

        // If we don't have coordinates, we can't calculate distance
        if (!newEvent.Latitude.HasValue || !newEvent.Longitude.HasValue ||
            !firstEntry.Latitude.HasValue || !firstEntry.Longitude.HasValue)
            return (false, null);

        // Calculate distance from first entry
        var distance = CalculateDistance(
            firstEntry.Latitude.Value,
            firstEntry.Longitude.Value,
            newEvent.Latitude.Value,
            newEvent.Longitude.Value);

        // If within noise threshold, mark as noise
        if (distance <= noiseThresholdMeters)
            return (true, (decimal)distance);

        return (false, (decimal)distance);
    }

    private async Task<AttendanceEvent?> GetFirstEntryOfDayAsync(
        Guid tenantId,
        Guid employeeId,
        Guid siteId,
        DateOnly date,
        Guid excludeEventId,
        CancellationToken cancellationToken)
    {
        var startOfDay = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var endOfDay = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        return await _dbContext.AttendanceEvents
            .Where(e => e.TenantId == tenantId
                && e.EmployeeId == employeeId
                && e.SiteId == siteId
                && e.Timestamp >= startOfDay
                && e.Timestamp <= endOfDay
                && e.EventType == EventType.Enter
                && e.Id != excludeEventId
                && !e.IsNoise)
            .OrderBy(e => e.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
