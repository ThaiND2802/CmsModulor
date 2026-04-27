using FluentValidation;

namespace Commerce.Modules.Order.Application.Commands.ChangeOrderStatus;

public sealed class ChangeOrderStatusCommandValidator : AbstractValidator<ChangeOrderStatusCommand>
{
    public ChangeOrderStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.ToStatus)
            .IsInEnum();

        RuleFor(x => x.Note)
            .MaximumLength(1000);
    }
}
