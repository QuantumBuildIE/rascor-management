using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Rascor.Core.Application.Features.Companies;
using Rascor.Core.Application.Features.Contacts;
using Rascor.Core.Application.Features.Employees;
using Rascor.Core.Application.Features.Roles;
using Rascor.Core.Application.Features.Sites;
using Rascor.Core.Application.Features.Users;

namespace Rascor.Core.Application;

/// <summary>
/// Dependency injection configuration for the Core Application layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Core Application layer services with the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCoreApplication(this IServiceCollection services)
    {
        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Register Core application services
        services.AddScoped<ISiteService, SiteService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();

        return services;
    }
}
