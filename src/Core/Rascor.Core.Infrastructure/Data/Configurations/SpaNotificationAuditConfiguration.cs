using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Core.Domain.Entities;

namespace Rascor.Core.Infrastructure.Data.Configurations;

public class SpaNotificationAuditConfiguration : IEntityTypeConfiguration<SpaNotificationAudit>
{
    public void Configure(EntityTypeBuilder<SpaNotificationAudit> builder)
    {
        builder.ToTable("spa_notification_audit");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ScheduledHours)
            .HasPrecision(5, 2);

        builder.Property(x => x.NotificationType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.NotificationMethod)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.RecipientEmail)
            .HasMaxLength(255);

        builder.Property(x => x.Status)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(x => x.EmailProviderId)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Site)
            .WithMany()
            .HasForeignKey(x => x.SiteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for common query patterns
        builder.HasIndex(x => new { x.TenantId, x.ScheduledDate })
            .HasDatabaseName("IX_SpaNotificationAudit_TenantId_ScheduledDate");

        builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.ScheduledDate })
            .HasDatabaseName("IX_SpaNotificationAudit_TenantId_EmployeeId_ScheduledDate");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("IX_SpaNotificationAudit_TenantId_Status");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_SpaNotificationAudit_CreatedAt");
    }
}
