using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rascor.Core.Application.Features.Users.DTOs;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Core.Domain.Entities;

namespace Rascor.Core.Application.Features.Users;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ICurrentUserService _currentUserService;

    public UserService(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<UserDto>>> GetAllAsync()
    {
        try
        {
            var tenantId = _currentUserService.TenantId;

            var users = await _userManager.Users
                .Where(u => u.TenantId == tenantId)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new UserDto(
                    u.Id,
                    u.Email!,
                    u.FirstName,
                    u.LastName,
                    u.FirstName + " " + u.LastName,
                    u.TenantId,
                    u.IsActive,
                    u.UserRoles.Select(ur => new UserRoleDto(ur.RoleId, ur.Role.Name!)).ToList(),
                    u.CreatedAt
                ))
                .ToListAsync();

            return Result.Ok(users);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<UserDto>>($"Error retrieving users: {ex.Message}");
        }
    }

    public async Task<Result<PaginatedList<UserDto>>> GetPaginatedAsync(GetUsersQueryDto query)
    {
        try
        {
            var tenantId = _currentUserService.TenantId;

            var usersQuery = _userManager.Users
                .Where(u => u.TenantId == tenantId)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var searchLower = query.Search.ToLower();
                usersQuery = usersQuery.Where(u =>
                    u.Email!.ToLower().Contains(searchLower) ||
                    u.FirstName.ToLower().Contains(searchLower) ||
                    u.LastName.ToLower().Contains(searchLower) ||
                    (u.FirstName + " " + u.LastName).ToLower().Contains(searchLower)
                );
            }

            // Apply sorting
            usersQuery = ApplySorting(usersQuery, query.SortColumn, query.SortDirection);

            // Get total count before pagination
            var totalCount = await usersQuery.CountAsync();

            // Apply pagination
            var users = await usersQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(u => new UserDto(
                    u.Id,
                    u.Email!,
                    u.FirstName,
                    u.LastName,
                    u.FirstName + " " + u.LastName,
                    u.TenantId,
                    u.IsActive,
                    u.UserRoles.Select(ur => new UserRoleDto(ur.RoleId, ur.Role.Name!)).ToList(),
                    u.CreatedAt
                ))
                .ToListAsync();

            var result = new PaginatedList<UserDto>(users, totalCount, query.PageNumber, query.PageSize);
            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            return Result.Fail<PaginatedList<UserDto>>($"Error retrieving users: {ex.Message}");
        }
    }

    private static IQueryable<User> ApplySorting(IQueryable<User> query, string? sortColumn, string? sortDirection)
    {
        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortColumn?.ToLower() switch
        {
            "email" => isDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "firstname" => isDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            "lastname" => isDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
            "fullname" or "name" => isDescending
                ? query.OrderByDescending(u => u.FirstName + " " + u.LastName)
                : query.OrderBy(u => u.FirstName + " " + u.LastName),
            "isactive" => isDescending ? query.OrderByDescending(u => u.IsActive) : query.OrderBy(u => u.IsActive),
            "createdat" => isDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
        };
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var tenantId = _currentUserService.TenantId;

            var user = await _userManager.Users
                .Where(u => u.Id == id && u.TenantId == tenantId)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Select(u => new UserDto(
                    u.Id,
                    u.Email!,
                    u.FirstName,
                    u.LastName,
                    u.FirstName + " " + u.LastName,
                    u.TenantId,
                    u.IsActive,
                    u.UserRoles.Select(ur => new UserRoleDto(ur.RoleId, ur.Role.Name!)).ToList(),
                    u.CreatedAt
                ))
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Result.Fail<UserDto>($"User with ID {id} not found");
            }

            return Result.Ok(user);
        }
        catch (Exception ex)
        {
            return Result.Fail<UserDto>($"Error retrieving user: {ex.Message}");
        }
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserDto dto)
    {
        try
        {
            var tenantId = _currentUserService.TenantId;

            // Check for existing email within the same tenant
            var existingUser = await _userManager.Users
                .Where(u => u.TenantId == tenantId && u.NormalizedEmail == dto.Email.ToUpperInvariant())
                .FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return Result.Fail<UserDto>($"A user with email '{dto.Email}' already exists");
            }

            // Validate roles exist
            var roles = await _roleManager.Roles
                .Where(r => dto.RoleIds.Contains(r.Id))
                .ToListAsync();

            if (roles.Count != dto.RoleIds.Count)
            {
                return Result.Fail<UserDto>("One or more role IDs are invalid");
            }

            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                TenantId = tenantId,
                IsActive = dto.IsActive,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUserService.UserId
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                return Result.Fail<UserDto>(errors);
            }

            // Add user to roles
            foreach (var role in roles)
            {
                await _userManager.AddToRoleAsync(user, role.Name!);
            }

            // Reload user with roles
            var createdUser = await _userManager.Users
                .Where(u => u.Id == user.Id)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstAsync();

            var userDto = new UserDto(
                createdUser.Id,
                createdUser.Email!,
                createdUser.FirstName,
                createdUser.LastName,
                createdUser.FirstName + " " + createdUser.LastName,
                createdUser.TenantId,
                createdUser.IsActive,
                createdUser.UserRoles.Select(ur => new UserRoleDto(ur.RoleId, ur.Role.Name!)).ToList(),
                createdUser.CreatedAt
            );

            return Result.Ok(userDto);
        }
        catch (Exception ex)
        {
            return Result.Fail<UserDto>($"Error creating user: {ex.Message}");
        }
    }

    public async Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        try
        {
            var tenantId = _currentUserService.TenantId;

            var user = await _userManager.Users
                .Where(u => u.Id == id && u.TenantId == tenantId)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Result.Fail<UserDto>($"User with ID {id} not found");
            }

            // Validate roles exist
            var roles = await _roleManager.Roles
                .Where(r => dto.RoleIds.Contains(r.Id))
                .ToListAsync();

            if (roles.Count != dto.RoleIds.Count)
            {
                return Result.Fail<UserDto>("One or more role IDs are invalid");
            }

            // Update user properties
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = _currentUserService.UserId;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description).ToList();
                return Result.Fail<UserDto>(errors);
            }

            // Update roles - remove current roles and add new ones
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            foreach (var role in roles)
            {
                await _userManager.AddToRoleAsync(user, role.Name!);
            }

            // Reload user with roles
            var updatedUser = await _userManager.Users
                .Where(u => u.Id == user.Id)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstAsync();

            var userDto = new UserDto(
                updatedUser.Id,
                updatedUser.Email!,
                updatedUser.FirstName,
                updatedUser.LastName,
                updatedUser.FirstName + " " + updatedUser.LastName,
                updatedUser.TenantId,
                updatedUser.IsActive,
                updatedUser.UserRoles.Select(ur => new UserRoleDto(ur.RoleId, ur.Role.Name!)).ToList(),
                updatedUser.CreatedAt
            );

            return Result.Ok(userDto);
        }
        catch (Exception ex)
        {
            return Result.Fail<UserDto>($"Error updating user: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var tenantId = _currentUserService.TenantId;

            var user = await _userManager.Users
                .Where(u => u.Id == id && u.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Result.Fail($"User with ID {id} not found");
            }

            // Soft delete by deactivating the user
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = _currentUserService.UserId;

            await _userManager.UpdateAsync(user);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error deleting user: {ex.Message}");
        }
    }

    public async Task<Result> ResetPasswordAsync(Guid id, ResetPasswordDto dto)
    {
        try
        {
            var tenantId = _currentUserService.TenantId;

            var user = await _userManager.Users
                .Where(u => u.Id == id && u.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Result.Fail($"User with ID {id} not found");
            }

            // Generate password reset token and reset password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return Result.Fail(errors);
            }

            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = _currentUserService.UserId;
            await _userManager.UpdateAsync(user);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error resetting password: {ex.Message}");
        }
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return Result.Fail("User not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return Result.Fail(errors);
            }

            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = _currentUserService.UserId;
            await _userManager.UpdateAsync(user);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error changing password: {ex.Message}");
        }
    }
}
