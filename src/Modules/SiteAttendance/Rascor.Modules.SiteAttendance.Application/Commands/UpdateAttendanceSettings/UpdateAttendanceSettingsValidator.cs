using FluentValidation;

namespace Rascor.Modules.SiteAttendance.Application.Commands.UpdateAttendanceSettings;

public class UpdateAttendanceSettingsValidator : AbstractValidator<UpdateAttendanceSettingsCommand>
{
    public UpdateAttendanceSettingsValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");

        RuleFor(x => x.ExpectedHoursPerDay)
            .InclusiveBetween(0.5m, 24m)
            .WithMessage("ExpectedHoursPerDay must be between 0.5 and 24.");

        RuleFor(x => x.WorkStartTime)
            .NotEmpty()
            .WithMessage("WorkStartTime is required.");

        RuleFor(x => x.LateThresholdMinutes)
            .InclusiveBetween(0, 480)
            .WithMessage("LateThresholdMinutes must be between 0 and 480 (8 hours).");

        RuleFor(x => x.GeofenceRadiusMeters)
            .InclusiveBetween(10, 10000)
            .WithMessage("GeofenceRadiusMeters must be between 10 and 10000.");

        RuleFor(x => x.NoiseThresholdMeters)
            .InclusiveBetween(10, 10000)
            .WithMessage("NoiseThresholdMeters must be between 10 and 10000.");

        RuleFor(x => x.SpaGracePeriodMinutes)
            .InclusiveBetween(0, 60)
            .WithMessage("SpaGracePeriodMinutes must be between 0 and 60.");

        RuleFor(x => x.NotificationTitle)
            .NotEmpty()
            .WithMessage("NotificationTitle is required.")
            .MaximumLength(100)
            .WithMessage("NotificationTitle must not exceed 100 characters.");

        RuleFor(x => x.NotificationMessage)
            .NotEmpty()
            .WithMessage("NotificationMessage is required.")
            .MaximumLength(500)
            .WithMessage("NotificationMessage must not exceed 500 characters.");
    }
}
