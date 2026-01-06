using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Modules.StockManagement.Infrastructure.Persistence.Configurations;

public class ProductKitConfiguration : IEntityTypeConfiguration<ProductKit>
{
    public void Configure(EntityTypeBuilder<ProductKit> builder)
    {
        builder.ToTable("ProductKits");

        builder.HasKey(pk => pk.Id);

        builder.Property(pk => pk.KitCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pk => pk.KitName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pk => pk.Description)
            .HasMaxLength(1000);

        builder.Property(pk => pk.Notes)
            .HasMaxLength(1000);

        builder.Property(pk => pk.TotalCost)
            .HasPrecision(18, 2);

        builder.Property(pk => pk.TotalPrice)
            .HasPrecision(18, 2);

        // Indexes
        builder.HasIndex(pk => pk.TenantId);
        builder.HasIndex(pk => new { pk.TenantId, pk.KitCode })
            .IsUnique();
        builder.HasIndex(pk => pk.CategoryId);
        builder.HasIndex(pk => pk.IsActive);

        // Relationships
        builder.HasOne(pk => pk.Category)
            .WithMany()
            .HasForeignKey(pk => pk.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(pk => pk.Items)
            .WithOne(pki => pki.ProductKit)
            .HasForeignKey(pki => pki.ProductKitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filter for soft delete and tenant isolation
        builder.HasQueryFilter(pk => !pk.IsDeleted && pk.TenantId == Guid.Parse("11111111-1111-1111-1111-111111111111"));
    }
}
