using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Domain.Interfaces;

public interface IBankHolidayRepository
{
    Task<BankHoliday?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<BankHoliday>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BankHoliday>> GetByYearAsync(Guid tenantId, int year, CancellationToken cancellationToken = default);
    Task<bool> IsBankHolidayAsync(Guid tenantId, DateOnly date, CancellationToken cancellationToken = default);
    Task<IEnumerable<DateOnly>> GetBankHolidayDatesAsync(Guid tenantId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
    Task AddAsync(BankHoliday entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(BankHoliday entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
