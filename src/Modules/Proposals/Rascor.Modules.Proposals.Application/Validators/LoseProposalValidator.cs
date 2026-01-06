using FluentValidation;
using Rascor.Modules.Proposals.Application.DTOs;

namespace Rascor.Modules.Proposals.Application.Validators;

public class LoseProposalValidator : AbstractValidator<LoseProposalDto>
{
    public LoseProposalValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Loss reason is required")
            .MaximumLength(1000)
            .WithMessage("Reason must not exceed 1000 characters");

        RuleFor(x => x.LostDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .When(x => x.LostDate.HasValue)
            .WithMessage("Lost date cannot be in the future");
    }
}
