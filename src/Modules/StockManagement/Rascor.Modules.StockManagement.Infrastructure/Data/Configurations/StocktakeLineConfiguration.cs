using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Modules.StockManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for StocktakeLine entity
/// </summary>
public class StocktakeLineConfiguration : IEntityTypeConfiguration<StocktakeLine>
{
    public void Configure(EntityTypeBuilder<StocktakeLine> builder)
    {
        // Table name
        builder.ToTable("stocktake_lines");

        // Primary key
        builder.HasKey(stl => stl.Id);

        // Properties
        builder.Property(stl => stl.StocktakeId)
            .IsRequired();

        builder.Property(stl => stl.ProductId)
            .IsRequired();

        builder.Property(stl => stl.SystemQuantity)
            .IsRequired();

        builder.Property(stl => stl.CountedQuantity);

        builder.Property(stl => stl.AdjustmentCreated)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(stl => stl.VarianceReason)
            .HasMaxLength(500);

        builder.Property(stl => stl.BayLocationId);

        builder.Property(stl => stl.BayCode)
            .HasMaxLength(50);

        builder.Property(stl => stl.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(stl => stl.CreatedAt)
            .IsRequired();

        builder.Property(stl => stl.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(stl => stl.UpdatedAt);

        builder.Property(stl => stl.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(stl => stl.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(stl => stl.Product)
            .WithMany()
            .HasForeignKey(stl => stl.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Stocktake relationship configured in StocktakeConfiguration

        // Indexes
        builder.HasIndex(stl => stl.TenantId);

        builder.HasIndex(stl => stl.StocktakeId);

        builder.HasIndex(stl => stl.ProductId);
    }
}
