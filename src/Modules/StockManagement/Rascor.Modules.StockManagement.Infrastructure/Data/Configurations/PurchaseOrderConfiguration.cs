using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.StockManagement.Domain.Entities;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for PurchaseOrder entity
/// </summary>
public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        // Table name
        builder.ToTable("purchase_orders");

        // Primary key
        builder.HasKey(po => po.Id);

        // Properties
        builder.Property(po => po.PoNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(po => po.SupplierId)
            .IsRequired();

        builder.Property(po => po.OrderDate)
            .IsRequired();

        builder.Property(po => po.ExpectedDate);

        builder.Property(po => po.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(PurchaseOrderStatus.Draft);

        builder.Property(po => po.TotalValue)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(po => po.Notes)
            .HasMaxLength(1000);

        builder.Property(po => po.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(po => po.CreatedAt)
            .IsRequired();

        builder.Property(po => po.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(po => po.UpdatedAt);

        builder.Property(po => po.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(po => po.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(po => po.Supplier)
            .WithMany()
            .HasForeignKey(po => po.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(po => po.Lines)
            .WithOne(pol => pol.PurchaseOrder)
            .HasForeignKey(pol => pol.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(po => po.TenantId);

        // Unique index on TenantId and PoNumber
        builder.HasIndex(po => new { po.TenantId, po.PoNumber })
            .IsUnique()
            .HasDatabaseName("ix_purchase_orders_tenant_po_number");

        builder.HasIndex(po => po.SupplierId);

        builder.HasIndex(po => po.Status);

        builder.HasIndex(po => po.OrderDate);
    }
}
