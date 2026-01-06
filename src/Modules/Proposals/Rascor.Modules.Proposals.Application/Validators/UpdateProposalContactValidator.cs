using FluentValidation;
using Rascor.Modules.Proposals.Application.DTOs;

namespace Rascor.Modules.Proposals.Application.Validators;

public class UpdateProposalContactValidator : AbstractValidator<UpdateProposalContactDto>
{
    public UpdateProposalContactValidator()
    {
        RuleFor(x => x.ContactName)
            .NotEmpty()
            .WithMessage("Contact name is required")
            .MaximumLength(200)
            .WithMessage("Contact name must not exceed 200 characters");

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Role is required")
            .MaximumLength(100)
            .WithMessage("Role must not exceed 100 characters");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email must be a valid email address")
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email must not exceed 200 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone must not exceed 50 characters");
    }
}
