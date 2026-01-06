using FluentValidation;
using Rascor.Modules.Proposals.Application.DTOs;

namespace Rascor.Modules.Proposals.Application.Validators;

public class WinProposalValidator : AbstractValidator<WinProposalDto>
{
    public WinProposalValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Reason))
            .WithMessage("Reason must not exceed 1000 characters");

        RuleFor(x => x.WonDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .When(x => x.WonDate.HasValue)
            .WithMessage("Won date cannot be in the future");
    }
}
