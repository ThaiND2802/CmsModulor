using Commerce.Modules.Catalog.Application.Commands.CreateProduct;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.CreateProduct;

public sealed class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenCommandIsValid()
    {
        var command = new CreateProductCommand("Sneaker", "sneaker", null, null, "SKU-1", Guid.NewGuid(), null, false, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldHaveError_ForSku_WhenEmpty()
    {
        var command = new CreateProductCommand("Sneaker", "sneaker", null, null, string.Empty, Guid.NewGuid(), null, false, null);

        var result = _validator.Validate(command);

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(CreateProductCommand.Sku));
    }

    [Fact]
    public void Validate_ShouldHaveError_ForSlug_WhenInvalid()
    {
        var command = new CreateProductCommand("Sneaker", "Sneaker One", null, null, "SKU-1", Guid.NewGuid(), null, false, null);

        var result = _validator.Validate(command);

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(CreateProductCommand.Slug));
    }

    [Fact]
    public void Validate_ShouldHaveError_ForCategoryId_WhenEmpty()
    {
        var command = new CreateProductCommand("Sneaker", "sneaker", null, null, "SKU-1", Guid.Empty, null, false, null);

        var result = _validator.Validate(command);

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(CreateProductCommand.CategoryId));
    }
}
