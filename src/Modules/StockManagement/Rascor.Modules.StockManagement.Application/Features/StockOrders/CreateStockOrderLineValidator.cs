using FluentValidation;
using Rascor.Modules.StockManagement.Application.Features.StockOrders.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.StockOrders;

public class CreateStockOrderLineValidator : AbstractValidator<CreateStockOrderLineDto>
{
    public CreateStockOrderLineValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product is required");

        RuleFor(x => x.QuantityRequested)
            .GreaterThan(0)
            .WithMessage("Quantity requested must be greater than 0");
    }
}
