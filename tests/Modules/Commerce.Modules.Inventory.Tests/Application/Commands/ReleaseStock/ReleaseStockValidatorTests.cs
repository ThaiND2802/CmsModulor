using Commerce.Modules.Inventory.Application.Commands.ReleaseStock;
using FluentValidation.TestHelper;

namespace Commerce.Modules.Inventory.Tests.Application.Commands.ReleaseStock;

public sealed class ReleaseStockValidatorTests
{
    private readonly ReleaseStockValidator _validator = new();

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var result = _validator.TestValidate(new ReleaseStockCommand(Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyOrderId_FailsValidation()
    {
        var result = _validator.TestValidate(new ReleaseStockCommand(Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.OrderId);
    }
}
