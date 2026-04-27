using FluentValidation;

namespace Commerce.Modules.Sale.Application.Commands.OverrideSalePrice;

public sealed class OverrideSalePriceCommandValidator : AbstractValidator<OverrideSalePriceCommand>
{
    public OverrideSalePriceCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty();

        RuleFor(x => x.ItemId)
            .NotEmpty();

        RuleFor(x => x.OverridePrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);
    }
}
