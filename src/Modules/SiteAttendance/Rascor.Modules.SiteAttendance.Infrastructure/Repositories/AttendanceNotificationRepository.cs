using Microsoft.EntityFrameworkCore;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;
using Rascor.Modules.SiteAttendance.Infrastructure.Persistence;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Repositories;

public class AttendanceNotificationRepository : IAttendanceNotificationRepository
{
    private readonly SiteAttendanceDbContext _context;

    public AttendanceNotificationRepository(SiteAttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceNotifications
            .Include(n => n.Employee)
            .Include(n => n.RelatedEvent)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<AttendanceNotification>> GetByEmployeeAsync(
        Guid tenantId,
        Guid employeeId,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceNotifications
            .Include(n => n.RelatedEvent)
            .Where(n => n.TenantId == tenantId && n.EmployeeId == employeeId)
            .OrderByDescending(n => n.SentAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AttendanceNotification>> GetPendingAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceNotifications
            .Include(n => n.Employee)
            .Where(n => n.TenantId == tenantId && !n.Delivered)
            .OrderBy(n => n.SentAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AttendanceNotification entity, CancellationToken cancellationToken = default)
    {
        await _context.AttendanceNotifications.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AttendanceNotification entity, CancellationToken cancellationToken = default)
    {
        _context.AttendanceNotifications.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
