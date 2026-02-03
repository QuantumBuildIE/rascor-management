using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Persistence.Configurations;

public class DeviceStatusCacheConfiguration : IEntityTypeConfiguration<DeviceStatusCache>
{
    public void Configure(EntityTypeBuilder<DeviceStatusCache> builder)
    {
        builder.ToTable("device_status_cache", "site_attendance");

        builder.HasKey(e => e.DeviceId);

        builder.Property(e => e.DeviceId)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Model)
            .HasMaxLength(100);

        builder.Property(e => e.Platform)
            .HasMaxLength(20);

        builder.Property(e => e.LastLatitude)
            .HasPrecision(10, 8);

        builder.Property(e => e.LastLongitude)
            .HasPrecision(11, 8);

        builder.Property(e => e.LastAccuracy)
            .HasPrecision(10, 2);

        builder.Property(e => e.SyncedAt)
            .IsRequired();

        // Index for finding online devices
        builder.HasIndex(e => e.LastSeenAt);

        // Index for finding active devices
        builder.HasIndex(e => e.IsActive);
    }
}
