using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Services;

/// <summary>
/// Service interface for fetching site attendance report data.
/// Combines Float scheduling data with geofence events and SPA completion.
/// </summary>
public interface ISiteAttendanceReportDataService
{
    /// <summary>
    /// Generates the site attendance report for a specific date.
    /// Cross-references Float scheduling data with geofence events and SPA completion.
    /// </summary>
    /// <param name="tenantId">The tenant ID for multi-tenancy filtering.</param>
    /// <param name="date">The date to generate the report for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The site attendance report with entries categorized as Planned, Arrived, or Unplanned.</returns>
    Task<SiteAttendanceReportDto> GetReportAsync(
        Guid tenantId,
        DateOnly date,
        CancellationToken cancellationToken = default);
}
