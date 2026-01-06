namespace Rascor.Core.Application.Features.Users.DTOs;

public record UpdateUserDto(
    string FirstName,
    string LastName,
    bool IsActive,
    List<Guid> RoleIds
);
