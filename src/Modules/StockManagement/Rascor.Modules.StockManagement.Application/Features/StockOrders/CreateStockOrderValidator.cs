using FluentValidation;
using Rascor.Modules.StockManagement.Application.Features.StockOrders.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.StockOrders;

public class CreateStockOrderValidator : AbstractValidator<CreateStockOrderDto>
{
    public CreateStockOrderValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required");

        RuleFor(x => x.SiteName)
            .NotEmpty()
            .WithMessage("Site name is required")
            .MaximumLength(200)
            .WithMessage("Site name must not exceed 200 characters");

        RuleFor(x => x.OrderDate)
            .NotEmpty()
            .WithMessage("Order date is required");

        RuleFor(x => x.RequestedBy)
            .NotEmpty()
            .WithMessage("Requested by is required")
            .MaximumLength(100)
            .WithMessage("Requested by must not exceed 100 characters");

        RuleFor(x => x.Lines)
            .NotEmpty()
            .WithMessage("At least one line item is required")
            .Must(lines => lines != null && lines.Count > 0)
            .WithMessage("At least one line item is required");

        RuleForEach(x => x.Lines)
            .SetValidator(new CreateStockOrderLineValidator());

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes must not exceed 1000 characters");
    }
}
