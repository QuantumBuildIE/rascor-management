using FluentValidation;

namespace Rascor.Modules.SiteAttendance.Application.Commands.UpdateSitePhotoAttendance;

public class UpdateSitePhotoAttendanceValidator : AbstractValidator<UpdateSitePhotoAttendanceCommand>
{
    public UpdateSitePhotoAttendanceValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");

        RuleFor(x => x.WeatherConditions)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.WeatherConditions))
            .WithMessage("WeatherConditions must not exceed 200 characters.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048)
            .When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("ImageUrl must not exceed 2048 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes must not exceed 1000 characters.");
    }
}
