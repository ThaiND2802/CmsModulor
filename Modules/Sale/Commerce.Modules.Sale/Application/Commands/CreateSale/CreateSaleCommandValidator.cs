using FluentValidation;

namespace Commerce.Modules.Sale.Application.Commands.CreateSale;

public sealed class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.CustomerEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.CustomerPhone)
            .MaximumLength(50);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3);

        RuleFor(x => x.ShippingAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0);

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.ProductName).NotEmpty().MaximumLength(500);
                item.RuleFor(x => x.ProductSku).NotEmpty().MaximumLength(100);
                item.RuleFor(x => x.VariantName).MaximumLength(300);
                item.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
                item.RuleFor(x => x.Quantity).GreaterThan(0);
                item.RuleFor(x => x.DiscountAmount)
                    .GreaterThanOrEqualTo(0)
                    .LessThanOrEqualTo(x => x.UnitPrice * x.Quantity);
            });

        RuleFor(x => x)
            .Must(x => x.DiscountAmount <= (x.Items?.Sum(item => (item.UnitPrice * item.Quantity) - item.DiscountAmount) ?? 0))
            .WithMessage("Sale discount must be less than or equal to the subtotal amount.");

        When(x => x.ShippingAddress is not null, () =>
        {
            RuleFor(x => x.ShippingAddress!.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ShippingAddress!.PhoneNumber).NotEmpty().MaximumLength(50);
            RuleFor(x => x.ShippingAddress!.AddressLine1).NotEmpty().MaximumLength(500);
            RuleFor(x => x.ShippingAddress!.AddressLine2).MaximumLength(500);
            RuleFor(x => x.ShippingAddress!.City).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ShippingAddress!.State).MaximumLength(200);
            RuleFor(x => x.ShippingAddress!.PostalCode).MaximumLength(20);
            RuleFor(x => x.ShippingAddress!.Country).NotEmpty().MaximumLength(100);
        });

        When(x => x.BillingAddress is not null, () =>
        {
            RuleFor(x => x.BillingAddress!.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.BillingAddress!.PhoneNumber).NotEmpty().MaximumLength(50);
            RuleFor(x => x.BillingAddress!.AddressLine1).NotEmpty().MaximumLength(500);
            RuleFor(x => x.BillingAddress!.AddressLine2).MaximumLength(500);
            RuleFor(x => x.BillingAddress!.City).NotEmpty().MaximumLength(200);
            RuleFor(x => x.BillingAddress!.State).MaximumLength(200);
            RuleFor(x => x.BillingAddress!.PostalCode).MaximumLength(20);
            RuleFor(x => x.BillingAddress!.Country).NotEmpty().MaximumLength(100);
        });
    }
}
