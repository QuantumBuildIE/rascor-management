using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.Rams.Domain.Entities;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Infrastructure.Configurations;

public class RamsDocumentConfiguration : IEntityTypeConfiguration<RamsDocument>
{
    public void Configure(EntityTypeBuilder<RamsDocument> builder)
    {
        builder.ToTable("RamsDocuments", "rams");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProjectName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ProjectReference)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ProjectType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.ClientName)
            .HasMaxLength(200);

        builder.Property(x => x.SiteAddress)
            .HasMaxLength(500);

        builder.Property(x => x.AreaOfActivity)
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(RamsStatus.Draft);

        builder.Property(x => x.ApprovalComments)
            .HasMaxLength(2000);

        builder.Property(x => x.MethodStatementBody);

        builder.Property(x => x.GeneratedPdfUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.TenantId)
            .IsRequired();

        // Audit fields
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasMany(x => x.RiskAssessments)
            .WithOne(x => x.RamsDocument)
            .HasForeignKey(x => x.RamsDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.MethodSteps)
            .WithOne(x => x.RamsDocument)
            .HasForeignKey(x => x.RamsDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_rams_documents_tenant");

        builder.HasIndex(x => x.ProjectReference)
            .HasDatabaseName("ix_rams_documents_reference");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_rams_documents_status");

        builder.HasIndex(x => new { x.TenantId, x.ProjectReference })
            .IsUnique()
            .HasDatabaseName("ix_rams_documents_tenant_reference_unique");

        builder.HasIndex(x => new { x.TenantId, x.IsDeleted, x.Status })
            .HasDatabaseName("ix_rams_documents_tenant_deleted_status");

        // Query filter for soft delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
