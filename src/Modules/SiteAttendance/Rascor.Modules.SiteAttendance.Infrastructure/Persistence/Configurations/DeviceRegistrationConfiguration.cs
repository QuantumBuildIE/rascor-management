using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Persistence.Configurations;

public class DeviceRegistrationConfiguration : IEntityTypeConfiguration<DeviceRegistration>
{
    public void Configure(EntityTypeBuilder<DeviceRegistration> builder)
    {
        builder.ToTable("device_registrations", "site_attendance");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.DeviceIdentifier)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.DeviceName)
            .HasMaxLength(200);

        builder.Property(e => e.Platform)
            .HasMaxLength(20);

        builder.Property(e => e.PushToken)
            .HasMaxLength(500);

        builder.HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        // Unique constraint: one device identifier per tenant
        builder.HasIndex(e => new { e.TenantId, e.DeviceIdentifier })
            .IsUnique();

        builder.HasIndex(e => new { e.TenantId, e.EmployeeId });
        builder.HasIndex(e => e.IsActive);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
