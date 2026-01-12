using FluentValidation;
using Rascor.Modules.Rams.Application.DTOs;

namespace Rascor.Modules.Rams.Application.Validators;

public class CreateMethodStepValidator : AbstractValidator<CreateMethodStepDto>
{
    public CreateMethodStepValidator()
    {
        RuleFor(x => x.StepTitle)
            .NotEmpty().WithMessage("Step title is required")
            .MaximumLength(200).WithMessage("Step title cannot exceed 200 characters");

        RuleFor(x => x.DetailedProcedure)
            .MaximumLength(4000).WithMessage("Detailed procedure cannot exceed 4000 characters");

        RuleFor(x => x.RequiredPermits)
            .MaximumLength(500).WithMessage("Required permits cannot exceed 500 characters");

        RuleFor(x => x.StepNumber)
            .GreaterThan(0).When(x => x.StepNumber.HasValue)
            .WithMessage("Step number must be greater than 0");
    }
}
