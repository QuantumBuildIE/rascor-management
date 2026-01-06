using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Modules.StockManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Product entity
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Table name
        builder.ToTable("products");

        // Primary key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.ProductCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.CategoryId)
            .IsRequired();

        builder.Property(p => p.SupplierId);

        builder.Property(p => p.UnitType)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Each");

        builder.Property(p => p.BaseRate)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.ReorderLevel)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.ReorderQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.LeadTimeDays)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.QrCodeData)
            .HasMaxLength(500);

        builder.Property(p => p.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.UpdatedAt);

        builder.Property(p => p.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Supplier)
            .WithMany()
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(p => p.TenantId);

        // Unique index on TenantId and ProductCode
        builder.HasIndex(p => new { p.TenantId, p.ProductCode })
            .IsUnique()
            .HasDatabaseName("ix_products_tenant_code");

        builder.HasIndex(p => p.CategoryId);

        builder.HasIndex(p => p.SupplierId);

        builder.HasIndex(p => p.IsActive);

        builder.HasIndex(p => p.ProductName);
    }
}
