using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Domain.Interfaces;

public interface IDeviceRegistrationRepository
{
    Task<DeviceRegistration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DeviceRegistration?> GetByDeviceIdentifierAsync(Guid tenantId, string deviceIdentifier, CancellationToken cancellationToken = default);
    Task<IEnumerable<DeviceRegistration>> GetByEmployeeAsync(Guid tenantId, Guid employeeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DeviceRegistration>> GetActiveDevicesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(DeviceRegistration entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(DeviceRegistration entity, CancellationToken cancellationToken = default);
}
