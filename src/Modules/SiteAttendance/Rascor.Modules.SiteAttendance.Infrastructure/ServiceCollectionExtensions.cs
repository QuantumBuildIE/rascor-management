using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rascor.Modules.SiteAttendance.Application.Mappings;
using Rascor.Modules.SiteAttendance.Application.Services;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;
using Rascor.Modules.SiteAttendance.Infrastructure.Persistence;
using Rascor.Modules.SiteAttendance.Infrastructure.Repositories;
using Rascor.Modules.SiteAttendance.Infrastructure.Services;
using Rascor.Modules.SiteAttendance.Infrastructure.Sync;

namespace Rascor.Modules.SiteAttendance.Infrastructure;

/// <summary>
/// Dependency injection configuration for the Site Attendance Infrastructure layer
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Site Attendance Infrastructure layer services with the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSiteAttendanceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register SiteAttendanceDbContext
        services.AddDbContext<SiteAttendanceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Register MediatR handlers from the Application assembly
        var applicationAssembly = typeof(SiteAttendanceMappingProfile).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

        // Register FluentValidation validators from the Application assembly
        services.AddValidatorsFromAssembly(applicationAssembly);

        // Register AutoMapper profiles from the Application assembly
        services.AddAutoMapper(applicationAssembly);

        // Register repositories
        services.AddScoped<IAttendanceEventRepository, AttendanceEventRepository>();
        services.AddScoped<IAttendanceSummaryRepository, AttendanceSummaryRepository>();
        services.AddScoped<ISitePhotoAttendanceRepository, SitePhotoAttendanceRepository>();
        services.AddScoped<IDeviceRegistrationRepository, DeviceRegistrationRepository>();
        services.AddScoped<IBankHolidayRepository, BankHolidayRepository>();
        services.AddScoped<IAttendanceSettingsRepository, AttendanceSettingsRepository>();
        services.AddScoped<IAttendanceNotificationRepository, AttendanceNotificationRepository>();

        // Register services
        services.AddScoped<ITimeCalculationService, TimeCalculationService>();
        services.AddScoped<IGeofenceService, GeofenceService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAttendanceAnalyticsService, AttendanceAnalyticsService>();

        // Register geofence sync settings and services
        services.Configure<GeofenceSyncSettings>(
            configuration.GetSection(GeofenceSyncSettings.SectionName));

        // Register GeofenceMobileDbContext for reading from mobile geofence database
        var mobileDbConnectionString = configuration.GetConnectionString("GeofenceMobileDb");
        if (!string.IsNullOrEmpty(mobileDbConnectionString) && !mobileDbConnectionString.Contains("<"))
        {
            services.AddDbContext<GeofenceMobileDbContext>(options =>
                options.UseNpgsql(mobileDbConnectionString));
        }

        // Register sync service as both hosted service and scoped service
        services.AddSingleton<GeofenceSyncService>();
        services.AddHostedService(provider => provider.GetRequiredService<GeofenceSyncService>());
        services.AddScoped<IGeofenceSyncService>(provider =>
            provider.GetRequiredService<GeofenceSyncService>());

        return services;
    }
}
