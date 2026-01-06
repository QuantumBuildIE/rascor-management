namespace Rascor.Core.Application.Features.Users.DTOs;

public record ResetPasswordDto(
    string NewPassword,
    string ConfirmPassword
);
