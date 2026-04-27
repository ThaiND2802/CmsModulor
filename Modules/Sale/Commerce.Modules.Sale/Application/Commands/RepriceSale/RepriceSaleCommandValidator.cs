using FluentValidation;

namespace Commerce.Modules.Sale.Application.Commands.RepriceSale;

public sealed class RepriceSaleCommandValidator : AbstractValidator<RepriceSaleCommand>
{
    public RepriceSaleCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty();

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ShippingAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0);
    }
}
