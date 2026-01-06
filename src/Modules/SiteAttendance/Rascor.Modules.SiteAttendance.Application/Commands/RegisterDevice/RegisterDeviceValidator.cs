using FluentValidation;

namespace Rascor.Modules.SiteAttendance.Application.Commands.RegisterDevice;

public class RegisterDeviceValidator : AbstractValidator<RegisterDeviceCommand>
{
    public RegisterDeviceValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");

        RuleFor(x => x.DeviceIdentifier)
            .NotEmpty()
            .WithMessage("DeviceIdentifier is required.")
            .MaximumLength(256)
            .WithMessage("DeviceIdentifier must not exceed 256 characters.");

        RuleFor(x => x.DeviceName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.DeviceName))
            .WithMessage("DeviceName must not exceed 100 characters.");

        RuleFor(x => x.Platform)
            .Must(x => string.IsNullOrEmpty(x) || x == "iOS" || x == "Android" || x == "Web")
            .WithMessage("Platform must be 'iOS', 'Android', or 'Web'.");

        RuleFor(x => x.PushToken)
            .MaximumLength(512)
            .When(x => !string.IsNullOrEmpty(x.PushToken))
            .WithMessage("PushToken must not exceed 512 characters.");
    }
}
