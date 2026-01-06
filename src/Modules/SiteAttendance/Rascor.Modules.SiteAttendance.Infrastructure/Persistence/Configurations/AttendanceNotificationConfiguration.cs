using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Persistence.Configurations;

public class AttendanceNotificationConfiguration : IEntityTypeConfiguration<AttendanceNotification>
{
    public void Configure(EntityTypeBuilder<AttendanceNotification> builder)
    {
        builder.ToTable("attendance_notifications", "site_attendance");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.NotificationType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Reason)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.Message)
            .HasMaxLength(500);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(500);

        builder.Property(e => e.SentAt)
            .IsRequired();

        builder.HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.RelatedEvent)
            .WithMany()
            .HasForeignKey(e => e.RelatedEventId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.TenantId, e.EmployeeId });
        builder.HasIndex(e => e.SentAt);
        builder.HasIndex(e => new { e.TenantId, e.Delivered });
        builder.HasIndex(e => e.NotificationType);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
