using FluentValidation;
using Rascor.Modules.StockManagement.Application.Features.Stocktakes.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.Stocktakes;

public class CreateStocktakeValidator : AbstractValidator<CreateStocktakeDto>
{
    public CreateStocktakeValidator()
    {
        RuleFor(x => x.LocationId)
            .NotEmpty()
            .WithMessage("Location is required");

        RuleFor(x => x.CountedBy)
            .NotEmpty()
            .WithMessage("Counted by is required")
            .MaximumLength(100)
            .WithMessage("Counted by must not exceed 100 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes must not exceed 1000 characters");
    }
}
