using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Modules.StockManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for BayLocation entity
/// </summary>
public class BayLocationConfiguration : IEntityTypeConfiguration<BayLocation>
{
    public void Configure(EntityTypeBuilder<BayLocation> builder)
    {
        // Table name
        builder.ToTable("bay_locations");

        // Primary key
        builder.HasKey(bl => bl.Id);

        // Properties
        builder.Property(bl => bl.BayCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(bl => bl.BayName)
            .HasMaxLength(200);

        builder.Property(bl => bl.StockLocationId)
            .IsRequired();

        builder.Property(bl => bl.Capacity);

        builder.Property(bl => bl.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(bl => bl.Notes)
            .HasMaxLength(1000);

        builder.Property(bl => bl.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(bl => bl.CreatedAt)
            .IsRequired();

        builder.Property(bl => bl.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(bl => bl.UpdatedAt);

        builder.Property(bl => bl.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(bl => bl.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(bl => bl.StockLocation)
            .WithMany()
            .HasForeignKey(bl => bl.StockLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(bl => bl.TenantId);

        // Unique index on TenantId, StockLocationId and BayCode
        builder.HasIndex(bl => new { bl.TenantId, bl.StockLocationId, bl.BayCode })
            .IsUnique()
            .HasDatabaseName("ix_bay_locations_tenant_location_code");

        builder.HasIndex(bl => bl.StockLocationId);

        builder.HasIndex(bl => bl.IsActive);
    }
}
