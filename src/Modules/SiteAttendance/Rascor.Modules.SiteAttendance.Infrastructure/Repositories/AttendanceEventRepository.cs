using Microsoft.EntityFrameworkCore;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;
using Rascor.Modules.SiteAttendance.Infrastructure.Persistence;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Repositories;

public class AttendanceEventRepository : IAttendanceEventRepository
{
    private readonly SiteAttendanceDbContext _context;

    public AttendanceEventRepository(SiteAttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceEvents
            .Include(e => e.Employee)
            .Include(e => e.Site)
            .Include(e => e.DeviceRegistration)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<AttendanceEvent>> GetByEmployeeAsync(
        Guid tenantId,
        Guid employeeId,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AttendanceEvents
            .Include(e => e.Site)
            .Where(e => e.TenantId == tenantId && e.EmployeeId == employeeId);

        if (fromDate.HasValue)
            query = query.Where(e => DateOnly.FromDateTime(e.Timestamp) >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(e => DateOnly.FromDateTime(e.Timestamp) <= toDate.Value);

        return await query
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AttendanceEvent>> GetBySiteAsync(
        Guid tenantId,
        Guid siteId,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AttendanceEvents
            .Include(e => e.Employee)
            .Where(e => e.TenantId == tenantId && e.SiteId == siteId);

        if (fromDate.HasValue)
            query = query.Where(e => DateOnly.FromDateTime(e.Timestamp) >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(e => DateOnly.FromDateTime(e.Timestamp) <= toDate.Value);

        return await query
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AttendanceEvent>> GetByDateRangeAsync(
        Guid tenantId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceEvents
            .Include(e => e.Employee)
            .Include(e => e.Site)
            .Where(e => e.TenantId == tenantId)
            .Where(e => DateOnly.FromDateTime(e.Timestamp) >= fromDate)
            .Where(e => DateOnly.FromDateTime(e.Timestamp) <= toDate)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AttendanceEvent>> GetUnprocessedAsync(
        Guid tenantId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceEvents
            .Include(e => e.Employee)
            .Include(e => e.Site)
            .Where(e => e.TenantId == tenantId)
            .Where(e => !e.Processed)
            .Where(e => DateOnly.FromDateTime(e.Timestamp) == date)
            .OrderBy(e => e.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<AttendanceEvent?> GetLastEventForEmployeeSiteAsync(
        Guid tenantId,
        Guid employeeId,
        Guid siteId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceEvents
            .Where(e => e.TenantId == tenantId)
            .Where(e => e.EmployeeId == employeeId)
            .Where(e => e.SiteId == siteId)
            .Where(e => DateOnly.FromDateTime(e.Timestamp) == date)
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> GetEntryCountForDayAsync(
        Guid tenantId,
        Guid employeeId,
        Guid siteId,
        DateOnly date,
        Guid excludeEventId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceEvents
            .Where(e => e.TenantId == tenantId)
            .Where(e => e.EmployeeId == employeeId)
            .Where(e => e.SiteId == siteId)
            .Where(e => DateOnly.FromDateTime(e.Timestamp) == date)
            .Where(e => e.Id != excludeEventId)
            .Where(e => e.EventType == Domain.Enums.EventType.Enter)
            .CountAsync(cancellationToken);
    }

    public async Task AddAsync(AttendanceEvent entity, CancellationToken cancellationToken = default)
    {
        await _context.AttendanceEvents.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AttendanceEvent entity, CancellationToken cancellationToken = default)
    {
        _context.AttendanceEvents.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<DateOnly>> GetUniqueDatesWithEventsAsync(
        Guid tenantId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceEvents
            .Where(e => e.TenantId == tenantId)
            .Where(e => DateOnly.FromDateTime(e.Timestamp) >= fromDate)
            .Where(e => DateOnly.FromDateTime(e.Timestamp) <= toDate)
            .Select(e => DateOnly.FromDateTime(e.Timestamp))
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync(cancellationToken);
    }

    public async Task<(int Total, int Processed, int Unprocessed)> GetProcessingStatsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var stats = await _context.AttendanceEvents
            .Where(e => e.TenantId == tenantId)
            .GroupBy(e => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Processed = g.Count(e => e.Processed),
                Unprocessed = g.Count(e => !e.Processed)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return stats == null ? (0, 0, 0) : (stats.Total, stats.Processed, stats.Unprocessed);
    }

    public async Task<IEnumerable<(DateOnly Date, int Total, int Processed, int Unprocessed)>> GetEventCountsByDateAsync(
        Guid tenantId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var results = await _context.AttendanceEvents
            .Where(e => e.TenantId == tenantId)
            .Where(e => DateOnly.FromDateTime(e.Timestamp) >= fromDate)
            .Where(e => DateOnly.FromDateTime(e.Timestamp) <= toDate)
            .GroupBy(e => DateOnly.FromDateTime(e.Timestamp))
            .Select(g => new
            {
                Date = g.Key,
                Total = g.Count(),
                Processed = g.Count(e => e.Processed),
                Unprocessed = g.Count(e => !e.Processed)
            })
            .OrderByDescending(x => x.Date)
            .ToListAsync(cancellationToken);

        return results.Select(r => (r.Date, r.Total, r.Processed, r.Unprocessed));
    }
}
