using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Domain.Interfaces;

public interface IAttendanceSettingsRepository
{
    Task<AttendanceSettings?> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(AttendanceSettings entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(AttendanceSettings entity, CancellationToken cancellationToken = default);
}
