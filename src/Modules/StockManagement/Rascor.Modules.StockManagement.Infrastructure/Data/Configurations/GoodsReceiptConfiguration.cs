using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Modules.StockManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for GoodsReceipt entity
/// </summary>
public class GoodsReceiptConfiguration : IEntityTypeConfiguration<GoodsReceipt>
{
    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        // Table name
        builder.ToTable("goods_receipts");

        // Primary key
        builder.HasKey(gr => gr.Id);

        // Properties
        builder.Property(gr => gr.GrnNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(gr => gr.PurchaseOrderId);

        builder.Property(gr => gr.SupplierId)
            .IsRequired();

        builder.Property(gr => gr.LocationId)
            .IsRequired();

        builder.Property(gr => gr.ReceiptDate)
            .IsRequired();

        builder.Property(gr => gr.ReceivedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(gr => gr.Notes)
            .HasMaxLength(1000);

        builder.Property(gr => gr.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(gr => gr.CreatedAt)
            .IsRequired();

        builder.Property(gr => gr.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(gr => gr.UpdatedAt);

        builder.Property(gr => gr.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(gr => gr.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(gr => gr.PurchaseOrder)
            .WithMany()
            .HasForeignKey(gr => gr.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(gr => gr.Supplier)
            .WithMany()
            .HasForeignKey(gr => gr.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(gr => gr.Location)
            .WithMany()
            .HasForeignKey(gr => gr.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(gr => gr.Lines)
            .WithOne(grl => grl.GoodsReceipt)
            .HasForeignKey(grl => grl.GoodsReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(gr => gr.TenantId);

        // Unique index on TenantId and GrnNumber
        builder.HasIndex(gr => new { gr.TenantId, gr.GrnNumber })
            .IsUnique()
            .HasDatabaseName("ix_goods_receipts_tenant_grn_number");

        builder.HasIndex(gr => gr.PurchaseOrderId);

        builder.HasIndex(gr => gr.SupplierId);

        builder.HasIndex(gr => gr.LocationId);

        builder.HasIndex(gr => gr.ReceiptDate);
    }
}
