using FluentValidation;

namespace Commerce.Modules.Order.Application.Commands.UpdateOrderNotes;

public sealed class UpdateOrderNotesCommandValidator : AbstractValidator<UpdateOrderNotesCommand>
{
    public UpdateOrderNotesCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Notes)
            .MaximumLength(2000);
    }
}
