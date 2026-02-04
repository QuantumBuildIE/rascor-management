using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Services;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetSiteAttendanceReport;

/// <summary>
/// Handler for GetSiteAttendanceReportQuery.
/// Delegates to ISiteAttendanceReportDataService for cross-referencing Float scheduling data
/// with geofence events and SPA completion.
/// </summary>
public class GetSiteAttendanceReportQueryHandler : IRequestHandler<GetSiteAttendanceReportQuery, SiteAttendanceReportDto>
{
    private readonly ISiteAttendanceReportDataService _reportDataService;

    public GetSiteAttendanceReportQueryHandler(ISiteAttendanceReportDataService reportDataService)
    {
        _reportDataService = reportDataService;
    }

    public async Task<SiteAttendanceReportDto> Handle(GetSiteAttendanceReportQuery request, CancellationToken cancellationToken)
    {
        return await _reportDataService.GetReportAsync(
            request.TenantId,
            request.Date,
            cancellationToken);
    }
}
