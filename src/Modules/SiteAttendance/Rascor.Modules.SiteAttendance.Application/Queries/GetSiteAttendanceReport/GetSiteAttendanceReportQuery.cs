using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetSiteAttendanceReport;

/// <summary>
/// Query to get the site attendance report for a specific date.
/// Cross-references Float scheduling data with geofence events and SPA completion.
/// </summary>
public record GetSiteAttendanceReportQuery : IRequest<SiteAttendanceReportDto>
{
    /// <summary>
    /// The tenant ID for multi-tenancy filtering.
    /// </summary>
    public Guid TenantId { get; init; }

    /// <summary>
    /// The date to generate the report for.
    /// </summary>
    public DateOnly Date { get; init; }
}
