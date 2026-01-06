using FluentValidation;
using Rascor.Modules.StockManagement.Application.Features.Products.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.Products;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty()
            .WithMessage("Product code is required")
            .MaximumLength(50)
            .WithMessage("Product code must not exceed 50 characters");

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(200)
            .WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Category is required");

        RuleFor(x => x.BaseRate)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Base rate must be greater than or equal to 0");

        RuleFor(x => x.UnitType)
            .NotEmpty()
            .WithMessage("Unit type is required")
            .MaximumLength(50)
            .WithMessage("Unit type must not exceed 50 characters");

        RuleFor(x => x.ReorderLevel)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Reorder level must be a non-negative number");

        RuleFor(x => x.ReorderQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Reorder quantity must be a non-negative number");

        RuleFor(x => x.LeadTimeDays)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Lead time days must be a non-negative number");
    }
}
