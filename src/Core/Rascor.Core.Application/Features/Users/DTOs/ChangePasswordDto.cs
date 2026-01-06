namespace Rascor.Core.Application.Features.Users.DTOs;

public record ChangePasswordDto(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);
