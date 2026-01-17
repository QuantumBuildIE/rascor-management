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

            // GeoTrackerID mappings by email (from external tracker system)
            // Note: EVT8985 is duplicated for Dylan Byrne and Shane Redmond - assigned to Dylan only
            var geoTrackerMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "leeberns98@hotmail.com", "EVT7716" },
                { "jwhyte747@gmail.com", "EVT2898" },
                { "ethan.vickers@rascor.com", "EVT3953" },
                { "dylan.byrne12322@gmail.com", "EVT8985" },
                { "u3094959461@gmail.com", "EVT9999" }, // Shane Redmond - temporary ID (original EVT8985 duplicated with Dylan Byrne)
                { "eanna9malone@gmail.com", "EVT5592" },
                { "mark.kelly@rascor.com", "EVT1230" },
                { "Jnr.Rascor@gmail.com", "EVT00070" },
                { "antonio.andrade@rascor.com", "EVT4042" },
                { "sean.alegra@rascor.com", "EVT3795" },
                { "damian.whelan@rascor.com", "EVT0059" },
                { "jakub.waszkowski@rascor.com", "EVT0013" },
                { "eduardo.rodrigues@rascor.com", "EVT0012" },
                { "grant.edgar@rascor.com", "EVT0011" },
                { "luke.buls.rascor@gmail.com", "EVT0009" },
                { "edi.ruggeri@rascor.com", "EVT0007" },
                { "quantumbuildrascor@gmail.com", "EVT0003" },
                { "eddieheffernan@gmail.com", "EVT0001" },
                { "donal@quantumbuild.ai", "EVT0572" }
            };

            var employeesToCreate = new List<Employee>
            {
                new Employee
                {
                    Id = Guid.Parse("e1111111-1111-1111-1111-111111111111"),
                    TenantId = DataSeeder.DefaultTenantId,
                    EmployeeCode = "EMP001",
                    FirstName = "Lee",
                    LastName = "Berns",
                    Email = "leeberns98@hotmail.com",
                    JobTitle = "General Operative",
                    Department = "Construction",
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
                    FirstName = "John",
                    LastName = "Whyte",
                    Email = "jwhyte747@gmail.com",
                    JobTitle = "General Operative",
                    Department = "Construction",
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
                    FirstName = "Ethan",
                    LastName = "Vickers",
                    Email = "ethan.vickers@rascor.com",
                    JobTitle = "General Operative",
                    Department = "Electrical",
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
                    FirstName = "Dylan",
                    LastName = "Byrne",
                    Email = "dylan.byrne12322@gmail.com",
                    JobTitle = "General Operative",
                    Department = "Plumbing",
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
                    FirstName = "Shane",
                    LastName = "Redmond",
                    Email = "u3094959461@gmail.com",
                    JobTitle = "General Operative",
                    Department = "Management",
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
                    FirstName = "Éanna",
                    LastName = "O'Mhaoleoin",
                    Email = "eanna9malone@gmail.com",
                    JobTitle = "General Operative",
                    Department = "Construction",
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
                    FirstName = "Mark",
                    LastName = "Kelly",
                    Email = "mark.kelly@rascor.com",
                    JobTitle = "Project Manager",
                    Department = "Construction",
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
                    FirstName = "Jnr",
                    LastName = "Jnr",
                    Email = "Jnr.Rascor@gmail.com",
                    JobTitle = "General Operative",
                    Department = "Construction",
                    IsActive = true,
                    StartDate = new DateTime(2021, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                },
                new Employee
                {
                    Id = Guid.Parse("e9999999-9999-9999-9999-999999999999"),
                    TenantId = DataSeeder.DefaultTenantId,
                    EmployeeCode = "EMP009",
                    FirstName = "Antonio",
                    LastName = "Andrade",
                    Email = "antonio.andrade@rascor.com",
                    JobTitle = "General Operative",
                    Department = null,
                    IsActive = true,
                    StartDate = null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                },
                new Employee
                {
                    Id = Guid.Parse("a1010101-0101-0101-0101-010101010101"),
                    TenantId = DataSeeder.DefaultTenantId,
                    EmployeeCode = "EMP010",
                    FirstName = "Sean",
                    LastName = "Alegra",
                    Email = "sean.alegra@rascor.com",
                    JobTitle = "General Operative",
                    Department = "Construction",
                    IsActive = true,
                    StartDate = null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                },
                new Employee
                {
                    Id = Guid.Parse("a2020202-0202-0202-0202-020202020202"),
                    TenantId = DataSeeder.DefaultTenantId,
                    EmployeeCode = "EMP011",
                    FirstName = "Damian",
                    LastName = "Whelan",
                    Email = "damian.whelan@rascor.com",
                    JobTitle = "General Operative",
                    Department = "Construction",
                    IsActive = true,
                    StartDate = null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                },
                new Employee
                {
                    Id = Guid.Parse("a3030303-0303-0303-0303-030303030303"),
                    TenantId = DataSeeder.DefaultTenantId,
                    EmployeeCode = "EMP012",
                    FirstName = "Jakub",
                    LastName = "Waszkowski",
                    Email = "jakub.waszkowski@rascor.com",
                    JobTitle = "General Operative",
                    Department = "Construction",
                    IsActive = true,
                    StartDate = null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                },
                new Employee
                {
                    Id = Guid.Parse("a4040404-0404-0404-0404-040404040404"),
                    TenantId = DataSeeder.DefaultTenantId,
                    EmployeeCode = "EMP013",
                    FirstName = "Eduardo",
                    LastName = "Rodrigues",
                    Email = "eduardo.rodrigues@rascor.com",
                    JobTitle = "General Operative",
                    Department = "Construction",
                    IsActive = true,
                    StartDate = null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                },
                new Employee
                {
                    Id = Guid.Parse("a5050505-0505-0505-0505-050505050505"),
                    TenantId = DataSeeder.DefaultTenantId,
                    EmployeeCode = "EMP014",
                    FirstName = "Grant",
                    LastName = "Edgar",
                    Email = "grant.edgar@rascor.com",
                    JobTitle = "General Operative",
                    Department = "Construction",
                    IsActive = true,
                    StartDate = null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                },
                new Employee
                {
                    Id = Guid.Parse("a6060606-0606-0606-0606-060606060606"),
                    TenantId = DataSeeder.DefaultTenantId,
                    EmployeeCode = "EMP015",
                    FirstName = "Luke",
                    LastName = "Buls",
                    Email = "luke.buls.rascor@gmail.com",
                    JobTitle = "General Operative",
                    Department = "Construction",
                    IsActive = true,
                    StartDate = null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                },
                new Employee
                {
                    Id = Guid.Parse("a7070707-0707-0707-0707-070707070707"),
                    TenantId = DataSeeder.DefaultTenantId,
                    EmployeeCode = "EMP016",
                    FirstName = "Edi",
                    LastName = "Ruggeri",
                    Email = "edi.ruggeri@rascor.com",
                    JobTitle = "General Operative",
                    Department = "Construction",
                    IsActive = true,
                    StartDate = null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                },
                new Employee
                {
                    Id = Guid.Parse("a8080808-0808-0808-0808-080808080808"),
                    TenantId = DataSeeder.DefaultTenantId,
                    EmployeeCode = "EMP017",
                    FirstName = "Donal",
                    LastName = "Scannell",
                    Email = "quantumbuildrascor@gmail.com",
                    JobTitle = "General Operative",
                    Department = "Construction",
                    IsActive = true,
                    StartDate = null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                },
                new Employee
                {
                    Id = Guid.Parse("a9090909-0909-0909-0909-090909090909"),
                    TenantId = DataSeeder.DefaultTenantId,
                    EmployeeCode = "EMP018",
                    FirstName = "Eddie",
                    LastName = "Heffernan",
                    Email = "eddieheffernan@gmail.com",
                    JobTitle = "General Operative",
                    Department = "Construction",
                    IsActive = true,
                    StartDate = null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                },
                new Employee
                {
                    Id = Guid.Parse("b1010101-0101-0101-0101-010101010101"),
                    TenantId = DataSeeder.DefaultTenantId,
                    EmployeeCode = "EMP019",
                    FirstName = "Donal",
                    LastName = "Scannell",
                    Email = "donal@quantumbuild.ai",
                    JobTitle = "General Operative",
                    Department = "Construction",
                    IsActive = true,
                    StartDate = null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                }
            };

            // Assign GeoTrackerIDs to employees based on email mapping
            foreach (var employee in employeesToCreate)
            {
                if (employee.Email != null && geoTrackerMappings.TryGetValue(employee.Email, out var geoTrackerId))
                {
                    employee.SetGeoTrackerID(geoTrackerId);
                }
            }

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
