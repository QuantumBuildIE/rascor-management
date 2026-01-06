using FluentValidation;

namespace Rascor.Modules.SiteAttendance.Application.Commands.RecordAttendanceEvent;

public class RecordAttendanceEventValidator : AbstractValidator<RecordAttendanceEventCommand>
{
    public RecordAttendanceEventValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty()
            .WithMessage("EmployeeId is required.");

        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("SiteId is required.");

        RuleFor(x => x.EventType)
            .IsInEnum()
            .WithMessage("EventType must be a valid value.");

        RuleFor(x => x.Timestamp)
            .NotEmpty()
            .WithMessage("Timestamp is required.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
            .WithMessage("Timestamp cannot be in the future.");

        RuleFor(x => x.TriggerMethod)
            .IsInEnum()
            .WithMessage("TriggerMethod must be a valid value.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue)
            .WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue)
            .WithMessage("Longitude must be between -180 and 180.");

        RuleFor(x => x.DeviceIdentifier)
            .MaximumLength(256)
            .When(x => !string.IsNullOrEmpty(x.DeviceIdentifier))
            .WithMessage("DeviceIdentifier must not exceed 256 characters.");
    }
}
