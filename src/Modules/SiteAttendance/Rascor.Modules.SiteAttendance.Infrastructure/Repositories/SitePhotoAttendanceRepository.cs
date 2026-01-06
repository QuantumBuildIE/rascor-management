using Microsoft.EntityFrameworkCore;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;
using Rascor.Modules.SiteAttendance.Infrastructure.Persistence;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Repositories;

public class SitePhotoAttendanceRepository : ISitePhotoAttendanceRepository
{
    private readonly SiteAttendanceDbContext _context;

    public SitePhotoAttendanceRepository(SiteAttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<SitePhotoAttendance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SitePhotoAttendances
            .Include(s => s.Employee)
            .Include(s => s.Site)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<SitePhotoAttendance?> GetByEmployeeSiteDateAsync(
        Guid tenantId,
        Guid employeeId,
        Guid siteId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await _context.SitePhotoAttendances
            .Include(s => s.Employee)
            .Include(s => s.Site)
            .FirstOrDefaultAsync(s =>
                s.TenantId == tenantId &&
                s.EmployeeId == employeeId &&
                s.SiteId == siteId &&
                s.EventDate == date,
                cancellationToken);
    }

    public async Task<IEnumerable<SitePhotoAttendance>> GetByEmployeeAsync(
        Guid tenantId,
        Guid employeeId,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SitePhotoAttendances
            .Include(s => s.Site)
            .Where(s => s.TenantId == tenantId && s.EmployeeId == employeeId);

        if (fromDate.HasValue)
            query = query.Where(s => s.EventDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.EventDate <= toDate.Value);

        return await query
            .OrderByDescending(s => s.EventDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SitePhotoAttendance>> GetByDateRangeAsync(
        Guid tenantId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.SitePhotoAttendances
            .Include(s => s.Employee)
            .Include(s => s.Site)
            .Where(s => s.TenantId == tenantId)
            .Where(s => s.EventDate >= fromDate)
            .Where(s => s.EventDate <= toDate)
            .OrderByDescending(s => s.EventDate)
            .ThenBy(s => s.Employee.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsForEmployeeSiteDateAsync(
        Guid tenantId,
        Guid employeeId,
        Guid siteId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await _context.SitePhotoAttendances
            .AnyAsync(s =>
                s.TenantId == tenantId &&
                s.EmployeeId == employeeId &&
                s.SiteId == siteId &&
                s.EventDate == date,
                cancellationToken);
    }

    public async Task AddAsync(SitePhotoAttendance entity, CancellationToken cancellationToken = default)
    {
        await _context.SitePhotoAttendances.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SitePhotoAttendance entity, CancellationToken cancellationToken = default)
    {
        _context.SitePhotoAttendances.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
