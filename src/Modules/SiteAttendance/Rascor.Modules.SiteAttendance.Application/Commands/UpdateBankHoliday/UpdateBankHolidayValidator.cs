using FluentValidation;

namespace Rascor.Modules.SiteAttendance.Application.Commands.UpdateBankHoliday;

public class UpdateBankHolidayValidator : AbstractValidator<UpdateBankHolidayCommand>
{
    public UpdateBankHolidayValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Date is required.");

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Name must not exceed 100 characters.");
    }
}
