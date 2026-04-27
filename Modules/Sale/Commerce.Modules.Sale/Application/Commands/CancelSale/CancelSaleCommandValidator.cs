using FluentValidation;

namespace Commerce.Modules.Sale.Application.Commands.CancelSale;

public sealed class CancelSaleCommandValidator : AbstractValidator<CancelSaleCommand>
{
    public CancelSaleCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .MaximumLength(1000);
    }
}
