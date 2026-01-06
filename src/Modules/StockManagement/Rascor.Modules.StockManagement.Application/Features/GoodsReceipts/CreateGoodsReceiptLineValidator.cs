using FluentValidation;
using Rascor.Modules.StockManagement.Application.Features.GoodsReceipts.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.GoodsReceipts;

public class CreateGoodsReceiptLineValidator : AbstractValidator<CreateGoodsReceiptLineDto>
{
    public CreateGoodsReceiptLineValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product is required");

        RuleFor(x => x.QuantityReceived)
            .GreaterThan(0)
            .WithMessage("Quantity received must be greater than 0");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes must not exceed 500 characters");
    }
}
