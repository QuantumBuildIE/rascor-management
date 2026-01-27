using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Persistence.Configurations;

public class SitePhotoAttendanceConfiguration : IEntityTypeConfiguration<SitePhotoAttendance>
{
    public void Configure(EntityTypeBuilder<SitePhotoAttendance> builder)
    {
        builder.ToTable("site_photo_attendances", "site_attendance");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventDate)
            .IsRequired();

        builder.Property(e => e.WeatherConditions)
            .HasMaxLength(100);

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500);

        builder.Property(e => e.SignatureUrl)
            .HasMaxLength(500);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.Latitude)
            .HasPrecision(10, 8);

        builder.Property(e => e.Longitude)
            .HasPrecision(11, 8);

        builder.Property(e => e.DistanceToSite)
            .HasPrecision(10, 2);

        builder.HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Site)
            .WithMany()
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.TenantId, e.EmployeeId });
        builder.HasIndex(e => new { e.TenantId, e.SiteId });
        builder.HasIndex(e => new { e.TenantId, e.EventDate });
        builder.HasIndex(e => new { e.TenantId, e.EmployeeId, e.EventDate });

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
