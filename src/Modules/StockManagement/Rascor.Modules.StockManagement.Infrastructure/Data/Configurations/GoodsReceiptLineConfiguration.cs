using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Modules.StockManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for GoodsReceiptLine entity
/// </summary>
public class GoodsReceiptLineConfiguration : IEntityTypeConfiguration<GoodsReceiptLine>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptLine> builder)
    {
        // Table name
        builder.ToTable("goods_receipt_lines");

        // Primary key
        builder.HasKey(grl => grl.Id);

        // Properties
        builder.Property(grl => grl.GoodsReceiptId)
            .IsRequired();

        builder.Property(grl => grl.PurchaseOrderLineId);

        builder.Property(grl => grl.ProductId)
            .IsRequired();

        builder.Property(grl => grl.QuantityReceived)
            .IsRequired();

        builder.Property(grl => grl.Notes)
            .HasMaxLength(500);

        builder.Property(grl => grl.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(grl => grl.CreatedAt)
            .IsRequired();

        builder.Property(grl => grl.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(grl => grl.UpdatedAt);

        builder.Property(grl => grl.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(grl => grl.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(grl => grl.PurchaseOrderLine)
            .WithMany()
            .HasForeignKey(grl => grl.PurchaseOrderLineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(grl => grl.Product)
            .WithMany()
            .HasForeignKey(grl => grl.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(grl => grl.BayLocation)
            .WithMany()
            .HasForeignKey(grl => grl.BayLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        // GoodsReceipt relationship configured in GoodsReceiptConfiguration

        // Indexes
        builder.HasIndex(grl => grl.TenantId);

        builder.HasIndex(grl => grl.GoodsReceiptId);

        builder.HasIndex(grl => grl.PurchaseOrderLineId);

        builder.HasIndex(grl => grl.ProductId);
    }
}
