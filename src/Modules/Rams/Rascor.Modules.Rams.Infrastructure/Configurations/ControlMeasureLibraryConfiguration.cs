using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.Rams.Domain.Entities;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Infrastructure.Configurations;

public class ControlMeasureLibraryConfiguration : IEntityTypeConfiguration<ControlMeasureLibrary>
{
    public void Configure(EntityTypeBuilder<ControlMeasureLibrary> builder)
    {
        builder.ToTable("RamsControlMeasureLibrary", "rams");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Hierarchy)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.ApplicableToCategory)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Keywords)
            .HasMaxLength(500);

        builder.Property(x => x.TypicalLikelihoodReduction)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.TypicalSeverityReduction)
            .IsRequired()
            .HasDefaultValue(0);

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
            .HasDatabaseName("ix_rams_control_measure_library_tenant");

        builder.HasIndex(x => x.Code)
            .HasDatabaseName("ix_rams_control_measure_library_code");

        builder.HasIndex(x => x.Hierarchy)
            .HasDatabaseName("ix_rams_control_measure_library_hierarchy");

        builder.HasIndex(x => new { x.TenantId, x.Code })
            .IsUnique()
            .HasDatabaseName("ix_rams_control_measure_library_tenant_code_unique");

        builder.HasIndex(x => new { x.TenantId, x.IsDeleted, x.IsActive })
            .HasDatabaseName("ix_rams_control_measure_library_tenant_deleted_active");

        // Query filter for soft delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
