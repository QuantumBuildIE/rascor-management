using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.Proposals.Domain.Entities;

namespace Rascor.Modules.Proposals.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for ProposalLineItem entity
/// </summary>
public class ProposalLineItemConfiguration : IEntityTypeConfiguration<ProposalLineItem>
{
    public void Configure(EntityTypeBuilder<ProposalLineItem> builder)
    {
        // Table name
        builder.ToTable("proposal_line_items");

        // Primary key
        builder.HasKey(li => li.Id);

        // Properties
        builder.Property(li => li.ProposalSectionId)
            .IsRequired();

        builder.Property(li => li.ProductId);

        builder.Property(li => li.ProductCode)
            .HasMaxLength(50);

        builder.Property(li => li.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(li => li.Quantity)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(li => li.Unit)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Each");

        builder.Property(li => li.UnitCost)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(li => li.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(li => li.LineTotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(li => li.LineCost)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(li => li.LineMargin)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(li => li.MarginPercent)
            .IsRequired()
            .HasPrecision(5, 2)
            .HasDefaultValue(0);

        builder.Property(li => li.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(li => li.Notes)
            .HasMaxLength(1000);

        builder.Property(li => li.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(li => li.CreatedAt)
            .IsRequired();

        builder.Property(li => li.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(li => li.UpdatedAt);

        builder.Property(li => li.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(li => li.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(li => li.Section)
            .WithMany(s => s.LineItems)
            .HasForeignKey(li => li.ProposalSectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(li => li.ProposalSectionId);

        builder.HasIndex(li => li.ProductId);

        builder.HasIndex(li => new { li.ProposalSectionId, li.SortOrder });
    }
}
