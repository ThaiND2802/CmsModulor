using FluentValidation;

namespace Commerce.Modules.Sale.Application.Commands.MarkSalePaid;

public sealed class MarkSalePaidCommandValidator : AbstractValidator<MarkSalePaidCommand>
{
    public MarkSalePaidCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty()
            .WithMessage("SaleId is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0");

        RuleFor(x => x.PaymentReference)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.PaymentReference))
            .WithMessage("PaymentReference cannot exceed 200 characters");
    }
}
