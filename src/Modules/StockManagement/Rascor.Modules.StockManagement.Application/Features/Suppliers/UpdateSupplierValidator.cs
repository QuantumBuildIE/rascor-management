using FluentValidation;
using Rascor.Modules.StockManagement.Application.Features.Suppliers.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.Suppliers;

public class UpdateSupplierValidator : AbstractValidator<UpdateSupplierDto>
{
    public UpdateSupplierValidator()
    {
        RuleFor(x => x.SupplierCode)
            .NotEmpty()
            .WithMessage("Supplier code is required")
            .MaximumLength(50)
            .WithMessage("Supplier code must not exceed 50 characters");

        RuleFor(x => x.SupplierName)
            .NotEmpty()
            .WithMessage("Supplier name is required")
            .MaximumLength(200)
            .WithMessage("Supplier name must not exceed 200 characters");

        RuleFor(x => x.ContactName)
            .MaximumLength(100)
            .WithMessage("Contact name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactName));

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MaximumLength(100)
            .WithMessage("Email must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(50)
            .WithMessage("Phone must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("Address must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.PaymentTerms)
            .MaximumLength(100)
            .WithMessage("Payment terms must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.PaymentTerms));
    }
}
