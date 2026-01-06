using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Rascor.Core.Application;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Infrastructure.Identity;
using Rascor.Core.Infrastructure.Persistence;
using Rascor.Core.Infrastructure.Repositories;
using Rascor.Modules.StockManagement.Application;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Modules.StockManagement.Infrastructure.Data;
using Rascor.Modules.StockManagement.Infrastructure.Services;
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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add CORS for frontend development
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Next.js default
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

// Register SiteAttendance module (separate DbContext with own schema)
builder.Services.AddSiteAttendanceInfrastructure(builder.Configuration);

// Register ToolboxTalks module services
builder.Services.AddToolboxTalksInfrastructure(builder.Configuration);

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

// Register HttpContextAccessor for accessing current user from JWT
builder.Services.AddHttpContextAccessor();

// Register Infrastructure services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// Register background jobs
builder.Services.AddScoped<DailyAttendanceProcessorJob>();
builder.Services.AddScoped<ProcessToolboxTalkSchedulesJob>();
builder.Services.AddScoped<SendToolboxTalkRemindersJob>();
builder.Services.AddScoped<UpdateOverdueToolboxTalksJob>();

// Add Hangfire with PostgreSQL storage
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options
        .UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();

// Add controllers
builder.Services.AddControllers();

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

var app = builder.Build();

// Seed database with initial data
await DataSeeder.SeedAsync(app.Services);

// Seed Site Attendance module data (separate DbContext)
await SeedSiteAttendanceDataAsync(app.Services);

// Seed Toolbox Talks module data
await SeedToolboxTalksDataAsync(app.Services);

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

// Configure Hangfire dashboard (only in development for security)
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}

// Register recurring jobs
RecurringJob.AddOrUpdate<DailyAttendanceProcessorJob>(
    "daily-attendance-processor",
    job => job.ExecuteAsync(CancellationToken.None),
    "0 1 * * *", // Run at 1:00 AM daily
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time") // Ireland time
    });

// Toolbox Talks background jobs
RecurringJob.AddOrUpdate<ProcessToolboxTalkSchedulesJob>(
    "process-toolbox-talk-schedules",
    job => job.ExecuteAsync(CancellationToken.None),
    "30 6 * * *", // Run at 6:30 AM daily
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time") // Ireland time
    });

RecurringJob.AddOrUpdate<SendToolboxTalkRemindersJob>(
    "send-toolbox-talk-reminders",
    job => job.ExecuteAsync(CancellationToken.None),
    "0 8 * * *", // Run at 8:00 AM daily
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time") // Ireland time
    });

RecurringJob.AddOrUpdate<UpdateOverdueToolboxTalksJob>(
    "update-overdue-toolbox-talks",
    job => job.ExecuteAsync(CancellationToken.None),
    "0 * * * *"); // Run every hour

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

// Make the Program class public so integration tests can access it
public partial class Program { }
