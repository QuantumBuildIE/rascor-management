using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Modules.Rams.Domain.Entities;

namespace Rascor.Modules.Rams.Infrastructure.Configurations;

public class HazardControlLinkConfiguration : IEntityTypeConfiguration<HazardControlLink>
{
    public void Configure(EntityTypeBuilder<HazardControlLink> builder)
    {
        builder.ToTable("RamsHazardControlLinks", "rams");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

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
        builder.HasOne(x => x.HazardLibrary)
            .WithMany(h => h.HazardControlLinks)
            .HasForeignKey(x => x.HazardLibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ControlMeasureLibrary)
            .WithMany(c => c.HazardControlLinks)
            .HasForeignKey(x => x.ControlMeasureLibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.HazardLibraryId)
            .HasDatabaseName("ix_rams_hazard_control_links_hazard");

        builder.HasIndex(x => x.ControlMeasureLibraryId)
            .HasDatabaseName("ix_rams_hazard_control_links_control");

        builder.HasIndex(x => new { x.HazardLibraryId, x.ControlMeasureLibraryId })
            .IsUnique()
            .HasDatabaseName("ix_rams_hazard_control_links_hazard_control_unique");

        // Query filter for soft delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
