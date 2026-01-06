using FluentValidation;
using Rascor.Modules.StockManagement.Application.Features.GoodsReceipts.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.GoodsReceipts;

public class CreateGoodsReceiptValidator : AbstractValidator<CreateGoodsReceiptDto>
{
    public CreateGoodsReceiptValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty()
            .WithMessage("Supplier is required");

        RuleFor(x => x.LocationId)
            .NotEmpty()
            .WithMessage("Location is required");

        RuleFor(x => x.ReceiptDate)
            .NotEmpty()
            .WithMessage("Receipt date is required");

        RuleFor(x => x.ReceivedBy)
            .NotEmpty()
            .WithMessage("Received by is required")
            .MaximumLength(100)
            .WithMessage("Received by must not exceed 100 characters");

        RuleFor(x => x.Lines)
            .NotEmpty()
            .WithMessage("At least one line item is required")
            .Must(lines => lines != null && lines.Count > 0)
            .WithMessage("At least one line item is required");

        RuleForEach(x => x.Lines)
            .SetValidator(new CreateGoodsReceiptLineValidator());

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes must not exceed 1000 characters");
    }
}
