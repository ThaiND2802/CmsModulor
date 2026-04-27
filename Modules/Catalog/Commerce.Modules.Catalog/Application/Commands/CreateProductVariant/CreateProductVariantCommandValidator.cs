using FluentValidation;

namespace Commerce.Modules.Catalog.Application.Commands.CreateProductVariant;

public sealed class CreateProductVariantCommandValidator : AbstractValidator<CreateProductVariantCommand>
{
    public CreateProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.CompareAtPrice).GreaterThan(0).When(x => x.CompareAtPrice.HasValue);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Options)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(options => options!.Select(option => option.AttributeId).Distinct().Count() == options.Count)
            .WithMessage("Variant options cannot contain duplicate attributes.");
        RuleForEach(x => x.Options!).SetValidator(new VariantOptionRequestValidator())
            .When(x => x.Options is not null);
    }

    private sealed class VariantOptionRequestValidator : AbstractValidator<VariantOptionRequest>
    {
        public VariantOptionRequestValidator()
        {
            RuleFor(x => x.AttributeId).NotEmpty();
            RuleFor(x => x.Value).NotEmpty().MaximumLength(200);
        }
    }
}
