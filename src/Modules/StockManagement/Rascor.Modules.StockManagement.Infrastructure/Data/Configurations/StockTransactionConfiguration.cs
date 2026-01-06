using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.StockManagement.Domain.Entities;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for StockTransaction entity
/// </summary>
public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        // Table name
        builder.ToTable("stock_transactions");

        // Primary key
        builder.HasKey(st => st.Id);

        // Properties
        builder.Property(st => st.TransactionNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(st => st.TransactionDate)
            .IsRequired();

        builder.Property(st => st.TransactionType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(st => st.ProductId)
            .IsRequired();

        builder.Property(st => st.LocationId)
            .IsRequired();

        builder.Property(st => st.Quantity)
            .IsRequired();

        builder.Property(st => st.ReferenceType)
            .HasMaxLength(50);

        builder.Property(st => st.ReferenceId);

        builder.Property(st => st.Notes)
            .HasMaxLength(500);

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
        builder.HasOne(st => st.Product)
            .WithMany()
            .HasForeignKey(st => st.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(st => st.Location)
            .WithMany()
            .HasForeignKey(st => st.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(st => st.TenantId);

        builder.HasIndex(st => new { st.TenantId, st.TransactionDate })
            .HasDatabaseName("ix_stock_transactions_tenant_date");

        builder.HasIndex(st => st.TransactionNumber);

        builder.HasIndex(st => st.ProductId);

        builder.HasIndex(st => st.LocationId);

        builder.HasIndex(st => st.TransactionType);

        builder.HasIndex(st => new { st.ReferenceType, st.ReferenceId })
            .HasDatabaseName("ix_stock_transactions_reference");
    }
}
