using Rascor.Core.Application.Features.Roles.DTOs;
using Rascor.Core.Application.Models;

namespace Rascor.Core.Application.Features.Roles;

public interface IRoleService
{
    Task<Result<List<RoleDto>>> GetAllAsync();
}
