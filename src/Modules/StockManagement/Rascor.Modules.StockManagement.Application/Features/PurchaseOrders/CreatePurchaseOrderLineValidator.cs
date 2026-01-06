using FluentValidation;
using Rascor.Modules.StockManagement.Application.Features.PurchaseOrders.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.PurchaseOrders;

public class CreatePurchaseOrderLineValidator : AbstractValidator<CreatePurchaseOrderLineDto>
{
    public CreatePurchaseOrderLineValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product is required");

        RuleFor(x => x.QuantityOrdered)
            .GreaterThan(0)
            .WithMessage("Quantity ordered must be greater than 0");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Unit price must be greater than or equal to 0");
    }
}
