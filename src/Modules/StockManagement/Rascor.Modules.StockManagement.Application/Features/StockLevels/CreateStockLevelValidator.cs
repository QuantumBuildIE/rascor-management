using FluentValidation;
using Rascor.Modules.StockManagement.Application.Features.StockLevels.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.StockLevels;

public class CreateStockLevelValidator : AbstractValidator<CreateStockLevelDto>
{
    public CreateStockLevelValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product is required");

        RuleFor(x => x.LocationId)
            .NotEmpty()
            .WithMessage("Location is required");

        RuleFor(x => x.QuantityOnHand)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantity on hand must be greater than or equal to 0");

        RuleFor(x => x.BinLocation)
            .MaximumLength(50)
            .WithMessage("Bin location must not exceed 50 characters");
    }
}
