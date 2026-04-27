using FluentValidation;

namespace Commerce.Modules.Catalog.Application.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9-]+$");
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.ShortDescription).MaximumLength(1000);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Tags).MaximumLength(2000);
    }
}
