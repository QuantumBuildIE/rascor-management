using FluentValidation;
using Rascor.Modules.Rams.Application.DTOs;

namespace Rascor.Modules.Rams.Application.Validators;

public class CreateRamsDocumentValidator : AbstractValidator<CreateRamsDocumentDto>
{
    public CreateRamsDocumentValidator()
    {
        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(500).WithMessage("Project name cannot exceed 500 characters");

        RuleFor(x => x.ProjectReference)
            .NotEmpty().WithMessage("Project reference is required")
            .MaximumLength(100).WithMessage("Project reference cannot exceed 100 characters");

        RuleFor(x => x.ProjectType)
            .IsInEnum().WithMessage("Invalid project type");

        RuleFor(x => x.ClientName)
            .MaximumLength(200).WithMessage("Client name cannot exceed 200 characters");

        RuleFor(x => x.SiteAddress)
            .MaximumLength(500).WithMessage("Site address cannot exceed 500 characters");

        RuleFor(x => x.AreaOfActivity)
            .MaximumLength(500).WithMessage("Area of activity cannot exceed 500 characters");

        RuleFor(x => x.ProposedEndDate)
            .GreaterThanOrEqualTo(x => x.ProposedStartDate)
            .When(x => x.ProposedStartDate.HasValue && x.ProposedEndDate.HasValue)
            .WithMessage("End date must be on or after start date");
    }
}
