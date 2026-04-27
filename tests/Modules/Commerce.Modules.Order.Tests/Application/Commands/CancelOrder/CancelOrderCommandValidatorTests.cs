using Commerce.Modules.Order.Application.Commands.CancelOrder;
using FluentValidation.TestHelper;
using FluentAssertions;

namespace Commerce.Modules.Order.Tests.Application.Commands.CancelOrder;

public sealed class CancelOrderCommandValidatorTests
{
    private readonly CancelOrderCommandValidator _validator;

    public CancelOrderCommandValidatorTests()
    {
        _validator = new CancelOrderCommandValidator();
    }

    [Fact]
    public void EmptyReason_FailsValidation()
    {
        // Arrange
        var command = new CancelOrderCommand
        {
            Id = Guid.NewGuid(),
            CancelReason = string.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CancelReason);
    }

    [Fact]
    public void ValidReason_PassesValidation()
    {
        // Arrange
        var command = new CancelOrderCommand
        {
            Id = Guid.NewGuid(),
            CancelReason = "Customer requested cancellation"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}