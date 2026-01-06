using Microsoft.AspNetCore.Authorization;

namespace Rascor.Core.Infrastructure.Identity;

/// <summary>
/// Authorization requirement for permission-based authorization
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
