using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Domain.Interfaces;

public interface IAttendanceEventRepository
{
    Task<AttendanceEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceEvent>> GetByEmployeeAsync(Guid tenantId, Guid employeeId, DateOnly? fromDate = null, DateOnly? toDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceEvent>> GetBySiteAsync(Guid tenantId, Guid siteId, DateOnly? fromDate = null, DateOnly? toDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceEvent>> GetByDateRangeAsync(Guid tenantId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceEvent>> GetUnprocessedAsync(Guid tenantId, DateOnly date, CancellationToken cancellationToken = default);
    Task<AttendanceEvent?> GetLastEventForEmployeeSiteAsync(Guid tenantId, Guid employeeId, Guid siteId, DateOnly date, CancellationToken cancellationToken = default);
    Task<int> GetEntryCountForDayAsync(Guid tenantId, Guid employeeId, Guid siteId, DateOnly date, Guid excludeEventId, CancellationToken cancellationToken = default);
    Task AddAsync(AttendanceEvent entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(AttendanceEvent entity, CancellationToken cancellationToken = default);
}
