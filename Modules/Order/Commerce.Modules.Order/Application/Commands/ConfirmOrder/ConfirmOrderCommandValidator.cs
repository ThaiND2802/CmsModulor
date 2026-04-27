using FluentValidation;

namespace Commerce.Modules.Order.Application.Commands.ConfirmOrder;

public sealed class ConfirmOrderCommandValidator : AbstractValidator<ConfirmOrderCommand>
{
    public ConfirmOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Note)
            .MaximumLength(1000);
    }
}
