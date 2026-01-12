using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Domain.Entities;
using Rascor.Core.Infrastructure.Services;

namespace Rascor.Core.Infrastructure.Identity;

/// <summary>
/// Extension methods for configuring Identity and JWT authentication
/// </summary>
public static class IdentityServiceExtensions
{
    /// <summary>
    /// Add Identity services with JWT authentication
    /// </summary>
    public static IServiceCollection AddIdentityServices<TContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TContext : DbContext
    {
        // Configure JWT settings
        var jwtSettings = new JwtSettings();
        configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Configure Identity
        services.AddIdentity<User, Role>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            // User settings
            options.User.RequireUniqueEmail = true;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<TContext>()
        .AddDefaultTokenProviders();

        // Configure JWT authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false; // Set to true in production
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.Zero // Remove default 5 min clock skew
            };
        });

        // Register auth service
        services.AddScoped<IAuthService, AuthService>();

        // Register email service
        services.AddScoped<IEmailService, EmailService>();

        // Register permission authorization handler
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }

    /// <summary>
    /// Add permission-based authorization policies
    /// </summary>
    public static IServiceCollection AddPermissionPolicies(
        this IServiceCollection services,
        IEnumerable<string> permissions)
    {
        services.AddAuthorization(options =>
        {
            foreach (var permission in permissions)
            {
                options.AddPolicy(permission, policy =>
                    policy.Requirements.Add(new PermissionRequirement(permission)));
            }
        });

        return services;
    }
}
