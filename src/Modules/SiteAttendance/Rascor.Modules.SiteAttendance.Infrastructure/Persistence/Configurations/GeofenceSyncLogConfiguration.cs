using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Persistence.Configurations;

public class GeofenceSyncLogConfiguration : IEntityTypeConfiguration<GeofenceSyncLog>
{
    public void Configure(EntityTypeBuilder<GeofenceSyncLog> builder)
    {
        builder.ToTable("geofence_sync_logs", "site_attendance");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SyncStarted)
            .IsRequired();

        builder.Property(e => e.LastEventId)
            .HasMaxLength(100);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        // Index for querying latest sync per tenant
        builder.HasIndex(e => new { e.TenantId, e.SyncStarted })
            .IsDescending(false, true);

        // Index for finding successful syncs
        builder.HasIndex(e => new { e.TenantId, e.SyncCompleted })
            .HasFilter("\"SyncCompleted\" IS NOT NULL AND \"ErrorMessage\" IS NULL");

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
