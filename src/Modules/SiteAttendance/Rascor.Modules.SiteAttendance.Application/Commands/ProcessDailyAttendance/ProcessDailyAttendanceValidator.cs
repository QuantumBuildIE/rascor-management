using FluentValidation;

namespace Rascor.Modules.SiteAttendance.Application.Commands.ProcessDailyAttendance;

public class ProcessDailyAttendanceValidator : AbstractValidator<ProcessDailyAttendanceCommand>
{
    public ProcessDailyAttendanceValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Date is required.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date cannot be in the future.");
    }
}
