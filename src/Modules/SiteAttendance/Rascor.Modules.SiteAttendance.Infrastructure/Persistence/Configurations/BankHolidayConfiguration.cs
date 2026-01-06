using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Persistence.Configurations;

public class BankHolidayConfiguration : IEntityTypeConfiguration<BankHoliday>
{
    public void Configure(EntityTypeBuilder<BankHoliday> builder)
    {
        builder.ToTable("bank_holidays", "site_attendance");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Date)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(100);

        // Unique constraint: one entry per date per tenant
        builder.HasIndex(e => new { e.TenantId, e.Date })
            .IsUnique();

        builder.HasIndex(e => e.Date);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
