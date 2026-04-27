using Commerce.Modules.Inventory.Application.Commands.ReserveStock;
using FluentValidation.TestHelper;

namespace Commerce.Modules.Inventory.Tests.Application.Commands.ReserveStock;

public sealed class ReserveStockValidatorTests
{
    private readonly ReserveStockValidator _validator = new();

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var command = new ReserveStockCommand
        {
            OrderId = Guid.NewGuid(),
            Items = [new ReserveStockItemRequest(Guid.NewGuid(), 2)]
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyOrderId_FailsValidation()
    {
        var command = new ReserveStockCommand
        {
            OrderId = Guid.Empty,
            Items = [new ReserveStockItemRequest(Guid.NewGuid(), 2)]
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OrderId);
    }

    [Fact]
    public void DuplicateVariants_FailsValidation()
    {
        var variantId = Guid.NewGuid();
        var command = new ReserveStockCommand
        {
            OrderId = Guid.NewGuid(),
            Items = [new ReserveStockItemRequest(variantId, 1), new ReserveStockItemRequest(variantId, 2)]
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Items);
    }
}
