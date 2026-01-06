using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.Proposals.Domain.Entities;

namespace Rascor.Modules.Proposals.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for ProposalSection entity
/// </summary>
public class ProposalSectionConfiguration : IEntityTypeConfiguration<ProposalSection>
{
    public void Configure(EntityTypeBuilder<ProposalSection> builder)
    {
        // Table name
        builder.ToTable("proposal_sections");

        // Primary key
        builder.HasKey(ps => ps.Id);

        // Properties
        builder.Property(ps => ps.ProposalId)
            .IsRequired();

        builder.Property(ps => ps.SourceKitId);

        builder.Property(ps => ps.SectionName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ps => ps.Description)
            .HasMaxLength(1000);

        builder.Property(ps => ps.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ps => ps.SectionCost)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(ps => ps.SectionTotal)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(ps => ps.SectionMargin)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(ps => ps.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(ps => ps.CreatedAt)
            .IsRequired();

        builder.Property(ps => ps.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ps => ps.UpdatedAt);

        builder.Property(ps => ps.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(ps => ps.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(ps => ps.Proposal)
            .WithMany(p => p.Sections)
            .HasForeignKey(ps => ps.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ps => ps.LineItems)
            .WithOne(li => li.Section)
            .HasForeignKey(li => li.ProposalSectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ps => ps.ProposalId);

        builder.HasIndex(ps => new { ps.ProposalId, ps.SortOrder });
    }
}
