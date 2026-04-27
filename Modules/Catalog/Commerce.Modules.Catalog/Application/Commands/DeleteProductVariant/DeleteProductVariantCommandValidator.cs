using FluentValidation;

namespace Commerce.Modules.Catalog.Application.Commands.DeleteProductVariant;

public sealed class DeleteProductVariantCommandValidator : AbstractValidator<DeleteProductVariantCommand>
{
    public DeleteProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
    }
}
