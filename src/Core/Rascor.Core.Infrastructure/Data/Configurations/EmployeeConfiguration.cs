using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rascor.Core.Domain.Entities;

namespace Rascor.Core.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EmployeeCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasMaxLength(255);

        builder.Property(e => e.Phone)
            .HasMaxLength(50);

        builder.Property(e => e.Mobile)
            .HasMaxLength(50);

        builder.Property(e => e.JobTitle)
            .HasMaxLength(100);

        builder.Property(e => e.Department)
            .HasMaxLength(100);

        builder.Property(e => e.UserId)
            .HasMaxLength(450);

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.PreferredLanguage)
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue("en");

        // Ignore computed property
        builder.Ignore(e => e.FullName);

        builder.HasIndex(e => new { e.TenantId, e.EmployeeCode })
            .IsUnique()
            .HasDatabaseName("IX_Employees_TenantId_EmployeeCode");

        builder.HasOne(e => e.PrimarySite)
            .WithMany()
            .HasForeignKey(e => e.PrimarySiteId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
