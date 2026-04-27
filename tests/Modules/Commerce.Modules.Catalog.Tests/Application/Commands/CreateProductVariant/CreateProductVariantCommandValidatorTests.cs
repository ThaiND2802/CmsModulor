using Commerce.Modules.Catalog.Application.Commands.CreateProductVariant;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.CreateProductVariant;

public sealed class CreateProductVariantCommandValidatorTests
{
    private readonly CreateProductVariantCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_ForPrice_WhenZero()
    {
        var command = CreateCommand(price: 0m);

        var result = _validator.Validate(command);

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(CreateProductVariantCommand.Price));
    }

    [Fact]
    public void Validate_ShouldHaveError_ForStockQuantity_WhenNegative()
    {
        var command = CreateCommand(stockQuantity: -1);

        var result = _validator.Validate(command);

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(CreateProductVariantCommand.StockQuantity));
    }

    [Fact]
    public void Validate_ShouldHaveError_ForOptions_WhenEmpty()
    {
        var command = CreateCommand(options: []);

        var result = _validator.Validate(command);

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(CreateProductVariantCommand.Options));
    }

    private static CreateProductVariantCommand CreateCommand(
        decimal price = 10m,
        int stockQuantity = 1,
        IReadOnlyList<VariantOptionRequest>? options = null)
    {
        return new CreateProductVariantCommand(
            Guid.NewGuid(),
            "SKU-1",
            "Variant",
            price,
            null,
            stockQuantity,
            true,
            options ?? [new VariantOptionRequest(Guid.NewGuid(), "Red")]);
    }
}
