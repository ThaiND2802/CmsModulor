using FluentValidation;

namespace Commerce.Modules.Sale.Application.Commands.UpdateSaleItem;

public sealed class UpdateSaleItemCommandValidator : AbstractValidator<UpdateSaleItemCommand>
{
    public UpdateSaleItemCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty();

        RuleFor(x => x.ItemId)
            .NotEmpty();

        RuleFor(x => x.VariantId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0);
    }
}
