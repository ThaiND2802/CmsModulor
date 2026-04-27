using System.Reflection;
using Commerce.Modules.Inventory.Contracts;
using Commerce.Modules.Inventory.Controllers;
using CommerceCore.FeatureManagement.Attributes;
using CommerceCore.FeatureManagement.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Commerce.Modules.Inventory.Tests.Controllers;

public sealed class InventoryControllerAuthorizationTests
{
    [Fact]
    public void Controller_RequiresInventoryModule()
    {
        var attribute = typeof(InventoryController).GetCustomAttribute<RequireModuleAttribute>();

        attribute.Should().NotBeNull();
        attribute!.ModuleName.Should().Be("Inventory");
    }

    [Theory]
    [InlineData(nameof(InventoryController.Receive), InventoryPermissions.StockReceive)]
    [InlineData(nameof(InventoryController.Adjust), InventoryPermissions.StockAdjust)]
    [InlineData(nameof(InventoryController.GetByVariant), InventoryPermissions.StockView)]
    [InlineData(nameof(InventoryController.GetList), InventoryPermissions.StockView)]
    [InlineData(nameof(InventoryController.Reserve), InventoryPermissions.ReservationsCreate)]
    [InlineData(nameof(InventoryController.Release), InventoryPermissions.ReservationsRelease)]
    [InlineData(nameof(InventoryController.GetMovements), InventoryPermissions.StockViewMovements)]
    public void Actions_RequireExpectedPermission(string actionName, string permission)
    {
        var method = typeof(InventoryController).GetMethod(actionName);

        method.Should().NotBeNull();
        method!.GetCustomAttribute<PermissionAuthorizeAttribute>()?.Policy
            .Should().Be($"{PermissionAuthorizeAttribute.PolicyPrefix}:{permission}");
    }

    [Theory]
    [InlineData(nameof(InventoryController.Receive))]
    [InlineData(nameof(InventoryController.Adjust))]
    [InlineData(nameof(InventoryController.GetByVariant))]
    [InlineData(nameof(InventoryController.GetList))]
    [InlineData(nameof(InventoryController.Reserve))]
    [InlineData(nameof(InventoryController.Release))]
    [InlineData(nameof(InventoryController.GetMovements))]
    public void Actions_AreHttpEndpoints(string actionName)
    {
        var method = typeof(InventoryController).GetMethod(actionName);

        method.Should().NotBeNull();
        method!.GetCustomAttributes()
            .Should()
            .Contain(attribute => attribute is HttpMethodAttribute);
    }
}
