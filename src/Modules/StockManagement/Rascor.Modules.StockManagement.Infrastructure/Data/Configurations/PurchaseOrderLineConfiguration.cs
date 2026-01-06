using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.StockManagement.Domain.Entities;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for PurchaseOrderLine entity
/// </summary>
public class PurchaseOrderLineConfiguration : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        // Table name
        builder.ToTable("purchase_order_lines");

        // Primary key
        builder.HasKey(pol => pol.Id);

        // Properties
        builder.Property(pol => pol.PurchaseOrderId)
            .IsRequired();

        builder.Property(pol => pol.ProductId)
            .IsRequired();

        builder.Property(pol => pol.QuantityOrdered)
            .IsRequired();

        builder.Property(pol => pol.QuantityReceived)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(pol => pol.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(pol => pol.LineStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(PurchaseOrderLineStatus.Open);

        builder.Property(pol => pol.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(pol => pol.CreatedAt)
            .IsRequired();

        builder.Property(pol => pol.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(pol => pol.UpdatedAt);

        builder.Property(pol => pol.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(pol => pol.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(pol => pol.Product)
            .WithMany()
            .HasForeignKey(pol => pol.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // PurchaseOrder relationship configured in PurchaseOrderConfiguration

        // Indexes
        builder.HasIndex(pol => pol.TenantId);

        builder.HasIndex(pol => pol.PurchaseOrderId);

        builder.HasIndex(pol => pol.ProductId);

        builder.HasIndex(pol => pol.LineStatus);
    }
}
