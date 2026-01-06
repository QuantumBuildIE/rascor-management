using FluentValidation;
using Rascor.Modules.Proposals.Application.DTOs;

namespace Rascor.Modules.Proposals.Application.Validators;

public class CreateProposalSectionValidator : AbstractValidator<CreateProposalSectionDto>
{
    public CreateProposalSectionValidator()
    {
        RuleFor(x => x.ProposalId)
            .NotEmpty()
            .WithMessage("Proposal ID is required");

        RuleFor(x => x.SectionName)
            .NotEmpty()
            .WithMessage("Section name is required")
            .MaximumLength(200)
            .WithMessage("Section name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description must not exceed 1000 characters");
    }
}
