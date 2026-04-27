using FluentValidation;

namespace Commerce.Modules.Order.Application.Commands.CancelOrder;

public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.CancelReason)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
