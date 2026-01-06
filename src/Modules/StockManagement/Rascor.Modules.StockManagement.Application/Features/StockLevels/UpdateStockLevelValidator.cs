using FluentValidation;
using Rascor.Modules.StockManagement.Application.Features.StockLevels.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.StockLevels;

public class UpdateStockLevelValidator : AbstractValidator<UpdateStockLevelDto>
{
    public UpdateStockLevelValidator()
    {
        RuleFor(x => x.QuantityOnHand)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantity on hand must be greater than or equal to 0");

        RuleFor(x => x.QuantityReserved)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantity reserved must be greater than or equal to 0");

        RuleFor(x => x.QuantityOnOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantity on order must be greater than or equal to 0");

        RuleFor(x => x.BinLocation)
            .MaximumLength(50)
            .WithMessage("Bin location must not exceed 50 characters");
    }
}
