using Microsoft.EntityFrameworkCore;
using Rascor.Core.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Persistence;

public class SiteAttendanceDbContext : DbContext
{
    public SiteAttendanceDbContext(DbContextOptions<SiteAttendanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<AttendanceEvent> AttendanceEvents => Set<AttendanceEvent>();
    public DbSet<AttendanceSummary> AttendanceSummaries => Set<AttendanceSummary>();
    public DbSet<SitePhotoAttendance> SitePhotoAttendances => Set<SitePhotoAttendance>();
    public DbSet<DeviceRegistration> DeviceRegistrations => Set<DeviceRegistration>();
    public DbSet<BankHoliday> BankHolidays => Set<BankHoliday>();
    public DbSet<AttendanceSettings> AttendanceSettings => Set<AttendanceSettings>();
    public DbSet<AttendanceNotification> AttendanceNotifications => Set<AttendanceNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("site_attendance");

        // Configure Core entities referenced by this module (they exist in public schema)
        // These tables already exist - we just need to reference them for FK relationships
        // Mark them as excluded from migrations to prevent EF from trying to create them
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees", "public", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);

            // Ignore navigation properties to prevent EF from discovering related entities
            entity.Ignore(e => e.PrimarySite);
            entity.Ignore(e => e.FullName);
        });

        modelBuilder.Entity<Site>(entity =>
        {
            entity.ToTable("Sites", "public", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);

            // Ignore navigation properties to prevent EF from discovering related entities
            entity.Ignore(e => e.SiteManager);
            entity.Ignore(e => e.Company);
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SiteAttendanceDbContext).Assembly);
    }
}
