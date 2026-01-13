using Microsoft.EntityFrameworkCore;
using Rascor.Modules.SiteAttendance.Infrastructure.Sync.Models;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Sync;

/// <summary>
/// Read-only DbContext for accessing the mobile geofence database.
/// This context is used to sync data from the mobile app to Rascor.
/// </summary>
public class GeofenceMobileDbContext : DbContext
{
    public GeofenceMobileDbContext(DbContextOptions<GeofenceMobileDbContext> options)
        : base(options)
    {
    }

    public DbSet<MobileGeofenceEvent> GeofenceEvents => Set<MobileGeofenceEvent>();
    public DbSet<MobileDevice> Devices => Set<MobileDevice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure MobileGeofenceEvent mapping to geofence_events table
        modelBuilder.Entity<MobileGeofenceEvent>(entity =>
        {
            entity.ToTable("geofence_events");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.SiteId)
                .HasColumnName("site_id")
                .IsRequired();

            entity.Property(e => e.EventType)
                .HasColumnName("event_type")
                .IsRequired();

            entity.Property(e => e.Timestamp)
                .HasColumnName("timestamp")
                .IsRequired();

            entity.Property(e => e.Latitude)
                .HasColumnName("latitude")
                .HasPrecision(10, 8);

            entity.Property(e => e.Longitude)
                .HasColumnName("longitude")
                .HasPrecision(11, 8);

            entity.Property(e => e.TriggerMethod)
                .HasColumnName("trigger_method");
        });

        // Configure MobileDevice mapping to devices table
        modelBuilder.Entity<MobileDevice>(entity =>
        {
            entity.ToTable("devices");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.PlatformIdentifier)
                .HasColumnName("platform_identifier");

            entity.Property(e => e.Platform)
                .HasColumnName("platform");

            entity.Property(e => e.Model)
                .HasColumnName("model");

            entity.Property(e => e.Manufacturer)
                .HasColumnName("manufacturer");

            entity.Property(e => e.OsVersion)
                .HasColumnName("os_version");

            entity.Property(e => e.DeviceType)
                .HasColumnName("device_type");

            entity.Property(e => e.RegisteredAt)
                .HasColumnName("registered_at");

            entity.Property(e => e.LastSeenAt)
                .HasColumnName("last_seen_at");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active");
        });
    }

    /// <summary>
    /// Override SaveChanges to prevent modifications to the mobile database.
    /// This context is read-only.
    /// </summary>
    public override int SaveChanges()
    {
        throw new InvalidOperationException("GeofenceMobileDbContext is read-only. Modifications are not allowed.");
    }

    /// <summary>
    /// Override SaveChanges to prevent modifications to the mobile database.
    /// This context is read-only.
    /// </summary>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        throw new InvalidOperationException("GeofenceMobileDbContext is read-only. Modifications are not allowed.");
    }

    /// <summary>
    /// Override SaveChangesAsync to prevent modifications to the mobile database.
    /// This context is read-only.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("GeofenceMobileDbContext is read-only. Modifications are not allowed.");
    }

    /// <summary>
    /// Override SaveChangesAsync to prevent modifications to the mobile database.
    /// This context is read-only.
    /// </summary>
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("GeofenceMobileDbContext is read-only. Modifications are not allowed.");
    }
}
