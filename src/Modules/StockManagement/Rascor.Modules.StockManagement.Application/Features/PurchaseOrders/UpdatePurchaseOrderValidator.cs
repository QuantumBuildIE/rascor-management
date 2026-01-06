using FluentValidation;
using Rascor.Modules.StockManagement.Application.Features.PurchaseOrders.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.PurchaseOrders;

public class UpdatePurchaseOrderValidator : AbstractValidator<UpdatePurchaseOrderDto>
{
    public UpdatePurchaseOrderValidator()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes must not exceed 1000 characters");
    }
}
