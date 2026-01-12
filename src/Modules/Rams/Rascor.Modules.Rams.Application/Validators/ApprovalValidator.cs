using FluentValidation;
using Rascor.Modules.Rams.Application.DTOs;

namespace Rascor.Modules.Rams.Application.Validators;

public class ApprovalValidator : AbstractValidator<ApprovalDto>
{
    public ApprovalValidator()
    {
        RuleFor(x => x.Comments)
            .MaximumLength(2000).WithMessage("Comments cannot exceed 2000 characters");
    }
}
