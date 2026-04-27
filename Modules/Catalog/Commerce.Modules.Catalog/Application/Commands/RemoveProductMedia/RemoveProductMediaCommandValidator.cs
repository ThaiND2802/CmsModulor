using FluentValidation;

namespace Commerce.Modules.Catalog.Application.Commands.RemoveProductMedia;

public sealed class RemoveProductMediaCommandValidator : AbstractValidator<RemoveProductMediaCommand>
{
    public RemoveProductMediaCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.MediaId).NotEmpty();
    }
}
