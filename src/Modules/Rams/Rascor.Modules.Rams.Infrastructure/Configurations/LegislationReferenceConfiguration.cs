using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.Rams.Domain.Entities;

namespace Rascor.Modules.Rams.Infrastructure.Configurations;

public class LegislationReferenceConfiguration : IEntityTypeConfiguration<LegislationReference>
{
    public void Configure(EntityTypeBuilder<LegislationReference> builder)
    {
        builder.ToTable("RamsLegislationReferences", "rams");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.ShortName)
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Jurisdiction)
            .HasMaxLength(50);

        builder.Property(x => x.Keywords)
            .HasMaxLength(500);

        builder.Property(x => x.DocumentUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.ApplicableCategories)
            .HasMaxLength(500);

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
            .HasDatabaseName("ix_rams_legislation_references_tenant");

        builder.HasIndex(x => x.Code)
            .HasDatabaseName("ix_rams_legislation_references_code");

        builder.HasIndex(x => x.Jurisdiction)
            .HasDatabaseName("ix_rams_legislation_references_jurisdiction");

        builder.HasIndex(x => new { x.TenantId, x.Code })
            .IsUnique()
            .HasDatabaseName("ix_rams_legislation_references_tenant_code_unique");

        builder.HasIndex(x => new { x.TenantId, x.IsDeleted, x.IsActive })
            .HasDatabaseName("ix_rams_legislation_references_tenant_deleted_active");

        // Query filter for soft delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
