using FluentValidation;

namespace Commerce.Modules.Sale.Application.Commands.ExpireSale;

public sealed class ExpireSaleCommandValidator : AbstractValidator<ExpireSaleCommand>
{
    public ExpireSaleCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .MaximumLength(1000);
    }
}
