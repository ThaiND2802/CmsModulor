using FluentValidation;

namespace Commerce.Modules.Sale.Application.Commands.SubmitSale;

public sealed class SubmitSaleCommandValidator : AbstractValidator<SubmitSaleCommand>
{
    public SubmitSaleCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty();
    }
}
