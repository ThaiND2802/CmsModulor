using FluentValidation;

namespace Commerce.Modules.Sale.Application.Commands.AddSaleItem;

public sealed class AddSaleItemCommandValidator : AbstractValidator<AddSaleItemCommand>
{
    public AddSaleItemCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty();

        RuleFor(x => x.VariantId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0);
    }
}
