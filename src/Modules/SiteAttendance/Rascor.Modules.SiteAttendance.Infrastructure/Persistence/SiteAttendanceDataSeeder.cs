using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Enums;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Persistence;

/// <summary>
/// Seeds test data for the Site Attendance module (module-specific entities)
/// This seeder handles: AttendanceSettings, BankHolidays, DeviceRegistrations,
/// AttendanceEvents, AttendanceSummaries, and SitePhotoAttendances
/// </summary>
public static class SiteAttendanceDataSeeder
{
    /// <summary>
    /// Default tenant ID for RASCOR (must match Core.Infrastructure.Persistence.DataSeeder)
    /// </summary>
    public static readonly Guid DefaultTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    // GPS coordinates for sites (matching those in Core SiteAttendanceSeeder)
    private static readonly Dictionary<Guid, (decimal Lat, decimal Lon)> SiteCoordinates = new()
    {
        { Guid.Parse("22222222-2222-2222-2222-222222222222"), (53.3498m, -6.2603m) },   // Quantum Build
        { Guid.Parse("33333333-3333-3333-3333-333333333333"), (51.8969m, -8.4863m) },   // South West Gate
        { Guid.Parse("44444444-4444-4444-4444-444444444444"), (53.2707m, -9.0568m) },   // Marmalade Lane
        { Guid.Parse("55555555-5555-5555-5555-555555555555"), (53.3883m, -6.3757m) },   // Rathbourne Crossing
        { Guid.Parse("66666666-6666-6666-6666-666666666666"), (53.3524m, -6.2458m) },   // Castleforbes Prem Inn
        { Guid.Parse("77777777-7777-7777-7777-777777777777"), (52.6638m, -8.6267m) },   // Eden
        { Guid.Parse("88888888-8888-8888-8888-888888888888"), (52.2583m, -7.1119m) }    // Ford
    };

    // Employee IDs for reference (matching those in Core SiteAttendanceSeeder)
    private static readonly Guid[] EmployeeIds =
    [
        Guid.Parse("e1111111-1111-1111-1111-111111111111"), // Michael O'Brien
        Guid.Parse("e2222222-2222-2222-2222-222222222222"), // Sean Murphy
        Guid.Parse("e3333333-3333-3333-3333-333333333333"), // Aoife Walsh
        Guid.Parse("e4444444-4444-4444-4444-444444444444"), // Patrick Kelly
        Guid.Parse("e5555555-5555-5555-5555-555555555555"), // Ciara Ryan
        Guid.Parse("e6666666-6666-6666-6666-666666666666"), // Declan Byrne
        Guid.Parse("e7777777-7777-7777-7777-777777777777"), // Niamh Doyle
        Guid.Parse("e8888888-8888-8888-8888-888888888888")  // Brian McCarthy
    ];

    // Employee to Site mapping (primary sites)
    private static readonly Dictionary<Guid, Guid> EmployeePrimarySites = new()
    {
        { Guid.Parse("e1111111-1111-1111-1111-111111111111"), Guid.Parse("22222222-2222-2222-2222-222222222222") }, // Michael -> Quantum Build
        { Guid.Parse("e2222222-2222-2222-2222-222222222222"), Guid.Parse("22222222-2222-2222-2222-222222222222") }, // Sean -> Quantum Build
        { Guid.Parse("e3333333-3333-3333-3333-333333333333"), Guid.Parse("33333333-3333-3333-3333-333333333333") }, // Aoife -> South West Gate
        { Guid.Parse("e4444444-4444-4444-4444-444444444444"), Guid.Parse("33333333-3333-3333-3333-333333333333") }, // Patrick -> South West Gate
        { Guid.Parse("e5555555-5555-5555-5555-555555555555"), Guid.Parse("44444444-4444-4444-4444-444444444444") }, // Ciara -> Marmalade Lane
        { Guid.Parse("e6666666-6666-6666-6666-666666666666"), Guid.Parse("55555555-5555-5555-5555-555555555555") }, // Declan -> Rathbourne Crossing
        { Guid.Parse("e7777777-7777-7777-7777-777777777777"), Guid.Parse("22222222-2222-2222-2222-222222222222") }, // Niamh -> Quantum Build (safety officer travels)
        { Guid.Parse("e8888888-8888-8888-8888-888888888888"), Guid.Parse("66666666-6666-6666-6666-666666666666") }  // Brian -> Castleforbes Prem Inn
    };

    // Device registration IDs (consistent for FK references)
    private static readonly Guid[] DeviceIds =
    [
        Guid.Parse("d1111111-1111-1111-1111-111111111111"),
        Guid.Parse("d2222222-2222-2222-2222-222222222222"),
        Guid.Parse("d3333333-3333-3333-3333-333333333333"),
        Guid.Parse("d4444444-4444-4444-4444-444444444444")
    ];

    /// <summary>
    /// Seed all Site Attendance module data
    /// </summary>
    public static async Task SeedAsync(SiteAttendanceDbContext context, ILogger logger)
    {
        await SeedAttendanceSettingsAsync(context, logger);
        await SeedBankHolidaysAsync(context, logger);
        await SeedDeviceRegistrationsAsync(context, logger);
        await SeedAttendanceDataAsync(context, logger);
    }

    private static async Task SeedAttendanceSettingsAsync(SiteAttendanceDbContext context, ILogger logger)
    {
        if (await context.AttendanceSettings.IgnoreQueryFilters().AnyAsync(s => s.TenantId == DefaultTenantId))
        {
            logger.LogInformation("Attendance settings already exist, skipping");
            return;
        }

        var settings = AttendanceSettings.CreateDefault(DefaultTenantId);
        await context.AttendanceSettings.AddAsync(settings);
        await context.SaveChangesAsync();
        logger.LogInformation("Created default attendance settings");
    }

    private static async Task SeedBankHolidaysAsync(SiteAttendanceDbContext context, ILogger logger)
    {
        if (await context.BankHolidays.IgnoreQueryFilters().AnyAsync(b => b.TenantId == DefaultTenantId))
        {
            logger.LogInformation("Bank holidays already exist, skipping");
            return;
        }

        // Irish Bank Holidays 2025
        var bankHolidays2025 = new (DateOnly Date, string Name)[]
        {
            (new DateOnly(2025, 1, 1), "New Year's Day"),
            (new DateOnly(2025, 2, 3), "St. Brigid's Day"),
            (new DateOnly(2025, 3, 17), "St. Patrick's Day"),
            (new DateOnly(2025, 4, 21), "Easter Monday"),
            (new DateOnly(2025, 5, 5), "May Bank Holiday"),
            (new DateOnly(2025, 6, 2), "June Bank Holiday"),
            (new DateOnly(2025, 8, 4), "August Bank Holiday"),
            (new DateOnly(2025, 10, 27), "October Bank Holiday"),
            (new DateOnly(2025, 12, 25), "Christmas Day"),
            (new DateOnly(2025, 12, 26), "St. Stephen's Day"),
        };

        var holidays = bankHolidays2025
            .Select(h => BankHoliday.Create(DefaultTenantId, h.Date, h.Name))
            .ToList();

        await context.BankHolidays.AddRangeAsync(holidays);
        await context.SaveChangesAsync();
        logger.LogInformation("Created {Count} bank holidays for 2025", holidays.Count);
    }

    private static async Task SeedDeviceRegistrationsAsync(SiteAttendanceDbContext context, ILogger logger)
    {
        if (await context.DeviceRegistrations.IgnoreQueryFilters().AnyAsync(d => d.TenantId == DefaultTenantId))
        {
            logger.LogInformation("Device registrations already exist, skipping");
            return;
        }

        // Sample devices assigned to employees
        var devices = new (Guid Id, string Identifier, string Name, string Platform, Guid? EmployeeId)[]
        {
            (DeviceIds[0], "device-001-iphone-14pro", "Michael's iPhone 14 Pro", "iOS", EmployeeIds[0]),
            (DeviceIds[1], "device-002-samsung-s23", "Sean's Samsung S23", "Android", EmployeeIds[1]),
            (DeviceIds[2], "device-003-iphone-13", "Aoife's iPhone 13", "iOS", EmployeeIds[2]),
            (DeviceIds[3], "device-004-tablet-android", "Site Tablet 01", "Android", null), // Shared site tablet
        };

        var deviceRegistrations = devices.Select(d =>
        {
            var device = DeviceRegistration.Create(
                DefaultTenantId,
                d.Identifier,
                d.Name,
                d.Platform,
                d.EmployeeId
            );
            // Use reflection to set the Id for consistent test data
            typeof(DeviceRegistration).GetProperty("Id")!.SetValue(device, d.Id);
            device.UpdatePushToken($"test-push-token-{d.Identifier}");
            return device;
        }).ToList();

        await context.DeviceRegistrations.AddRangeAsync(deviceRegistrations);
        await context.SaveChangesAsync();
        logger.LogInformation("Created {Count} device registrations", deviceRegistrations.Count);
    }

    private static async Task SeedAttendanceDataAsync(SiteAttendanceDbContext context, ILogger logger)
    {
        if (await context.AttendanceEvents.IgnoreQueryFilters().AnyAsync(e => e.TenantId == DefaultTenantId))
        {
            logger.LogInformation("Attendance events already exist, skipping");
            return;
        }

        var random = new Random(42); // Seeded for reproducibility
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = today.AddDays(-14);

        // Get bank holidays for filtering
        var bankHolidayDates = await context.BankHolidays
            .IgnoreQueryFilters()
            .Where(b => b.TenantId == DefaultTenantId)
            .Select(b => b.Date)
            .ToHashSetAsync();

        var weatherOptions = new[] { "Sunny", "Cloudy", "Rainy", "Overcast", "Partly Cloudy", "Light Drizzle" };

        var events = new List<AttendanceEvent>();
        var summaries = new List<AttendanceSummary>();
        var spaRecords = new List<SitePhotoAttendance>();

        // Generate attendance data for 14 days
        for (var date = startDate; date <= today; date = date.AddDays(1))
        {
            // Skip weekends
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            // Skip bank holidays
            if (bankHolidayDates.Contains(date))
                continue;

            foreach (var employeeId in EmployeeIds)
            {
                // 5% chance of absence
                if (random.NextDouble() < 0.05)
                    continue;

                // Get the employee's primary site
                var siteId = EmployeePrimarySites[employeeId];
                var (siteLat, siteLon) = SiteCoordinates[siteId];

                // Determine which device (if any) - 60% chance of having a device
                Guid? deviceId = null;
                if (random.NextDouble() < 0.60)
                {
                    var deviceIndex = Array.IndexOf(EmployeeIds, employeeId);
                    if (deviceIndex < DeviceIds.Length)
                        deviceId = DeviceIds[deviceIndex];
                }

                // Generate entry time (7:30 - 8:45)
                var entryMinuteOffset = random.Next(0, 75); // 0-74 minutes after 7:30
                var entryHour = 7 + (30 + entryMinuteOffset) / 60;
                var entryMinute = (30 + entryMinuteOffset) % 60;
                var entryTime = new DateTime(date.Year, date.Month, date.Day, entryHour, entryMinute, random.Next(0, 60), DateTimeKind.Utc);

                // Determine work duration based on distribution
                int workMinutes;
                var durationRoll = random.NextDouble();
                if (durationRoll < 0.10) // 10% short day (4-6 hours)
                    workMinutes = 240 + random.Next(0, 120);
                else if (durationRoll < 0.15) // 5% long day (9-11 hours)
                    workMinutes = 540 + random.Next(0, 120);
                else // 85% normal day (7-8.5 hours)
                    workMinutes = 420 + random.Next(0, 90);

                var exitTime = entryTime.AddMinutes(workMinutes);

                // Add small GPS variation (within ~50 meters)
                var latVariation = (decimal)(random.NextDouble() * 0.0005 - 0.00025);
                var lonVariation = (decimal)(random.NextDouble() * 0.0005 - 0.00025);

                // Determine trigger method - 80% automatic (GPS), 20% manual
                var triggerMethod = random.NextDouble() < 0.80 ? TriggerMethod.Automatic : TriggerMethod.Manual;

                // Create ENTRY event
                var entryEvent = AttendanceEvent.Create(
                    DefaultTenantId,
                    employeeId,
                    siteId,
                    EventType.Enter,
                    entryTime,
                    siteLat + latVariation,
                    siteLon + lonVariation,
                    triggerMethod,
                    deviceId
                );
                entryEvent.MarkAsProcessed();
                events.Add(entryEvent);

                // Create EXIT event
                var exitEvent = AttendanceEvent.Create(
                    DefaultTenantId,
                    employeeId,
                    siteId,
                    EventType.Exit,
                    exitTime,
                    siteLat + latVariation,
                    siteLon + lonVariation,
                    triggerMethod,
                    deviceId
                );
                exitEvent.MarkAsProcessed();
                events.Add(exitEvent);

                // Create corresponding SUMMARY
                var summary = AttendanceSummary.Create(DefaultTenantId, employeeId, siteId, date, 7.5m);
                summary.UpdateFromEvents(entryTime, exitTime, workMinutes, 1, 1);
                summaries.Add(summary);

                // 70% chance of having SPA record
                if (random.NextDouble() < 0.70)
                {
                    var spa = SitePhotoAttendance.Create(
                        DefaultTenantId,
                        employeeId,
                        siteId,
                        date,
                        weatherOptions[random.Next(weatherOptions.Length)],
                        null, // No actual image URL
                        siteLat + latVariation,
                        siteLon + lonVariation,
                        $"Auto-generated test SPA record for {date:yyyy-MM-dd}"
                    );
                    spa.SetDistanceToSite((decimal)(random.NextDouble() * 50)); // Within 50 meters
                    spaRecords.Add(spa);

                    // Mark summary as having SPA
                    summary.MarkHasSpa(true);
                }
            }
        }

        // Save all entities
        await context.AttendanceEvents.AddRangeAsync(events);
        await context.AttendanceSummaries.AddRangeAsync(summaries);
        await context.SitePhotoAttendances.AddRangeAsync(spaRecords);
        await context.SaveChangesAsync();

        logger.LogInformation(
            "Created attendance data: {EventCount} events, {SummaryCount} summaries, {SpaCount} SPA records",
            events.Count, summaries.Count, spaRecords.Count);
    }
}
