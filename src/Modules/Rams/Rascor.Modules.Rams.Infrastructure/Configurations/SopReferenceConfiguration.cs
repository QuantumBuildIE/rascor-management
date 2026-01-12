using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.Rams.Domain.Entities;

namespace Rascor.Modules.Rams.Infrastructure.Configurations;

public class SopReferenceConfiguration : IEntityTypeConfiguration<SopReference>
{
    public void Configure(EntityTypeBuilder<SopReference> builder)
    {
        builder.ToTable("RamsSopReferences", "rams");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SopId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Topic)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.TaskKeywords)
            .HasMaxLength(500);

        builder.Property(x => x.PolicySnippet)
            .HasMaxLength(2000);

        builder.Property(x => x.ProcedureDetails)
            .HasMaxLength(4000);

        builder.Property(x => x.ApplicableLegislation)
            .HasMaxLength(500);

        builder.Property(x => x.DocumentUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

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

        // Indexes
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_rams_sop_references_tenant");

        builder.HasIndex(x => x.SopId)
            .HasDatabaseName("ix_rams_sop_references_sop_id");

        builder.HasIndex(x => new { x.TenantId, x.SopId })
            .IsUnique()
            .HasDatabaseName("ix_rams_sop_references_tenant_sop_id_unique");

        builder.HasIndex(x => new { x.TenantId, x.IsDeleted, x.IsActive })
            .HasDatabaseName("ix_rams_sop_references_tenant_deleted_active");

        // Query filter for soft delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
