using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.Proposals.Domain.Entities;

namespace Rascor.Modules.Proposals.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for ProposalContact entity
/// </summary>
public class ProposalContactConfiguration : IEntityTypeConfiguration<ProposalContact>
{
    public void Configure(EntityTypeBuilder<ProposalContact> builder)
    {
        // Table name
        builder.ToTable("proposal_contacts");

        // Primary key
        builder.HasKey(pc => pc.Id);

        // Properties
        builder.Property(pc => pc.ProposalId)
            .IsRequired();

        builder.Property(pc => pc.ContactId);

        builder.Property(pc => pc.ContactName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pc => pc.Email)
            .HasMaxLength(256);

        builder.Property(pc => pc.Phone)
            .HasMaxLength(50);

        builder.Property(pc => pc.Role)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pc => pc.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pc => pc.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(pc => pc.CreatedAt)
            .IsRequired();

        builder.Property(pc => pc.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(pc => pc.UpdatedAt);

        builder.Property(pc => pc.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(pc => pc.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(pc => pc.Proposal)
            .WithMany(p => p.Contacts)
            .HasForeignKey(pc => pc.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(pc => pc.ProposalId);

        builder.HasIndex(pc => pc.ContactId);
    }
}
