using FluentValidation;

namespace Commerce.Modules.Order.Application.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.ProductName)
                    .NotEmpty()
                    .MaximumLength(500);

                item.RuleFor(x => x.ProductSku)
                    .NotEmpty()
                    .MaximumLength(100);

                item.RuleFor(x => x.UnitPrice)
                    .GreaterThan(0);

                item.RuleFor(x => x.Quantity)
                    .GreaterThan(0);

                item.RuleFor(x => x.DiscountAmount)
                    .GreaterThanOrEqualTo(0)
                    .LessThanOrEqualTo(x => x.UnitPrice * x.Quantity);
            });

        RuleFor(x => x)
            .Must(x => x.DiscountAmount <= (x.Items?.Sum(item => (item.UnitPrice * item.Quantity) - item.DiscountAmount) ?? 0))
            .WithMessage("Order discount must be less than or equal to the subtotal amount.");

        RuleFor(x => x.ShippingAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3);

        RuleFor(x => x.ShippingAddress)
            .NotNull();

        RuleFor(x => x.ShippingAddress.FullName)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.ShippingAddress is not null);

        RuleFor(x => x.ShippingAddress.PhoneNumber)
            .NotEmpty()
            .MaximumLength(50)
            .When(x => x.ShippingAddress is not null);

        RuleFor(x => x.ShippingAddress.AddressLine1)
            .NotEmpty()
            .MaximumLength(500)
            .When(x => x.ShippingAddress is not null);

        RuleFor(x => x.ShippingAddress.City)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.ShippingAddress is not null);

        RuleFor(x => x.ShippingAddress.Country)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.ShippingAddress is not null);
    }
}
