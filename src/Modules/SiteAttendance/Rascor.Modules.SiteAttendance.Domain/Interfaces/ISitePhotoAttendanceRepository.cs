using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Domain.Interfaces;

public interface ISitePhotoAttendanceRepository
{
    Task<SitePhotoAttendance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SitePhotoAttendance?> GetByEmployeeSiteDateAsync(Guid tenantId, Guid employeeId, Guid siteId, DateOnly date, CancellationToken cancellationToken = default);
    Task<IEnumerable<SitePhotoAttendance>> GetByEmployeeAsync(Guid tenantId, Guid employeeId, DateOnly? fromDate = null, DateOnly? toDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<SitePhotoAttendance>> GetByDateRangeAsync(Guid tenantId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
    Task<bool> ExistsForEmployeeSiteDateAsync(Guid tenantId, Guid employeeId, Guid siteId, DateOnly date, CancellationToken cancellationToken = default);
    Task AddAsync(SitePhotoAttendance entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(SitePhotoAttendance entity, CancellationToken cancellationToken = default);
}
