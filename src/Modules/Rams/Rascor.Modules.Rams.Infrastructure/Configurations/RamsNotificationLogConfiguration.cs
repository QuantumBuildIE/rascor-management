using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.Rams.Domain.Entities;

namespace Rascor.Modules.Rams.Infrastructure.Configurations;

public class RamsNotificationLogConfiguration : IEntityTypeConfiguration<RamsNotificationLog>
{
    public void Configure(EntityTypeBuilder<RamsNotificationLog> builder)
    {
        builder.ToTable("RamsNotificationLogs", "rams");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NotificationType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.RecipientEmail)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.RecipientName)
            .HasMaxLength(200);

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.BodyPreview)
            .HasMaxLength(1000);

        builder.Property(x => x.AttemptedAt)
            .IsRequired();

        builder.Property(x => x.WasSent)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(x => x.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.TriggeredByUserId)
            .HasMaxLength(100);

        builder.Property(x => x.TriggeredByUserName)
            .HasMaxLength(200);

        builder.Property(x => x.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationship to RamsDocument
        builder.HasOne(x => x.RamsDocument)
            .WithMany()
            .HasForeignKey(x => x.RamsDocumentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_rams_notification_logs_tenant");

        builder.HasIndex(x => x.RamsDocumentId)
            .HasDatabaseName("ix_rams_notification_logs_document");

        builder.HasIndex(x => x.AttemptedAt)
            .HasDatabaseName("ix_rams_notification_logs_attempted_at");

        builder.HasIndex(x => x.NotificationType)
            .HasDatabaseName("ix_rams_notification_logs_type");

        builder.HasIndex(x => new { x.WasSent, x.RetryCount })
            .HasDatabaseName("ix_rams_notification_logs_status");

        builder.HasIndex(x => new { x.TenantId, x.IsDeleted, x.AttemptedAt })
            .HasDatabaseName("ix_rams_notification_logs_tenant_deleted_date");

        // Query filter for soft delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
