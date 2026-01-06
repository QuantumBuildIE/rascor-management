using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.StockManagement.Domain.Entities;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Stocktake entity
/// </summary>
public class StocktakeConfiguration : IEntityTypeConfiguration<Stocktake>
{
    public void Configure(EntityTypeBuilder<Stocktake> builder)
    {
        // Table name
        builder.ToTable("stocktakes");

        // Primary key
        builder.HasKey(st => st.Id);

        // Properties
        builder.Property(st => st.StocktakeNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(st => st.LocationId)
            .IsRequired();

        builder.Property(st => st.CountDate)
            .IsRequired();

        builder.Property(st => st.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(StocktakeStatus.Draft);

        builder.Property(st => st.CountedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(st => st.Notes)
            .HasMaxLength(1000);

        builder.Property(st => st.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(st => st.CreatedAt)
            .IsRequired();

        builder.Property(st => st.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(st => st.UpdatedAt);

        builder.Property(st => st.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(st => st.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(st => st.Location)
            .WithMany()
            .HasForeignKey(st => st.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(st => st.Lines)
            .WithOne(stl => stl.Stocktake)
            .HasForeignKey(stl => stl.StocktakeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(st => st.TenantId);

        // Unique index on TenantId and StocktakeNumber
        builder.HasIndex(st => new { st.TenantId, st.StocktakeNumber })
            .IsUnique()
            .HasDatabaseName("ix_stocktakes_tenant_stocktake_number");

        builder.HasIndex(st => st.LocationId);

        builder.HasIndex(st => st.Status);

        builder.HasIndex(st => st.CountDate);
    }
}
