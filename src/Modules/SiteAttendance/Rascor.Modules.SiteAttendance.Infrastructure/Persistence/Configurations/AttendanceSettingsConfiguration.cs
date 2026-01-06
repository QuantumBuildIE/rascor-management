using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Persistence.Configurations;

public class AttendanceSettingsConfiguration : IEntityTypeConfiguration<AttendanceSettings>
{
    public void Configure(EntityTypeBuilder<AttendanceSettings> builder)
    {
        builder.ToTable("attendance_settings", "site_attendance");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ExpectedHoursPerDay)
            .HasPrecision(4, 2)
            .IsRequired();

        builder.Property(e => e.WorkStartTime)
            .IsRequired();

        builder.Property(e => e.NotificationTitle)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.NotificationMessage)
            .HasMaxLength(500)
            .IsRequired();

        // Unique constraint: one settings record per tenant
        builder.HasIndex(e => e.TenantId)
            .IsUnique();

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
