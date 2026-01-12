using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.Rams.Domain.Entities;

namespace Rascor.Modules.Rams.Infrastructure.Configurations;

public class MethodStepConfiguration : IEntityTypeConfiguration<MethodStep>
{
    public void Configure(EntityTypeBuilder<MethodStep> builder)
    {
        builder.ToTable("RamsMethodSteps", "rams");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StepNumber)
            .IsRequired();

        builder.Property(x => x.StepTitle)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.DetailedProcedure)
            .HasMaxLength(4000);

        builder.Property(x => x.RequiredPermits)
            .HasMaxLength(500);

        builder.Property(x => x.RequiresSignoff)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.SignoffUrl)
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

        // Relationship to linked risk assessment
        builder.HasOne(x => x.LinkedRiskAssessment)
            .WithMany()
            .HasForeignKey(x => x.LinkedRiskAssessmentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_rams_method_steps_tenant");

        builder.HasIndex(x => x.RamsDocumentId)
            .HasDatabaseName("ix_rams_method_steps_document");

        builder.HasIndex(x => new { x.RamsDocumentId, x.StepNumber })
            .HasDatabaseName("ix_rams_method_steps_document_order");

        // Query filter for soft delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
