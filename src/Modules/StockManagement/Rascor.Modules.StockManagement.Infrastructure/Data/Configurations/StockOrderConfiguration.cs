using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.StockManagement.Domain.Entities;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for StockOrder entity
/// </summary>
public class StockOrderConfiguration : IEntityTypeConfiguration<StockOrder>
{
    public void Configure(EntityTypeBuilder<StockOrder> builder)
    {
        // Table name
        builder.ToTable("stock_orders");

        // Primary key
        builder.HasKey(so => so.Id);

        // Properties
        builder.Property(so => so.OrderNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(so => so.SiteId)
            .IsRequired();

        builder.Property(so => so.SiteName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(so => so.OrderDate)
            .IsRequired();

        builder.Property(so => so.RequiredDate);

        builder.Property(so => so.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(StockOrderStatus.Draft);

        builder.Property(so => so.OrderTotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(so => so.RequestedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(so => so.ApprovedBy)
            .HasMaxLength(100);

        builder.Property(so => so.ApprovedDate);

        builder.Property(so => so.CollectedDate);

        builder.Property(so => so.Notes)
            .HasMaxLength(1000);

        builder.Property(so => so.SourceLocationId)
            .IsRequired();

        builder.Property(so => so.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(so => so.CreatedAt)
            .IsRequired();

        builder.Property(so => so.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(so => so.UpdatedAt);

        builder.Property(so => so.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(so => so.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(so => so.SourceLocation)
            .WithMany()
            .HasForeignKey(so => so.SourceLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(so => so.Lines)
            .WithOne(sol => sol.StockOrder)
            .HasForeignKey(sol => sol.StockOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(so => so.TenantId);

        // Unique index on TenantId and OrderNumber
        builder.HasIndex(so => new { so.TenantId, so.OrderNumber })
            .IsUnique()
            .HasDatabaseName("ix_stock_orders_tenant_order_number");

        builder.HasIndex(so => so.SiteId);

        builder.HasIndex(so => so.Status);

        builder.HasIndex(so => so.OrderDate);

        builder.HasIndex(so => so.SourceLocationId);
    }
}
