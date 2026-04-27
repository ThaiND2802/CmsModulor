using FluentValidation;

namespace Commerce.Modules.Inventory.Application.Commands.ReserveStock;

public sealed class ReserveStockValidator : AbstractValidator<ReserveStockCommand>
{
    public ReserveStockValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();

        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.VariantId)
                    .NotEmpty();

                item.RuleFor(x => x.Quantity)
                    .GreaterThan(0);
            });

        RuleFor(x => x.Items)
            .Must(items => items
                .GroupBy(static item => item.VariantId)
                .All(static group => group.Count() == 1))
            .WithMessage("Duplicate variants are not allowed.");
    }
}
