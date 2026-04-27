using Commerce.Modules.Catalog.Application.Commands.UpdateProduct;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidatorTests
{
    private readonly UpdateProductCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenCommandIsValid()
    {
        var result = _validator.Validate(new UpdateProductCommand(Guid.NewGuid(), "Air", "air", "Desc", "Short", "SKU-1", Guid.NewGuid(), null, false, "sport"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldHaveError_ForSlug_WhenInvalid()
    {
        var result = _validator.Validate(new UpdateProductCommand(Guid.NewGuid(), "Air", "Air One", null, null, "SKU-1", Guid.NewGuid(), null, false, null));

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(UpdateProductCommand.Slug));
    }

    [Fact]
    public void Validate_ShouldHaveError_ForCategoryId_WhenEmpty()
    {
        var result = _validator.Validate(new UpdateProductCommand(Guid.NewGuid(), "Air", "air", null, null, "SKU-1", Guid.Empty, null, false, null));

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(UpdateProductCommand.CategoryId));
    }
}
