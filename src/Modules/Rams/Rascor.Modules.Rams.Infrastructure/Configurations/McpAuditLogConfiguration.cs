using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.Rams.Domain.Entities;

namespace Rascor.Modules.Rams.Infrastructure.Configurations;

public class McpAuditLogConfiguration : IEntityTypeConfiguration<McpAuditLog>
{
    public void Configure(EntityTypeBuilder<McpAuditLog> builder)
    {
        builder.ToTable("RamsMcpAuditLogs", "rams");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RequestType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.InputPrompt)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.InputContext)
            .HasMaxLength(8000);

        // Use text column type for large fields
        builder.Property(x => x.AiResponse);

        builder.Property(x => x.ExtractedContent);

        builder.Property(x => x.ModelUsed)
            .HasMaxLength(100);

        builder.Property(x => x.CostEstimate)
            .HasPrecision(10, 6);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

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

        // Relationships
        builder.HasOne(x => x.RamsDocument)
            .WithMany()
            .HasForeignKey(x => x.RamsDocumentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.RiskAssessment)
            .WithMany()
            .HasForeignKey(x => x.RiskAssessmentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_rams_mcp_audit_logs_tenant");

        builder.HasIndex(x => x.RamsDocumentId)
            .HasDatabaseName("ix_rams_mcp_audit_logs_document");

        builder.HasIndex(x => x.RequestedAt)
            .HasDatabaseName("ix_rams_mcp_audit_logs_requested_at");

        builder.HasIndex(x => new { x.TenantId, x.IsDeleted })
            .HasDatabaseName("ix_rams_mcp_audit_logs_tenant_deleted");

        // Query filter for soft delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
