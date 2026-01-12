using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.Rams.Domain.Entities;

namespace Rascor.Modules.Rams.Infrastructure.Configurations;

public class RiskAssessmentConfiguration : IEntityTypeConfiguration<RiskAssessment>
{
    public void Configure(EntityTypeBuilder<RiskAssessment> builder)
    {
        builder.ToTable("RamsRiskAssessments", "rams");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TaskActivity)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.LocationArea)
            .HasMaxLength(200);

        builder.Property(x => x.HazardIdentified)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.WhoAtRisk)
            .HasMaxLength(200);

        builder.Property(x => x.InitialLikelihood)
            .IsRequired();

        builder.Property(x => x.InitialSeverity)
            .IsRequired();

        builder.Property(x => x.ControlMeasures)
            .HasMaxLength(4000);

        builder.Property(x => x.RelevantLegislation)
            .HasMaxLength(2000);

        builder.Property(x => x.ReferenceSops)
            .HasMaxLength(500);

        builder.Property(x => x.ResidualLikelihood)
            .IsRequired();

        builder.Property(x => x.ResidualSeverity)
            .IsRequired();

        builder.Property(x => x.IsAiGenerated)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

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

        // Ignore computed properties
        builder.Ignore(x => x.InitialRiskRating);
        builder.Ignore(x => x.InitialRiskLevel);
        builder.Ignore(x => x.ResidualRiskRating);
        builder.Ignore(x => x.ResidualRiskLevel);

        // Indexes
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_rams_risk_assessments_tenant");

        builder.HasIndex(x => x.RamsDocumentId)
            .HasDatabaseName("ix_rams_risk_assessments_document");

        builder.HasIndex(x => new { x.RamsDocumentId, x.SortOrder })
            .HasDatabaseName("ix_rams_risk_assessments_document_order");

        // Query filter for soft delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
