using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Services;

public interface IAttendanceAnalyticsService
{
    Task<DashboardKpisDto> GetDashboardKpisAsync(Guid tenantId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);

    Task<IEnumerable<EmployeePerformanceDto>> GetEmployeePerformanceAsync(Guid tenantId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);

    Task<IEnumerable<SiteActivityDto>> GetSiteActivityAsync(Guid tenantId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
}
