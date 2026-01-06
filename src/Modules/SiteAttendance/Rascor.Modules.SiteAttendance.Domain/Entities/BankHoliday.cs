using Rascor.Core.Domain.Common;

namespace Rascor.Modules.SiteAttendance.Domain.Entities;

/// <summary>
/// Bank holiday dates excluded from working day calculations
/// </summary>
public class BankHoliday : TenantEntity
{
    public DateOnly Date { get; private set; }
    public string? Name { get; private set; }

    private BankHoliday() { } // EF Core

    public static BankHoliday Create(Guid tenantId, DateOnly date, string? name = null)
    {
        return new BankHoliday
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Date = date,
            Name = name,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(DateOnly date, string? name)
    {
        Date = date;
        Name = name;
    }
}
