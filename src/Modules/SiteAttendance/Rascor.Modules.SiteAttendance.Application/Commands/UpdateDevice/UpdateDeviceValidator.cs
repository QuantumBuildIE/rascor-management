using FluentValidation;

namespace Rascor.Modules.SiteAttendance.Application.Commands.UpdateDevice;

public class UpdateDeviceValidator : AbstractValidator<UpdateDeviceCommand>
{
    public UpdateDeviceValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");

        RuleFor(x => x.DeviceName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.DeviceName))
            .WithMessage("DeviceName must not exceed 100 characters.");

        RuleFor(x => x.PushToken)
            .MaximumLength(512)
            .When(x => !string.IsNullOrEmpty(x.PushToken))
            .WithMessage("PushToken must not exceed 512 characters.");
    }
}
