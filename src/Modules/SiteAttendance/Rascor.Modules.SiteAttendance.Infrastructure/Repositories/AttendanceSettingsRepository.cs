using Microsoft.EntityFrameworkCore;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;
using Rascor.Modules.SiteAttendance.Infrastructure.Persistence;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Repositories;

public class AttendanceSettingsRepository : IAttendanceSettingsRepository
{
    private readonly SiteAttendanceDbContext _context;

    public AttendanceSettingsRepository(SiteAttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceSettings?> GetByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);
    }

    public async Task AddAsync(AttendanceSettings entity, CancellationToken cancellationToken = default)
    {
        await _context.AttendanceSettings.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AttendanceSettings entity, CancellationToken cancellationToken = default)
    {
        _context.AttendanceSettings.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
