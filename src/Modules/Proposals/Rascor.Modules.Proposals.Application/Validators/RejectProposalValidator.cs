using FluentValidation;
using Rascor.Modules.Proposals.Application.DTOs;

namespace Rascor.Modules.Proposals.Application.Validators;

public class RejectProposalValidator : AbstractValidator<RejectProposalDto>
{
    public RejectProposalValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Rejection reason is required")
            .MaximumLength(1000)
            .WithMessage("Rejection reason must not exceed 1000 characters");
    }
}
