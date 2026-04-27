using FluentValidation;

namespace Commerce.Modules.Sale.Application.Commands.InitiateSalePayment;

public sealed class InitiateSalePaymentCommandValidator : AbstractValidator<InitiateSalePaymentCommand>
{
    public InitiateSalePaymentCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty()
            .WithMessage("SaleId is required");

        RuleFor(x => x.PaymentReference)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.PaymentReference))
            .WithMessage("PaymentReference cannot exceed 200 characters");
    }
}
