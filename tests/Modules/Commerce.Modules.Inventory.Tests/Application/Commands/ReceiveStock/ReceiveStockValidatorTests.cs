using Commerce.Modules.Inventory.Application.Commands.ReceiveStock;
using Commerce.Modules.Inventory.Tests.TestCommon;
using FluentValidation.TestHelper;

namespace Commerce.Modules.Inventory.Tests.Application.Commands.ReceiveStock;

public sealed class ReceiveStockValidatorTests
{
    private readonly ReceiveStockValidator _validator = new();

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var command = InventoryTestFixture.CreateValidReceiveStockCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyVariantId_FailsValidation()
    {
        var command = InventoryTestFixture.CreateValidReceiveStockCommand() with { VariantId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.VariantId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositiveQuantity_FailsValidation(int quantity)
    {
        var command = InventoryTestFixture.CreateValidReceiveStockCommand() with { Quantity = quantity };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void EmptySku_FailsValidation(string sku)
    {
        var command = InventoryTestFixture.CreateValidReceiveStockCommand() with { Sku = sku };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Sku);
    }
}
