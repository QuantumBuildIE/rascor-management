using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Core.Domain.Entities;

namespace Rascor.Core.Infrastructure.Data.Configurations;

public class FloatUnmatchedItemConfiguration : IEntityTypeConfiguration<FloatUnmatchedItem>
{
    public void Configure(EntityTypeBuilder<FloatUnmatchedItem> builder)
    {
        builder.ToTable("float_unmatched_items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.FloatName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.FloatEmail)
            .HasMaxLength(255);

        builder.Property(x => x.SuggestedMatchName)
            .HasMaxLength(200);

        builder.Property(x => x.MatchConfidence)
            .HasPrecision(5, 4);

        builder.Property(x => x.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ResolvedBy)
            .HasMaxLength(100);

        // Unique index on Float ID per tenant and item type
        builder.HasIndex(x => new { x.TenantId, x.ItemType, x.FloatId })
            .IsUnique()
            .HasDatabaseName("IX_FloatUnmatchedItems_TenantId_ItemType_FloatId");

        // Index for filtering by status
        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("IX_FloatUnmatchedItems_TenantId_Status");
    }
}
