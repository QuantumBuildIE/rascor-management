using FluentValidation;

namespace Rascor.Modules.SiteAttendance.Application.Commands.DeleteBankHoliday;

public class DeleteBankHolidayValidator : AbstractValidator<DeleteBankHolidayCommand>
{
    public DeleteBankHolidayValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");
    }
}
