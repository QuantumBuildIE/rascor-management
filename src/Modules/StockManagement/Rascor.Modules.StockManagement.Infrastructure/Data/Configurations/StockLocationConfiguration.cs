using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.StockManagement.Domain.Entities;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for StockLocation entity
/// </summary>
public class StockLocationConfiguration : IEntityTypeConfiguration<StockLocation>
{
    public void Configure(EntityTypeBuilder<StockLocation> builder)
    {
        // Table name
        builder.ToTable("stock_locations");

        // Primary key
        builder.HasKey(sl => sl.Id);

        // Properties
        builder.Property(sl => sl.LocationCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(sl => sl.LocationName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sl => sl.LocationType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(sl => sl.Address)
            .HasMaxLength(500);

        builder.Property(sl => sl.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(sl => sl.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(sl => sl.CreatedAt)
            .IsRequired();

        builder.Property(sl => sl.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(sl => sl.UpdatedAt);

        builder.Property(sl => sl.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(sl => sl.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(sl => sl.TenantId);

        // Unique index on TenantId and LocationCode
        builder.HasIndex(sl => new { sl.TenantId, sl.LocationCode })
            .IsUnique()
            .HasDatabaseName("ix_stock_locations_tenant_code");

        builder.HasIndex(sl => sl.IsActive);

        builder.HasIndex(sl => sl.LocationType);
    }
}
