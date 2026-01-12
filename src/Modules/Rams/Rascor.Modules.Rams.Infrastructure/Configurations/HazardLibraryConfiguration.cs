using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.Rams.Domain.Entities;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Infrastructure.Configurations;

public class HazardLibraryConfiguration : IEntityTypeConfiguration<HazardLibrary>
{
    public void Configure(EntityTypeBuilder<HazardLibrary> builder)
    {
        builder.ToTable("RamsHazardLibrary", "rams");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Keywords)
            .HasMaxLength(500);

        builder.Property(x => x.DefaultLikelihood)
            .IsRequired()
            .HasDefaultValue(3);

        builder.Property(x => x.DefaultSeverity)
            .IsRequired()
            .HasDefaultValue(4);

        builder.Property(x => x.TypicalWhoAtRisk)
            .HasMaxLength(200);

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
            .HasDatabaseName("ix_rams_hazard_library_tenant");

        builder.HasIndex(x => x.Code)
            .HasDatabaseName("ix_rams_hazard_library_code");

        builder.HasIndex(x => x.Category)
            .HasDatabaseName("ix_rams_hazard_library_category");

        builder.HasIndex(x => new { x.TenantId, x.Code })
            .IsUnique()
            .HasDatabaseName("ix_rams_hazard_library_tenant_code_unique");

        builder.HasIndex(x => new { x.TenantId, x.IsDeleted, x.IsActive })
            .HasDatabaseName("ix_rams_hazard_library_tenant_deleted_active");

        // Query filter for soft delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
