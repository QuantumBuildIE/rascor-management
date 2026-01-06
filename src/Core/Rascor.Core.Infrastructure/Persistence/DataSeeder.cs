using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rascor.Core.Domain.Entities;
using Rascor.Core.Infrastructure.Identity;

namespace Rascor.Core.Infrastructure.Persistence;

/// <summary>
/// Seeds initial data for the application including permissions, roles, tenants, and users
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// Default tenant ID for RASCOR
    /// </summary>
    public static readonly Guid DefaultTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary>
    /// Seed all initial data
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<object>>();

        try
        {
            var context = services.GetRequiredService<DbContext>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<Role>>();

            await SeedTenantsAsync(context, logger);
            await SeedPermissionsAsync(context, logger);
            await SeedRolesAsync(context, roleManager, logger);
            await SeedRolePermissionsAsync(context, logger);
            await SeedAdminUserAsync(userManager, roleManager, logger);
            await SeedTestUsersAsync(userManager, roleManager, logger);
            await SeedSitesAsync(context, logger);

            // Seed Stock Management test data
            await StockManagementSeeder.SeedAsync(context, logger);

            // Seed Proposals test data
            await ProposalsSeeder.SeedAsync(context, logger);

            // Seed Site Attendance core data (Employees, Site GPS)
            await SiteAttendanceSeeder.SeedAsync(context, logger);

            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static async Task SeedTenantsAsync(DbContext context, ILogger logger)
    {
        var tenants = context.Set<Tenant>();

        if (await tenants.IgnoreQueryFilters().AnyAsync(t => t.Id == DefaultTenantId))
        {
            logger.LogInformation("Default tenant already exists, skipping");
            return;
        }

        var tenant = new Tenant
        {
            Id = DefaultTenantId,
            Name = "RASCOR",
            Code = "RASCOR",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await tenants.AddAsync(tenant);
        await context.SaveChangesAsync();
        logger.LogInformation("Created default tenant: {TenantName}", tenant.Name);
    }

    private static async Task SeedPermissionsAsync(DbContext context, ILogger logger)
    {
        var permissions = context.Set<Permission>();
        var existingPermissions = await permissions
            .IgnoreQueryFilters()
            .Select(p => p.Name)
            .ToListAsync();

        var allPermissions = Permissions.GetAll().ToList();
        var newPermissions = new List<Permission>();

        foreach (var permissionName in allPermissions)
        {
            if (existingPermissions.Contains(permissionName))
                continue;

            var moduleName = Permissions.GetModuleName(permissionName);
            var permission = new Permission
            {
                Id = Guid.NewGuid(),
                Name = permissionName,
                Module = moduleName,
                Description = GetPermissionDescription(permissionName),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            newPermissions.Add(permission);
        }

        if (newPermissions.Count > 0)
        {
            await permissions.AddRangeAsync(newPermissions);
            await context.SaveChangesAsync();
            logger.LogInformation("Created {Count} new permissions", newPermissions.Count);
        }
        else
        {
            logger.LogInformation("All permissions already exist, skipping");
        }
    }

    private static async Task SeedRolesAsync(DbContext context, RoleManager<Role> roleManager, ILogger logger)
    {
        var rolesToCreate = new[]
        {
            new { Name = "Admin", Description = "Full system administrator with all permissions" },
            new { Name = "Finance", Description = "Finance team with view and costing permissions" },
            new { Name = "OfficeStaff", Description = "Office staff with proposals and basic stock access" },
            new { Name = "SiteManager", Description = "Site manager with attendance and stock ordering" },
            new { Name = "WarehouseStaff", Description = "Warehouse staff with stock management permissions" }
        };

        foreach (var roleInfo in rolesToCreate)
        {
            var existingRole = await roleManager.FindByNameAsync(roleInfo.Name);
            if (existingRole != null)
            {
                logger.LogInformation("Role {RoleName} already exists, skipping", roleInfo.Name);
                continue;
            }

            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = roleInfo.Name,
                NormalizedName = roleInfo.Name.ToUpperInvariant(),
                Description = roleInfo.Description,
                IsSystemRole = true,
                IsActive = true,
                TenantId = null, // System-wide roles
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            var result = await roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                logger.LogInformation("Created role: {RoleName}", roleInfo.Name);
            }
            else
            {
                logger.LogWarning("Failed to create role {RoleName}: {Errors}",
                    roleInfo.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private static async Task SeedRolePermissionsAsync(DbContext context, ILogger logger)
    {
        var roles = await context.Set<Role>()
            .Include(r => r.RolePermissions)
            .ToListAsync();

        var allPermissions = await context.Set<Permission>()
            .IgnoreQueryFilters()
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        var rolePermissions = context.Set<RolePermission>();
        var newAssignments = new List<RolePermission>();

        foreach (var role in roles)
        {
            var permissionsForRole = GetPermissionsForRole(role.Name!, allPermissions);
            var existingPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();

            foreach (var permission in permissionsForRole)
            {
                if (existingPermissionIds.Contains(permission.Id))
                    continue;

                newAssignments.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
            }
        }

        if (newAssignments.Count > 0)
        {
            await rolePermissions.AddRangeAsync(newAssignments);
            await context.SaveChangesAsync();
            logger.LogInformation("Created {Count} new role-permission assignments", newAssignments.Count);
        }
        else
        {
            logger.LogInformation("All role permissions already assigned, skipping");
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<User> userManager, RoleManager<Role> roleManager, ILogger logger)
    {
        const string adminEmail = "admin@rascor.ie";
        const string adminPassword = "Admin123!";

        var existingUser = await userManager.FindByEmailAsync(adminEmail);
        if (existingUser != null)
        {
            logger.LogInformation("Admin user already exists, skipping");
            return;
        }

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Administrator",
            TenantId = DefaultTenantId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            logger.LogInformation("Created admin user: {Email}", adminEmail);

            // Assign Admin role
            var adminRole = await roleManager.FindByNameAsync("Admin");
            if (adminRole != null)
            {
                var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Assigned Admin role to user: {Email}", adminEmail);
                }
                else
                {
                    logger.LogWarning("Failed to assign Admin role: {Errors}",
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
        }
        else
        {
            logger.LogWarning("Failed to create admin user: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private static async Task SeedTestUsersAsync(UserManager<User> userManager, RoleManager<Role> roleManager, ILogger logger)
    {
        var testUsers = new[]
        {
            new { Email = "warehouse@rascor.ie", Password = "Warehouse123!", FirstName = "John", LastName = "Warehouse", Role = "WarehouseStaff" },
            new { Email = "sitemanager@rascor.ie", Password = "SiteManager123!", FirstName = "Sarah", LastName = "Site", Role = "SiteManager" },
            new { Email = "office@rascor.ie", Password = "Office123!", FirstName = "Mike", LastName = "Office", Role = "OfficeStaff" },
            new { Email = "finance@rascor.ie", Password = "Finance123!", FirstName = "Emma", LastName = "Finance", Role = "Finance" }
        };

        foreach (var testUser in testUsers)
        {
            var existingUser = await userManager.FindByEmailAsync(testUser.Email);
            if (existingUser != null)
            {
                logger.LogInformation("Test user {Email} already exists, skipping", testUser.Email);
                continue;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = testUser.Email,
                Email = testUser.Email,
                EmailConfirmed = true,
                FirstName = testUser.FirstName,
                LastName = testUser.LastName,
                TenantId = DefaultTenantId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            var result = await userManager.CreateAsync(user, testUser.Password);
            if (result.Succeeded)
            {
                logger.LogInformation("Created test user: {Email}", testUser.Email);

                // Assign role
                var role = await roleManager.FindByNameAsync(testUser.Role);
                if (role != null)
                {
                    var roleResult = await userManager.AddToRoleAsync(user, testUser.Role);
                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation("Assigned {Role} role to user: {Email}", testUser.Role, testUser.Email);
                    }
                    else
                    {
                        logger.LogWarning("Failed to assign {Role} role to {Email}: {Errors}",
                            testUser.Role, testUser.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }
            }
            else
            {
                logger.LogWarning("Failed to create test user {Email}: {Errors}",
                    testUser.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private static async Task SeedSitesAsync(DbContext context, ILogger logger)
    {
        var sites = context.Set<Site>();

        if (await sites.IgnoreQueryFilters().AnyAsync())
        {
            logger.LogInformation("Sites already exist, skipping site seeding");
            return;
        }

        var sitesToCreate = new List<Site>
        {
            new Site
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                TenantId = DefaultTenantId,
                SiteCode = "SITE001",
                SiteName = "Quantum Build",
                Address = "123 Construction Way",
                City = "Dublin",
                PostalCode = "D01 AB12",
                IsActive = true,
                Notes = "Main residential development",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Site
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                TenantId = DefaultTenantId,
                SiteCode = "SITE002",
                SiteName = "South West Gate",
                Address = "456 Gateway Road",
                City = "Cork",
                PostalCode = "T12 CD34",
                IsActive = true,
                Notes = "Commercial development",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Site
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                TenantId = DefaultTenantId,
                SiteCode = "SITE003",
                SiteName = "Marmalade Lane",
                Address = "789 Sweet Street",
                City = "Galway",
                PostalCode = "H91 EF56",
                IsActive = true,
                Notes = "Mixed-use development",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Site
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                TenantId = DefaultTenantId,
                SiteCode = "SITE004",
                SiteName = "Rathbourne Crossing",
                Address = "101 Crossing Boulevard",
                City = "Dublin",
                PostalCode = "D15 GH78",
                IsActive = true,
                Notes = "Infrastructure project",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Site
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                TenantId = DefaultTenantId,
                SiteCode = "SITE005",
                SiteName = "Castleforbes Prem Inn",
                Address = "202 Hotel Drive",
                City = "Dublin",
                PostalCode = "D01 IJ90",
                IsActive = true,
                Notes = "Hotel construction",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Site
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                TenantId = DefaultTenantId,
                SiteCode = "SITE006",
                SiteName = "Eden",
                Address = "303 Garden Avenue",
                City = "Limerick",
                PostalCode = "V94 KL12",
                IsActive = true,
                Notes = "Residential complex",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new Site
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                TenantId = DefaultTenantId,
                SiteCode = "SITE007",
                SiteName = "Ford",
                Address = "404 Motor Lane",
                City = "Waterford",
                PostalCode = "X91 MN34",
                IsActive = true,
                Notes = "Industrial facility",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            }
        };

        await sites.AddRangeAsync(sitesToCreate);
        await context.SaveChangesAsync();
        logger.LogInformation("Created {Count} test sites", sitesToCreate.Count);
    }

    private static IEnumerable<Permission> GetPermissionsForRole(string roleName, List<Permission> allPermissions)
    {
        return roleName switch
        {
            "Admin" => allPermissions, // All permissions

            "Finance" => allPermissions.Where(p =>
                p.Name.EndsWith(".View") ||
                p.Name == Permissions.StockManagement.ViewCostings ||
                p.Name == Permissions.Proposals.ViewCostings ||
                p.Name.StartsWith("Proposals.")),

            "OfficeStaff" => allPermissions.Where(p =>
                p.Name == Permissions.Proposals.View ||
                p.Name == Permissions.Proposals.Create ||
                p.Name == Permissions.Proposals.Edit ||
                p.Name == Permissions.Proposals.Submit ||
                p.Name == Permissions.StockManagement.View ||
                p.Name == Permissions.StockManagement.CreateOrders),

            "SiteManager" => allPermissions.Where(p =>
                p.Name.StartsWith("SiteAttendance.") ||
                p.Name == Permissions.StockManagement.View ||
                p.Name == Permissions.StockManagement.CreateOrders),

            "WarehouseStaff" => allPermissions.Where(p =>
                p.Name.StartsWith("StockManagement.") &&
                p.Name != Permissions.StockManagement.Admin &&
                p.Name != Permissions.StockManagement.ViewCostings),

            _ => Enumerable.Empty<Permission>()
        };
    }

    private static string GetPermissionDescription(string permissionName)
    {
        return permissionName switch
        {
            // Stock Management
            Permissions.StockManagement.View => "View stock management data",
            Permissions.StockManagement.CreateOrders => "Create stock orders",
            Permissions.StockManagement.ApproveOrders => "Approve stock orders",
            Permissions.StockManagement.ViewCostings => "View cost and pricing information",
            Permissions.StockManagement.ManageProducts => "Manage products and categories",
            Permissions.StockManagement.ManageSuppliers => "Manage suppliers",
            Permissions.StockManagement.ReceiveGoods => "Receive goods and create GRNs",
            Permissions.StockManagement.Stocktake => "Perform stocktakes",
            Permissions.StockManagement.Admin => "Full stock management administration",

            // Site Attendance
            Permissions.SiteAttendance.View => "View site attendance records",
            Permissions.SiteAttendance.MarkAttendance => "Mark site attendance",
            Permissions.SiteAttendance.Admin => "Full site attendance administration",

            // Proposals
            Permissions.Proposals.View => "View proposals",
            Permissions.Proposals.Create => "Create proposals",
            Permissions.Proposals.Edit => "Edit proposals",
            Permissions.Proposals.Delete => "Delete proposals",
            Permissions.Proposals.Submit => "Submit proposals for approval",
            Permissions.Proposals.Approve => "Approve proposals",
            Permissions.Proposals.ViewCostings => "View proposal costings and margins",
            Permissions.Proposals.Admin => "Full proposals administration",

            // Core
            Permissions.Core.ManageSites => "Manage sites",
            Permissions.Core.ManageEmployees => "Manage employees",
            Permissions.Core.ManageCompanies => "Manage companies and contacts",
            Permissions.Core.ManageUsers => "Manage user accounts",
            Permissions.Core.ManageRoles => "Manage roles and permissions",
            Permissions.Core.Admin => "Full core system administration",

            _ => $"Permission: {permissionName}"
        };
    }
}
