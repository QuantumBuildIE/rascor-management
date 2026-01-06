using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rascor.Core.Domain.Entities;

namespace Rascor.Core.Infrastructure.Persistence;

/// <summary>
/// Seeds test data for Site Attendance module - Core entities only (Employees, Sites)
/// Module-specific seeding is done by SiteAttendanceDataSeeder in SiteAttendance.Infrastructure
/// </summary>
public static class SiteAttendanceSeeder
{
    /// <summary>
    /// Seed Site Attendance related core data (Employees and Site GPS coordinates)
    /// </summary>
    public static async Task SeedAsync(DbContext context, ILogger logger)
    {
        await SeedEmployeesAsync(context, logger);
        await UpdateSitesWithGpsAsync(context, logger);
    }

    private static async Task SeedEmployeesAsync(DbContext context, ILogger logger)
    {
        var employees = context.Set<Employee>();

        if (await employees.IgnoreQueryFilters().AnyAsync())
        {
            logger.LogInformation("Employees already exist, skipping employee seeding");
            return;
        }

        var employeesToCreate = new List<Employee>
        {
            new Employee
            {
                Id = Guid.Parse("e1111111-1111-1111-1111-111111111111"),
                TenantId = DataSeeder.DefaultTenantId,
                EmployeeCode = "EMP001",
                FirstName = "Michael",
                LastName = "O'Brien",
                Email = "michael.obrien@rascor.ie",
                Phone = "+353 1 555 0101",
                Mobile = "+353 86 555 0101",
                JobTitle = "Site Supervisor",
                Department = "Construction",
                PrimarySiteId = Guid.Parse("22222222-2222-2222-2222-222222222222"), // Quantum Build
                IsActive = true,
                StartDate = new DateTime(2022, 3, 15, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Employee
            {
                Id = Guid.Parse("e2222222-2222-2222-2222-222222222222"),
                TenantId = DataSeeder.DefaultTenantId,
                EmployeeCode = "EMP002",
                FirstName = "Sean",
                LastName = "Murphy",
                Email = "sean.murphy@rascor.ie",
                Phone = "+353 1 555 0102",
                Mobile = "+353 87 555 0102",
                JobTitle = "Carpenter",
                Department = "Construction",
                PrimarySiteId = Guid.Parse("22222222-2222-2222-2222-222222222222"), // Quantum Build
                IsActive = true,
                StartDate = new DateTime(2021, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Employee
            {
                Id = Guid.Parse("e3333333-3333-3333-3333-333333333333"),
                TenantId = DataSeeder.DefaultTenantId,
                EmployeeCode = "EMP003",
                FirstName = "Aoife",
                LastName = "Walsh",
                Email = "aoife.walsh@rascor.ie",
                Phone = "+353 1 555 0103",
                Mobile = "+353 85 555 0103",
                JobTitle = "Electrician",
                Department = "Electrical",
                PrimarySiteId = Guid.Parse("33333333-3333-3333-3333-333333333333"), // South West Gate
                IsActive = true,
                StartDate = new DateTime(2023, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Employee
            {
                Id = Guid.Parse("e4444444-4444-4444-4444-444444444444"),
                TenantId = DataSeeder.DefaultTenantId,
                EmployeeCode = "EMP004",
                FirstName = "Patrick",
                LastName = "Kelly",
                Email = "patrick.kelly@rascor.ie",
                Phone = "+353 1 555 0104",
                Mobile = "+353 86 555 0104",
                JobTitle = "Plumber",
                Department = "Plumbing",
                PrimarySiteId = Guid.Parse("33333333-3333-3333-3333-333333333333"), // South West Gate
                IsActive = true,
                StartDate = new DateTime(2022, 8, 20, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Employee
            {
                Id = Guid.Parse("e5555555-5555-5555-5555-555555555555"),
                TenantId = DataSeeder.DefaultTenantId,
                EmployeeCode = "EMP005",
                FirstName = "Ciara",
                LastName = "Ryan",
                Email = "ciara.ryan@rascor.ie",
                Phone = "+353 1 555 0105",
                Mobile = "+353 87 555 0105",
                JobTitle = "Project Manager",
                Department = "Management",
                PrimarySiteId = Guid.Parse("44444444-4444-4444-4444-444444444444"), // Marmalade Lane
                IsActive = true,
                StartDate = new DateTime(2021, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Employee
            {
                Id = Guid.Parse("e6666666-6666-6666-6666-666666666666"),
                TenantId = DataSeeder.DefaultTenantId,
                EmployeeCode = "EMP006",
                FirstName = "Declan",
                LastName = "Byrne",
                Email = "declan.byrne@rascor.ie",
                Phone = "+353 1 555 0106",
                Mobile = "+353 85 555 0106",
                JobTitle = "General Operative",
                Department = "Construction",
                PrimarySiteId = Guid.Parse("55555555-5555-5555-5555-555555555555"), // Rathbourne Crossing
                IsActive = true,
                StartDate = new DateTime(2023, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Employee
            {
                Id = Guid.Parse("e7777777-7777-7777-7777-777777777777"),
                TenantId = DataSeeder.DefaultTenantId,
                EmployeeCode = "EMP007",
                FirstName = "Niamh",
                LastName = "Doyle",
                Email = "niamh.doyle@rascor.ie",
                Phone = "+353 1 555 0107",
                Mobile = "+353 86 555 0107",
                JobTitle = "Safety Officer",
                Department = "Health & Safety",
                PrimarySiteId = null, // Works across sites
                IsActive = true,
                StartDate = new DateTime(2022, 11, 15, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Employee
            {
                Id = Guid.Parse("e8888888-8888-8888-8888-888888888888"),
                TenantId = DataSeeder.DefaultTenantId,
                EmployeeCode = "EMP008",
                FirstName = "Brian",
                LastName = "McCarthy",
                Email = "brian.mccarthy@rascor.ie",
                Phone = "+353 1 555 0108",
                Mobile = "+353 87 555 0108",
                JobTitle = "Crane Operator",
                Department = "Construction",
                PrimarySiteId = Guid.Parse("66666666-6666-6666-6666-666666666666"), // Castleforbes Prem Inn
                IsActive = true,
                StartDate = new DateTime(2021, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            }
        };

        await employees.AddRangeAsync(employeesToCreate);
        await context.SaveChangesAsync();
        logger.LogInformation("Created {Count} test employees", employeesToCreate.Count);
    }

    private static async Task UpdateSitesWithGpsAsync(DbContext context, ILogger logger)
    {
        var sites = context.Set<Site>();

        // GPS coordinates for Irish construction sites (realistic Dublin/Cork/Galway/Limerick/Waterford locations)
        var siteGpsData = new Dictionary<Guid, (decimal Lat, decimal Lon, int Radius)>
        {
            // Dublin - Quantum Build (Dublin Docklands area)
            { Guid.Parse("22222222-2222-2222-2222-222222222222"), (53.3498m, -6.2603m, 100) },
            // Cork - South West Gate (Cork city centre)
            { Guid.Parse("33333333-3333-3333-3333-333333333333"), (51.8969m, -8.4863m, 120) },
            // Galway - Marmalade Lane (Galway city)
            { Guid.Parse("44444444-4444-4444-4444-444444444444"), (53.2707m, -9.0568m, 100) },
            // Dublin - Rathbourne Crossing (North Dublin)
            { Guid.Parse("55555555-5555-5555-5555-555555555555"), (53.3883m, -6.3757m, 150) },
            // Dublin - Castleforbes Prem Inn (Dublin city centre)
            { Guid.Parse("66666666-6666-6666-6666-666666666666"), (53.3524m, -6.2458m, 80) },
            // Limerick - Eden
            { Guid.Parse("77777777-7777-7777-7777-777777777777"), (52.6638m, -8.6267m, 100) },
            // Waterford - Ford
            { Guid.Parse("88888888-8888-8888-8888-888888888888"), (52.2583m, -7.1119m, 100) }
        };

        var updated = 0;
        foreach (var (siteId, gps) in siteGpsData)
        {
            var site = await sites.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == siteId);
            if (site != null && site.Latitude == null)
            {
                site.Latitude = gps.Lat;
                site.Longitude = gps.Lon;
                site.GeofenceRadiusMeters = gps.Radius;
                updated++;
            }
        }

        if (updated > 0)
        {
            await context.SaveChangesAsync();
            logger.LogInformation("Updated {Count} sites with GPS coordinates", updated);
        }
        else
        {
            logger.LogInformation("Sites already have GPS coordinates, skipping");
        }
    }
}
