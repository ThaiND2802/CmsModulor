using FluentValidation;

namespace Commerce.Modules.Catalog.Application.Commands.DeleteBrand;

public sealed class DeleteBrandCommandValidator : AbstractValidator<DeleteBrandCommand>
{
    public DeleteBrandCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
