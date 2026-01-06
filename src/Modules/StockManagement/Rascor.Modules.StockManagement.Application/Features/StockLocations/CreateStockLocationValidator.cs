using FluentValidation;
using Rascor.Modules.StockManagement.Application.Features.StockLocations.DTOs;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Application.Features.StockLocations;

public class CreateStockLocationValidator : AbstractValidator<CreateStockLocationDto>
{
    public CreateStockLocationValidator()
    {
        RuleFor(x => x.LocationCode)
            .NotEmpty()
            .WithMessage("Location code is required")
            .MaximumLength(20)
            .WithMessage("Location code must not exceed 20 characters");

        RuleFor(x => x.LocationName)
            .NotEmpty()
            .WithMessage("Location name is required")
            .MaximumLength(100)
            .WithMessage("Location name must not exceed 100 characters");

        RuleFor(x => x.LocationType)
            .NotEmpty()
            .WithMessage("Location type is required")
            .Must(BeValidLocationType)
            .WithMessage("Location type must be one of: Warehouse, SiteStore, VanStock, Transit");

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("Address must not exceed 500 characters");
    }

    private static bool BeValidLocationType(string locationType)
    {
        return Enum.TryParse<LocationType>(locationType, ignoreCase: true, out _);
    }
}
