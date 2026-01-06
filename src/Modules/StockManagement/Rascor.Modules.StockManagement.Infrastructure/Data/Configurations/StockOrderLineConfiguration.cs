using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Modules.StockManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for StockOrderLine entity
/// </summary>
public class StockOrderLineConfiguration : IEntityTypeConfiguration<StockOrderLine>
{
    public void Configure(EntityTypeBuilder<StockOrderLine> builder)
    {
        // Table name
        builder.ToTable("stock_order_lines");

        // Primary key
        builder.HasKey(sol => sol.Id);

        // Properties
        builder.Property(sol => sol.StockOrderId)
            .IsRequired();

        builder.Property(sol => sol.ProductId)
            .IsRequired();

        builder.Property(sol => sol.QuantityRequested)
            .IsRequired();

        builder.Property(sol => sol.QuantityIssued)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sol => sol.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(sol => sol.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(sol => sol.CreatedAt)
            .IsRequired();

        builder.Property(sol => sol.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(sol => sol.UpdatedAt);

        builder.Property(sol => sol.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(sol => sol.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(sol => sol.Product)
            .WithMany()
            .HasForeignKey(sol => sol.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // StockOrder relationship configured in StockOrderConfiguration

        // Indexes
        builder.HasIndex(sol => sol.TenantId);

        builder.HasIndex(sol => sol.StockOrderId);

        builder.HasIndex(sol => sol.ProductId);
    }
}
