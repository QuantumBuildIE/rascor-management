using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Persistence.Configurations;

public class AttendanceSummaryConfiguration : IEntityTypeConfiguration<AttendanceSummary>
{
    public void Configure(EntityTypeBuilder<AttendanceSummary> builder)
    {
        builder.ToTable("attendance_summaries", "site_attendance");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Date)
            .IsRequired();

        builder.Property(e => e.ExpectedHours)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(e => e.UtilizationPercent)
            .HasPrecision(5, 2);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Site)
            .WithMany()
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: one summary per employee/site/date per tenant
        builder.HasIndex(e => new { e.TenantId, e.EmployeeId, e.SiteId, e.Date })
            .IsUnique();

        builder.HasIndex(e => new { e.TenantId, e.Date });
        builder.HasIndex(e => new { e.TenantId, e.EmployeeId });
        builder.HasIndex(e => new { e.TenantId, e.SiteId });
        builder.HasIndex(e => e.Status);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
