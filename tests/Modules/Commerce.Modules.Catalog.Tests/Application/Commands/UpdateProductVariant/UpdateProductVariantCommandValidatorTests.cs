using Commerce.Modules.Catalog.Application.Commands.UpdateProductVariant;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.UpdateProductVariant;

public sealed class UpdateProductVariantCommandValidatorTests
{
    private readonly UpdateProductVariantCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenCommandIsValid()
    {
        var result = _validator.Validate(new UpdateProductVariantCommand(Guid.NewGuid(), Guid.NewGuid(), "SKU-1", "Red", 10m, 12m, 5, true, true));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldHaveError_ForPrice_WhenZero()
    {
        var result = _validator.Validate(new UpdateProductVariantCommand(Guid.NewGuid(), Guid.NewGuid(), "SKU-1", "Red", 0m, null, 5, true, true));

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(UpdateProductVariantCommand.Price));
    }

    [Fact]
    public void Validate_ShouldHaveError_ForStockQuantity_WhenNegative()
    {
        var result = _validator.Validate(new UpdateProductVariantCommand(Guid.NewGuid(), Guid.NewGuid(), "SKU-1", "Red", 10m, null, -1, true, true));

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(UpdateProductVariantCommand.StockQuantity));
    }
}
