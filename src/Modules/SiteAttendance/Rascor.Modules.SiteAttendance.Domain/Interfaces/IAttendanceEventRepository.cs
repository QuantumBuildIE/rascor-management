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

    /// <summary>
    /// Gets all unique dates that have events within the specified range
    /// </summary>
    Task<IEnumerable<DateOnly>> GetUniqueDatesWithEventsAsync(Guid tenantId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets processing statistics for events (total, processed, unprocessed counts)
    /// </summary>
    Task<(int Total, int Processed, int Unprocessed)> GetProcessingStatsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets event counts grouped by date
    /// </summary>
    Task<IEnumerable<(DateOnly Date, int Total, int Processed, int Unprocessed)>> GetEventCountsByDateAsync(Guid tenantId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
}
