using FluentValidation;

namespace Commerce.Modules.Inventory.Application.Commands.ReceiveStock;

public sealed class ReceiveStockValidator : AbstractValidator<ReceiveStockCommand>
{
    public ReceiveStockValidator()
    {
        RuleFor(x => x.VariantId)
            .NotEmpty();

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.ReferenceType)
            .MaximumLength(100);
    }
}
