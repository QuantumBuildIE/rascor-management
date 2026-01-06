using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Modules.StockManagement.Infrastructure.Persistence.Configurations;

public class ProductKitItemConfiguration : IEntityTypeConfiguration<ProductKitItem>
{
    public void Configure(EntityTypeBuilder<ProductKitItem> builder)
    {
        builder.ToTable("ProductKitItems");

        builder.HasKey(pki => pki.Id);

        builder.Property(pki => pki.DefaultQuantity)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(pki => pki.Notes)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(pki => pki.ProductKitId);
        builder.HasIndex(pki => pki.ProductId);

        // Unique constraint: same product can't be in a kit twice
        builder.HasIndex(pki => new { pki.ProductKitId, pki.ProductId })
            .IsUnique();

        // Relationships
        builder.HasOne(pki => pki.ProductKit)
            .WithMany(pk => pk.Items)
            .HasForeignKey(pki => pki.ProductKitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pki => pki.Product)
            .WithMany(p => p.KitItems)
            .HasForeignKey(pki => pki.ProductId)
            .OnDelete(DeleteBehavior.Restrict); // Can't delete product if in a kit

        // Query filter for soft delete and tenant isolation
        builder.HasQueryFilter(pki => !pki.IsDeleted && pki.TenantId == Guid.Parse("11111111-1111-1111-1111-111111111111"));
    }
}
