using FluentValidation;

namespace Commerce.Modules.Sale.Application.Commands.SelectPaymentMethod;

public sealed class SelectPaymentMethodCommandValidator : AbstractValidator<SelectPaymentMethodCommand>
{
    public SelectPaymentMethodCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty()
            .WithMessage("SaleId is required");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .NotEqual(Domain.PaymentMethod.None)
            .WithMessage("Valid payment method is required");
    }
}
