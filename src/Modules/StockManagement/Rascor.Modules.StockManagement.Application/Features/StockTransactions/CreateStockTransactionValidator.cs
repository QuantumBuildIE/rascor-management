using FluentValidation;
using Rascor.Modules.StockManagement.Application.Features.StockTransactions.DTOs;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Application.Features.StockTransactions;

public class CreateStockTransactionValidator : AbstractValidator<CreateStockTransactionDto>
{
    public CreateStockTransactionValidator()
    {
        RuleFor(x => x.TransactionType)
            .NotEmpty()
            .WithMessage("Transaction type is required")
            .Must(BeValidTransactionType)
            .WithMessage("Transaction type must be one of: GrnReceipt, OrderIssue, AdjustmentIn, AdjustmentOut, TransferIn, TransferOut, StocktakeAdjustment");

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product is required");

        RuleFor(x => x.LocationId)
            .NotEmpty()
            .WithMessage("Location is required");

        RuleFor(x => x.Quantity)
            .NotEqual(0)
            .WithMessage("Quantity cannot be 0");

        RuleFor(x => x.ReferenceType)
            .MaximumLength(50)
            .WithMessage("Reference type must not exceed 50 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes must not exceed 500 characters");
    }

    private static bool BeValidTransactionType(string transactionType)
    {
        return Enum.TryParse<TransactionType>(transactionType, ignoreCase: true, out _);
    }
}
