using Rascor.Core.Application.Features.Users.DTOs;
using Rascor.Core.Application.Models;

namespace Rascor.Core.Application.Features.Users;

public interface IUserService
{
    Task<Result<List<UserDto>>> GetAllAsync();
    Task<Result<PaginatedList<UserDto>>> GetPaginatedAsync(GetUsersQueryDto query);
    Task<Result<UserDto>> GetByIdAsync(Guid id);
    Task<Result<UserDto>> CreateAsync(CreateUserDto dto);
    Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserDto dto);
    Task<Result> DeleteAsync(Guid id);
    Task<Result> ResetPasswordAsync(Guid id, ResetPasswordDto dto);
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);

    /// <summary>
    /// Sets a user's password using an email verification token.
    /// Used when new users click the "Set Password" link in their welcome email.
    /// </summary>
    Task<Result> SetPasswordWithTokenAsync(SetPasswordWithTokenDto dto);
}
