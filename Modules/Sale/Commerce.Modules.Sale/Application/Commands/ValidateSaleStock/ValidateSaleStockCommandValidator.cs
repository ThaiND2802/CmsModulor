using FluentValidation;

namespace Commerce.Modules.Sale.Application.Commands.ValidateSaleStock;

public sealed class ValidateSaleStockCommandValidator : AbstractValidator<ValidateSaleStockCommand>
{
    public ValidateSaleStockCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty();
    }
}
