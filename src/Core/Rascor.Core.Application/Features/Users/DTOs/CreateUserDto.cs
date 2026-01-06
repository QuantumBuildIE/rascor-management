namespace Rascor.Core.Application.Features.Users.DTOs;

public record CreateUserDto(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string ConfirmPassword,
    bool IsActive,
    List<Guid> RoleIds
);
