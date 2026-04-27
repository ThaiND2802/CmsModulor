using FluentValidation;

namespace Commerce.Modules.Catalog.Application.Commands.UpdateProductVariant;

public sealed class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
{
    public UpdateProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.CompareAtPrice).GreaterThan(0).When(x => x.CompareAtPrice.HasValue);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}
