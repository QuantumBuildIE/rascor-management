using Microsoft.EntityFrameworkCore;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;
using Rascor.Modules.SiteAttendance.Infrastructure.Persistence;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Repositories;

public class DeviceRegistrationRepository : IDeviceRegistrationRepository
{
    private readonly SiteAttendanceDbContext _context;

    public DeviceRegistrationRepository(SiteAttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<DeviceRegistration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DeviceRegistrations
            .Include(d => d.Employee)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<DeviceRegistration?> GetByDeviceIdentifierAsync(
        Guid tenantId,
        string deviceIdentifier,
        CancellationToken cancellationToken = default)
    {
        return await _context.DeviceRegistrations
            .Include(d => d.Employee)
            .FirstOrDefaultAsync(d =>
                d.TenantId == tenantId &&
                d.DeviceIdentifier == deviceIdentifier,
                cancellationToken);
    }

    public async Task<IEnumerable<DeviceRegistration>> GetByEmployeeAsync(
        Guid tenantId,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return await _context.DeviceRegistrations
            .Where(d => d.TenantId == tenantId && d.EmployeeId == employeeId)
            .OrderByDescending(d => d.LastActiveAt ?? d.RegisteredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DeviceRegistration>> GetActiveDevicesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.DeviceRegistrations
            .Include(d => d.Employee)
            .Where(d => d.TenantId == tenantId && d.IsActive)
            .OrderByDescending(d => d.LastActiveAt ?? d.RegisteredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DeviceRegistration entity, CancellationToken cancellationToken = default)
    {
        await _context.DeviceRegistrations.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DeviceRegistration entity, CancellationToken cancellationToken = default)
    {
        _context.DeviceRegistrations.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
