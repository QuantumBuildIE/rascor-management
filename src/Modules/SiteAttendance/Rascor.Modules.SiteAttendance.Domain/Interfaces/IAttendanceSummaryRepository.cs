using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Domain.Interfaces;

public interface IAttendanceSummaryRepository
{
    Task<AttendanceSummary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AttendanceSummary?> GetByEmployeeSiteDateAsync(Guid tenantId, Guid employeeId, Guid siteId, DateOnly date, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceSummary>> GetByEmployeeAsync(Guid tenantId, Guid employeeId, DateOnly? fromDate = null, DateOnly? toDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceSummary>> GetBySiteAsync(Guid tenantId, Guid siteId, DateOnly? fromDate = null, DateOnly? toDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceSummary>> GetByDateRangeAsync(Guid tenantId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
    Task AddAsync(AttendanceSummary entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(AttendanceSummary entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets total count of summaries for a tenant
    /// </summary>
    Task<int> GetTotalCountAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets summary counts grouped by date
    /// </summary>
    Task<IEnumerable<(DateOnly Date, int Count)>> GetCountsByDateAsync(Guid tenantId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
}
