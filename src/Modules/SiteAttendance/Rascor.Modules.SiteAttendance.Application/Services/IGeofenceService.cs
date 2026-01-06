using Rascor.Core.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Application.Services;

public interface IGeofenceService
{
    /// <summary>
    /// Finds the nearest site to given coordinates
    /// </summary>
    Task<(Site? Site, double Distance)> FindNearestSiteAsync(Guid tenantId, decimal latitude, decimal longitude, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if coordinates are within a site's geofence radius
    /// </summary>
    Task<bool> IsWithinGeofenceAsync(Guid siteId, decimal latitude, decimal longitude, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates distance between two coordinates in meters (Haversine formula)
    /// </summary>
    double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2);

    /// <summary>
    /// Checks if an event should be marked as noise (too close to first entry)
    /// </summary>
    Task<(bool IsNoise, decimal? Distance)> CheckForNoiseAsync(AttendanceEvent newEvent, Guid tenantId, CancellationToken cancellationToken = default);
}
