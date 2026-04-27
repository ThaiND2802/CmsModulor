using Commerce.Modules.Inventory.Application.Commands.AdjustStock;
using Commerce.Modules.Inventory.Tests.TestCommon;
using FluentValidation.TestHelper;

namespace Commerce.Modules.Inventory.Tests.Application.Commands.AdjustStock;

public sealed class AdjustStockValidatorTests
{
    private readonly AdjustStockValidator _validator = new();

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var command = InventoryTestFixture.CreateValidAdjustStockCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyVariantId_FailsValidation()
    {
        var command = InventoryTestFixture.CreateValidAdjustStockCommand() with { VariantId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.VariantId);
    }

    [Fact]
    public void ZeroQuantityDelta_FailsValidation()
    {
        var command = InventoryTestFixture.CreateValidAdjustStockCommand() with { QuantityDelta = 0 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.QuantityDelta);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void EmptyReason_FailsValidation(string reason)
    {
        var command = InventoryTestFixture.CreateValidAdjustStockCommand() with { Reason = reason };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }
}
