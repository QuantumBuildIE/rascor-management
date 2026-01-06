using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.Proposals.Domain.Entities;

namespace Rascor.Modules.Proposals.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Proposal entity
/// </summary>
public class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
    {
        // Table name
        builder.ToTable("proposals");

        // Primary key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.ProposalNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Version)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(p => p.ParentProposalId);

        builder.Property(p => p.CompanyId)
            .IsRequired();

        builder.Property(p => p.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.PrimaryContactId);

        builder.Property(p => p.PrimaryContactName)
            .HasMaxLength(200);

        builder.Property(p => p.ProjectName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.ProjectAddress)
            .HasMaxLength(500);

        builder.Property(p => p.ProjectDescription)
            .HasMaxLength(2000);

        builder.Property(p => p.ProposalDate)
            .IsRequired();

        builder.Property(p => p.ValidUntilDate);

        builder.Property(p => p.SubmittedDate);

        builder.Property(p => p.ApprovedDate);

        builder.Property(p => p.ApprovedBy)
            .HasMaxLength(200);

        builder.Property(p => p.WonDate);

        builder.Property(p => p.LostDate);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(ProposalStatus.Draft);

        builder.Property(p => p.WonLostReason)
            .HasMaxLength(500);

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("EUR");

        builder.Property(p => p.Subtotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.DiscountPercent)
            .IsRequired()
            .HasPrecision(5, 2)
            .HasDefaultValue(0);

        builder.Property(p => p.DiscountAmount)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(p => p.NetTotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.VatRate)
            .IsRequired()
            .HasPrecision(5, 2)
            .HasDefaultValue(23m);

        builder.Property(p => p.VatAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.GrandTotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.TotalCost)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(p => p.TotalMargin)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(p => p.MarginPercent)
            .IsRequired()
            .HasPrecision(5, 2)
            .HasDefaultValue(0);

        builder.Property(p => p.PaymentTerms)
            .HasMaxLength(500);

        builder.Property(p => p.TermsAndConditions)
            .HasMaxLength(4000);

        builder.Property(p => p.Notes)
            .HasMaxLength(2000);

        builder.Property(p => p.DrawingFileName)
            .HasMaxLength(255);

        builder.Property(p => p.DrawingUrl)
            .HasMaxLength(1000);

        builder.Property(p => p.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.UpdatedAt);

        builder.Property(p => p.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(p => p.ParentProposal)
            .WithMany(p => p.Revisions)
            .HasForeignKey(p => p.ParentProposalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Sections)
            .WithOne(s => s.Proposal)
            .HasForeignKey(s => s.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Contacts)
            .WithOne(c => c.Proposal)
            .HasForeignKey(c => c.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.TenantId);

        builder.HasIndex(p => new { p.TenantId, p.ProposalNumber })
            .IsUnique()
            .HasDatabaseName("ix_proposals_tenant_number");

        builder.HasIndex(p => p.Status);

        builder.HasIndex(p => p.CompanyId);

        builder.HasIndex(p => p.ProposalDate);

        builder.HasIndex(p => p.ParentProposalId);
    }
}
