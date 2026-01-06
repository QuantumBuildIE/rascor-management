using Microsoft.EntityFrameworkCore;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;
using Rascor.Modules.SiteAttendance.Infrastructure.Persistence;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Repositories;

public class BankHolidayRepository : IBankHolidayRepository
{
    private readonly SiteAttendanceDbContext _context;

    public BankHolidayRepository(SiteAttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<BankHoliday?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BankHolidays
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<BankHoliday>> GetAllAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.BankHolidays
            .Where(b => b.TenantId == tenantId)
            .OrderBy(b => b.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BankHoliday>> GetByYearAsync(
        Guid tenantId,
        int year,
        CancellationToken cancellationToken = default)
    {
        return await _context.BankHolidays
            .Where(b => b.TenantId == tenantId && b.Date.Year == year)
            .OrderBy(b => b.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsBankHolidayAsync(
        Guid tenantId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await _context.BankHolidays
            .AnyAsync(b => b.TenantId == tenantId && b.Date == date, cancellationToken);
    }

    public async Task<IEnumerable<DateOnly>> GetBankHolidayDatesAsync(
        Guid tenantId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.BankHolidays
            .Where(b => b.TenantId == tenantId)
            .Where(b => b.Date >= fromDate && b.Date <= toDate)
            .Select(b => b.Date)
            .OrderBy(d => d)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(BankHoliday entity, CancellationToken cancellationToken = default)
    {
        await _context.BankHolidays.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(BankHoliday entity, CancellationToken cancellationToken = default)
    {
        _context.BankHolidays.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.BankHolidays.FindAsync([id], cancellationToken);
        if (entity != null)
        {
            _context.BankHolidays.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
