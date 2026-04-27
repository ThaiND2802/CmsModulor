using Commerce.Modules.Catalog.Domain;
using FluentValidation;

namespace Commerce.Modules.Catalog.Application.Commands.ChangeProductStatus;

public sealed class ChangeProductStatusCommandValidator : AbstractValidator<ChangeProductStatusCommand>
{
    public ChangeProductStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}
