using FluentValidation;
using Rascor.Modules.StockManagement.Application.Features.PurchaseOrders.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.PurchaseOrders;

public class CreatePurchaseOrderValidator : AbstractValidator<CreatePurchaseOrderDto>
{
    public CreatePurchaseOrderValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty()
            .WithMessage("Supplier is required");

        RuleFor(x => x.OrderDate)
            .NotEmpty()
            .WithMessage("Order date is required");

        RuleFor(x => x.Lines)
            .NotEmpty()
            .WithMessage("At least one line item is required")
            .Must(lines => lines != null && lines.Count > 0)
            .WithMessage("At least one line item is required");

        RuleForEach(x => x.Lines)
            .SetValidator(new CreatePurchaseOrderLineValidator());

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes must not exceed 1000 characters");
    }
}
