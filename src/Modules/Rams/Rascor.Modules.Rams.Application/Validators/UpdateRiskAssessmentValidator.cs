using FluentValidation;
using Rascor.Modules.Rams.Application.DTOs;

namespace Rascor.Modules.Rams.Application.Validators;

public class UpdateRiskAssessmentValidator : AbstractValidator<UpdateRiskAssessmentDto>
{
    public UpdateRiskAssessmentValidator()
    {
        RuleFor(x => x.TaskActivity)
            .NotEmpty().WithMessage("Task/Activity is required")
            .MaximumLength(500).WithMessage("Task/Activity cannot exceed 500 characters");

        RuleFor(x => x.HazardIdentified)
            .NotEmpty().WithMessage("Hazard identified is required")
            .MaximumLength(500).WithMessage("Hazard identified cannot exceed 500 characters");

        RuleFor(x => x.LocationArea)
            .MaximumLength(200).WithMessage("Location/Area cannot exceed 200 characters");

        RuleFor(x => x.WhoAtRisk)
            .MaximumLength(200).WithMessage("Who at risk cannot exceed 200 characters");

        RuleFor(x => x.InitialLikelihood)
            .InclusiveBetween(1, 5).WithMessage("Initial likelihood must be between 1 and 5");

        RuleFor(x => x.InitialSeverity)
            .InclusiveBetween(1, 5).WithMessage("Initial severity must be between 1 and 5");

        RuleFor(x => x.ResidualLikelihood)
            .InclusiveBetween(1, 5).WithMessage("Residual likelihood must be between 1 and 5");

        RuleFor(x => x.ResidualSeverity)
            .InclusiveBetween(1, 5).WithMessage("Residual severity must be between 1 and 5");

        RuleFor(x => x.ControlMeasures)
            .MaximumLength(4000).WithMessage("Control measures cannot exceed 4000 characters");

        RuleFor(x => x.RelevantLegislation)
            .MaximumLength(2000).WithMessage("Relevant legislation cannot exceed 2000 characters");

        RuleFor(x => x.ReferenceSops)
            .MaximumLength(500).WithMessage("Reference SOPs cannot exceed 500 characters");
    }
}
