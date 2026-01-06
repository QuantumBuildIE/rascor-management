using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Core.Domain.Entities;

namespace Rascor.Core.Infrastructure.Data.Configurations;

public class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.SiteCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.SiteName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Address)
            .HasMaxLength(500);

        builder.Property(e => e.City)
            .HasMaxLength(100);

        builder.Property(e => e.PostalCode)
            .HasMaxLength(20);

        builder.Property(e => e.Phone)
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .HasMaxLength(255);

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        // Geolocation fields for attendance geofencing
        builder.Property(e => e.Latitude)
            .HasPrecision(10, 8);

        builder.Property(e => e.Longitude)
            .HasPrecision(11, 8);

        builder.HasIndex(e => new { e.TenantId, e.SiteCode })
            .IsUnique()
            .HasDatabaseName("IX_Sites_TenantId_SiteCode");

        builder.HasOne(e => e.SiteManager)
            .WithMany()
            .HasForeignKey(e => e.SiteManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Company)
            .WithMany(c => c.Sites)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
