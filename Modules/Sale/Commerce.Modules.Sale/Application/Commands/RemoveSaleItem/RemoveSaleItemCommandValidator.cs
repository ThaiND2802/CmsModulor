using FluentValidation;

namespace Commerce.Modules.Sale.Application.Commands.RemoveSaleItem;

public sealed class RemoveSaleItemCommandValidator : AbstractValidator<RemoveSaleItemCommand>
{
    public RemoveSaleItemCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty();

        RuleFor(x => x.ItemId)
            .NotEmpty();
    }
}
