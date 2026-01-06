using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Modules.StockManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for StockLevel entity
/// </summary>
public class StockLevelConfiguration : IEntityTypeConfiguration<StockLevel>
{
    public void Configure(EntityTypeBuilder<StockLevel> builder)
    {
        // Table name
        builder.ToTable("stock_levels");

        // Primary key
        builder.HasKey(sl => sl.Id);

        // Properties
        builder.Property(sl => sl.ProductId)
            .IsRequired();

        builder.Property(sl => sl.LocationId)
            .IsRequired();

        builder.Property(sl => sl.QuantityOnHand)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sl => sl.QuantityReserved)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sl => sl.QuantityOnOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sl => sl.BinLocation)
            .HasMaxLength(50);

        builder.Property(sl => sl.LastMovementDate);

        builder.Property(sl => sl.LastCountDate);

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

        // Relationships
        builder.HasOne(sl => sl.Product)
            .WithMany()
            .HasForeignKey(sl => sl.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sl => sl.Location)
            .WithMany()
            .HasForeignKey(sl => sl.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sl => sl.BayLocation)
            .WithMany()
            .HasForeignKey(sl => sl.BayLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(sl => sl.TenantId);

        // Unique index on TenantId, ProductId, and LocationId
        // Only one stock level record per product per location per tenant
        builder.HasIndex(sl => new { sl.TenantId, sl.ProductId, sl.LocationId })
            .IsUnique()
            .HasDatabaseName("ix_stock_levels_tenant_product_location");

        builder.HasIndex(sl => sl.ProductId);

        builder.HasIndex(sl => sl.LocationId);
    }
}
