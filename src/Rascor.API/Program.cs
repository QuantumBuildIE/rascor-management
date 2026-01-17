using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Rascor.Core.Application;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Infrastructure.Data;
using Rascor.Core.Infrastructure.Identity;
using Rascor.Core.Infrastructure.Persistence;
using Rascor.Core.Infrastructure.Repositories;
using Rascor.Core.Infrastructure.Services;
using Rascor.Modules.StockManagement.Application;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Modules.Proposals.Application;
using Rascor.Modules.Proposals.Application.Common.Interfaces;
using Rascor.Modules.SiteAttendance.Infrastructure;
using Rascor.Modules.SiteAttendance.Infrastructure.Jobs;
using Rascor.Modules.SiteAttendance.Infrastructure.Persistence;
using Rascor.Modules.ToolboxTalks.Application;
using Rascor.Modules.ToolboxTalks.Application.Common.Interfaces;
using Rascor.Modules.ToolboxTalks.Infrastructure;
using Rascor.Modules.ToolboxTalks.Infrastructure.Jobs;
using Rascor.Modules.ToolboxTalks.Infrastructure.Persistence.Seed;
using Rascor.Modules.Rams.Application;
using Rascor.Modules.Rams.Infrastructure;
using Rascor.Modules.Rams.Infrastructure.Jobs;
using Rascor.Modules.Rams.Infrastructure.Persistence.Seed;
using Rascor.Modules.ToolboxTalks.Infrastructure.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add CORS for frontend development
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://rascorweb-production.up.railway.app"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure PostgreSQL database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register IStockManagementDbContext
builder.Services.AddScoped<IStockManagementDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

// Register ICoreDbContext
builder.Services.AddScoped<ICoreDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

// Register IProposalsDbContext
builder.Services.AddScoped<IProposalsDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

// Register IToolboxTalksDbContext
builder.Services.AddScoped<IToolboxTalksDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

// Register IRamsDbContext
builder.Services.AddScoped<Rascor.Modules.Rams.Application.Common.Interfaces.IRamsDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

// Register SiteAttendance module (separate DbContext with own schema)
builder.Services.AddSiteAttendanceInfrastructure(builder.Configuration);

// Register ToolboxTalks module services
builder.Services.AddToolboxTalksInfrastructure(builder.Configuration);

// Register RAMS module services
builder.Services.AddRamsInfrastructure(builder.Configuration);

// Register DbContext (for DataSeeder)
builder.Services.AddScoped<DbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

// Add Identity services with JWT authentication
builder.Services.AddIdentityServices<ApplicationDbContext>(builder.Configuration);

// Add permission-based authorization policies
builder.Services.AddPermissionPolicies(Permissions.GetAll());

// Register Application layer services
builder.Services.AddCoreApplication();
builder.Services.AddApplication();
builder.Services.AddProposalsApplication();
builder.Services.AddToolboxTalksApplication();
builder.Services.AddRamsApplication();

// Register HttpContextAccessor for accessing current user from JWT
builder.Services.AddHttpContextAccessor();

// Register HttpClient for Claude API
builder.Services.AddHttpClient("ClaudeApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register Infrastructure services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// Register background jobs
builder.Services.AddScoped<DailyAttendanceProcessorJob>();
builder.Services.AddScoped<ProcessToolboxTalkSchedulesJob>();
builder.Services.AddScoped<SendToolboxTalkRemindersJob>();
builder.Services.AddScoped<UpdateOverdueToolboxTalksJob>();
builder.Services.AddScoped<RamsDailyDigestJob>();

// Add Hangfire with PostgreSQL storage
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options
        .UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();

// Add controllers with JSON options for enum string conversion
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add Swagger/OpenAPI documentation with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RASCOR Management System API",
        Version = "v1",
        Description = "API for the RASCOR Management System"
    });

    // Add JWT authentication support in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your JWT token. The 'Bearer ' prefix will be added automatically."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add health checks
var healthChecksBuilder = builder.Services.AddHealthChecks();

// Only add database health check if connection string is available (skipped in testing environment)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    healthChecksBuilder.AddNpgSql(connectionString, name: "database");
}

// Add SignalR for real-time subtitle processing progress updates
builder.Services.AddSignalR();

var app = builder.Build();

// Apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
        
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
                pendingMigrations.Count,
                string.Join(", ", pendingMigrations));
            
            await context.Database.MigrateAsync();
            
            logger.LogInformation("✓ Database migrations applied successfully");
        }
        else
        {
            logger.LogInformation("✓ Database schema is up to date");
        }
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Failed to apply database migrations");
        throw;
    }
}

// Seed database with initial data
await DataSeeder.SeedAsync(app.Services);

// Seed Site Attendance module data (separate DbContext)
// await SeedSiteAttendanceDataAsync(app.Services);

// Seed Toolbox Talks module data
await SeedToolboxTalksDataAsync(app.Services);

// Seed RAMS module library data
await SeedRamsLibraryDataAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "RASCOR Management System API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

// Enable CORS for development
app.UseCors("Development");

// Enable static files (for product images)
app.UseStaticFiles();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR hubs
app.MapHub<SubtitleProcessingHub>("/hubs/subtitle-processing");

// Map health check endpoint
app.MapHealthChecks("/health");

// Configure Hangfire dashboard (only in development for security)
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}

// Register recurring jobs using DI-based approach (required for production)
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    var irelandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

    recurringJobManager.AddOrUpdate<DailyAttendanceProcessorJob>(
        "daily-attendance-processor",
        job => job.ExecuteAsync(CancellationToken.None),
        "0 1 * * *", // Run at 1:00 AM daily
        new RecurringJobOptions { TimeZone = irelandTimeZone });

    // Toolbox Talks background jobs
    recurringJobManager.AddOrUpdate<ProcessToolboxTalkSchedulesJob>(
        "process-toolbox-talk-schedules",
        job => job.ExecuteAsync(CancellationToken.None),
        "30 6 * * *", // Run at 6:30 AM daily
        new RecurringJobOptions { TimeZone = irelandTimeZone });

    recurringJobManager.AddOrUpdate<SendToolboxTalkRemindersJob>(
        "send-toolbox-talk-reminders",
        job => job.ExecuteAsync(CancellationToken.None),
        "0 8 * * *", // Run at 8:00 AM daily
        new RecurringJobOptions { TimeZone = irelandTimeZone });

    recurringJobManager.AddOrUpdate<UpdateOverdueToolboxTalksJob>(
        "update-overdue-toolbox-talks",
        job => job.ExecuteAsync(CancellationToken.None),
        "0 * * * *"); // Run every hour

    // RAMS background jobs
    recurringJobManager.AddOrUpdate<RamsDailyDigestJob>(
        "rams-daily-digest",
        job => job.ExecuteAsync(CancellationToken.None),
        "0 8 * * 1-5", // Run at 8:00 AM on weekdays (Monday-Friday)
        new RecurringJobOptions { TimeZone = irelandTimeZone });
}

app.Run();

/// <summary>
/// Seeds Site Attendance module-specific data using SiteAttendanceDbContext
/// </summary>
static async Task SeedSiteAttendanceDataAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<SiteAttendanceDbContext>();
        await SiteAttendanceDataSeeder.SeedAsync(context, logger);
        logger.LogInformation("Site Attendance module seeding completed");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error seeding Site Attendance module data");
        throw;
    }
}

/// <summary>
/// Seeds Toolbox Talks module data using the main ApplicationDbContext
/// </summary>
static async Task SeedToolboxTalksDataAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<DbContext>();
        await ToolboxTalksSeedData.SeedAsync(context, logger);
        logger.LogInformation("Toolbox Talks module seeding completed");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error seeding Toolbox Talks module data");
        throw;
    }
}

/// <summary>
/// Seeds RAMS module library data (hazards, controls, legislation, SOPs)
/// </summary>
static async Task SeedRamsLibraryDataAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<DbContext>();
        await RamsLibrarySeedData.SeedAsync(context, logger);
        logger.LogInformation("RAMS module library seeding completed");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error seeding RAMS module library data");
        throw;
    }
}

// Make the Program class public so integration tests can access it
public partial class Program { }