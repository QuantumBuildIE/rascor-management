using FluentValidation;

namespace Rascor.Modules.SiteAttendance.Application.Commands.CreateBankHoliday;

public class CreateBankHolidayValidator : AbstractValidator<CreateBankHolidayCommand>
{
    public CreateBankHolidayValidator()
    {
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
