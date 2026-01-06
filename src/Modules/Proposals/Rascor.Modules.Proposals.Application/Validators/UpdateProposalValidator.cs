using FluentValidation;
using Rascor.Modules.Proposals.Application.DTOs;

namespace Rascor.Modules.Proposals.Application.Validators;

public class UpdateProposalValidator : AbstractValidator<UpdateProposalDto>
{
    public UpdateProposalValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Company is required");

        RuleFor(x => x.ProjectName)
            .NotEmpty()
            .WithMessage("Project name is required")
            .MaximumLength(200)
            .WithMessage("Project name must not exceed 200 characters");

        RuleFor(x => x.ProposalDate)
            .NotEmpty()
            .WithMessage("Proposal date is required");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .MaximumLength(3)
            .WithMessage("Currency must not exceed 3 characters");

        RuleFor(x => x.VatRate)
            .GreaterThanOrEqualTo(0)
            .WithMessage("VAT rate must be greater than or equal to 0")
            .LessThanOrEqualTo(100)
            .WithMessage("VAT rate must be less than or equal to 100");

        RuleFor(x => x.DiscountPercent)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Discount percent must be greater than or equal to 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Discount percent must be less than or equal to 100");

        RuleFor(x => x.ProjectAddress)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.ProjectAddress))
            .WithMessage("Project address must not exceed 500 characters");

        RuleFor(x => x.PaymentTerms)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.PaymentTerms))
            .WithMessage("Payment terms must not exceed 500 characters");
    }
}
