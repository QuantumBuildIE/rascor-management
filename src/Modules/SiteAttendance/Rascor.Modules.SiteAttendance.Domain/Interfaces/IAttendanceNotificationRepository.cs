using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Domain.Interfaces;

public interface IAttendanceNotificationRepository
{
    Task<AttendanceNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceNotification>> GetByEmployeeAsync(Guid tenantId, Guid employeeId, int limit = 50, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceNotification>> GetPendingAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(AttendanceNotification entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(AttendanceNotification entity, CancellationToken cancellationToken = default);
}
