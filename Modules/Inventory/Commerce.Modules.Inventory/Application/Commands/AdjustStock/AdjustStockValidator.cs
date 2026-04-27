using FluentValidation;

namespace Commerce.Modules.Inventory.Application.Commands.AdjustStock;

public sealed class AdjustStockValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockValidator()
    {
        RuleFor(x => x.VariantId)
            .NotEmpty();

        RuleFor(x => x.QuantityDelta)
            .NotEqual(0);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.ReferenceType)
            .MaximumLength(100);
    }
}
