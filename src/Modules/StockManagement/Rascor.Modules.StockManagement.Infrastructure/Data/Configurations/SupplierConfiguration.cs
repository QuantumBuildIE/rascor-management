using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Modules.StockManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Supplier entity
/// </summary>
public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        // Table name
        builder.ToTable("suppliers");

        // Primary key
        builder.HasKey(s => s.Id);

        // Properties
        builder.Property(s => s.SupplierCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.SupplierName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.ContactName)
            .HasMaxLength(100);

        builder.Property(s => s.Email)
            .HasMaxLength(255);

        builder.Property(s => s.Phone)
            .HasMaxLength(50);

        builder.Property(s => s.Address)
            .HasMaxLength(500);

        builder.Property(s => s.PaymentTerms)
            .HasMaxLength(100);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.UpdatedAt);

        builder.Property(s => s.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(s => s.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(s => s.TenantId);

        // Unique index on TenantId and SupplierCode
        builder.HasIndex(s => new { s.TenantId, s.SupplierCode })
            .IsUnique()
            .HasDatabaseName("ix_suppliers_tenant_code");

        builder.HasIndex(s => s.IsActive);

        builder.HasIndex(s => s.SupplierName);
    }
}
