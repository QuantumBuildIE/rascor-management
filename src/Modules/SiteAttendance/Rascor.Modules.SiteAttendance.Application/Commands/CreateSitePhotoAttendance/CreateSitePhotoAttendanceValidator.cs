using FluentValidation;

namespace Rascor.Modules.SiteAttendance.Application.Commands.CreateSitePhotoAttendance;

public class CreateSitePhotoAttendanceValidator : AbstractValidator<CreateSitePhotoAttendanceCommand>
{
    public CreateSitePhotoAttendanceValidator()
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

        RuleFor(x => x.EventDate)
            .NotEmpty()
            .WithMessage("EventDate is required.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("EventDate cannot be in the future.");

        RuleFor(x => x.WeatherConditions)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.WeatherConditions))
            .WithMessage("WeatherConditions must not exceed 200 characters.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048)
            .When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("ImageUrl must not exceed 2048 characters.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue)
            .WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue)
            .WithMessage("Longitude must be between -180 and 180.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes must not exceed 1000 characters.");
    }
}
