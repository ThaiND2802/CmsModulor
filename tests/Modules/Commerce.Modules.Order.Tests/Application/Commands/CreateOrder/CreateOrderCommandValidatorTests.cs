using Commerce.Modules.Order.Application.Commands.CreateOrder;
using Commerce.Modules.Order.Tests.TestCommon;
using FluentValidation.TestHelper;

namespace Commerce.Modules.Order.Tests.Application.Commands.CreateOrder;

public sealed class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator;

    public CreateOrderCommandValidatorTests()
    {
        _validator = new CreateOrderCommandValidator();
    }

    private static CreateOrderCommand CreateValidCommand() =>
        OrderTestFixture.CreateValidCreateOrderCommand();

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyItems_FailsWithExpectedMessage()
    {
        // Arrange
        var command = CreateValidCommand() with { Items = [] };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("Order must contain at least one item.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData(null)]
    public void InvalidEmail_Fails(string? email)
    {
        // Arrange
        var command = CreateValidCommand() with { CustomerEmail = email! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerEmail);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void NegativeUnitPrice_Fails(decimal unitPrice)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Items =
            [
                new CreateOrderItemRequest(
                    ProductId: Guid.NewGuid(),
                    ProductName: "Test Product",
                    ProductSku: "TEST-SKU",
                    VariantId: null,
                    VariantName: null,
                    UnitPrice: unitPrice,
                    Quantity: 1,
                    DiscountAmount: 0
                )
            ]
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].UnitPrice");
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("USDA")]
    public void ZeroCurrencyLength_Fails(string currency)
    {
        // Arrange
        var command = CreateValidCommand() with { Currency = currency };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void MissingShippingAddressCity_Fails()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            ShippingAddress = new CreateOrderAddressRequest(
                FullName: "John Doe",
                PhoneNumber: "1234567890",
                AddressLine1: "123 Main St",
                AddressLine2: null,
                City: string.Empty, // Missing city
                State: "NY",
                PostalCode: "10001",
                Country: "USA"
            )
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("ShippingAddress.City");
    }
}