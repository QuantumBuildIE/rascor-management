using Microsoft.EntityFrameworkCore;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;
using Rascor.Modules.SiteAttendance.Infrastructure.Persistence;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Repositories;

public class AttendanceSummaryRepository : IAttendanceSummaryRepository
{
    private readonly SiteAttendanceDbContext _context;

    public AttendanceSummaryRepository(SiteAttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceSummary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceSummaries
            .Include(s => s.Employee)
            .Include(s => s.Site)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<AttendanceSummary?> GetByEmployeeSiteDateAsync(
        Guid tenantId,
        Guid employeeId,
        Guid siteId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceSummaries
            .Include(s => s.Employee)
            .Include(s => s.Site)
            .FirstOrDefaultAsync(s =>
                s.TenantId == tenantId &&
                s.EmployeeId == employeeId &&
                s.SiteId == siteId &&
                s.Date == date,
                cancellationToken);
    }

    public async Task<IEnumerable<AttendanceSummary>> GetByEmployeeAsync(
        Guid tenantId,
        Guid employeeId,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AttendanceSummaries
            .Include(s => s.Site)
            .Where(s => s.TenantId == tenantId && s.EmployeeId == employeeId);

        if (fromDate.HasValue)
            query = query.Where(s => s.Date >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.Date <= toDate.Value);

        return await query
            .OrderByDescending(s => s.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AttendanceSummary>> GetBySiteAsync(
        Guid tenantId,
        Guid siteId,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AttendanceSummaries
            .Include(s => s.Employee)
            .Where(s => s.TenantId == tenantId && s.SiteId == siteId);

        if (fromDate.HasValue)
            query = query.Where(s => s.Date >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.Date <= toDate.Value);

        return await query
            .OrderByDescending(s => s.Date)
            .ThenBy(s => s.Employee.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AttendanceSummary>> GetByDateRangeAsync(
        Guid tenantId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceSummaries
            .Include(s => s.Employee)
            .Include(s => s.Site)
            .Where(s => s.TenantId == tenantId)
            .Where(s => s.Date >= fromDate)
            .Where(s => s.Date <= toDate)
            .OrderByDescending(s => s.Date)
            .ThenBy(s => s.Employee.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AttendanceSummary entity, CancellationToken cancellationToken = default)
    {
        await _context.AttendanceSummaries.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AttendanceSummary entity, CancellationToken cancellationToken = default)
    {
        _context.AttendanceSummaries.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
