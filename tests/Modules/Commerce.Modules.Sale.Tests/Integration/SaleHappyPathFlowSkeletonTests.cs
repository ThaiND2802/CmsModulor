using System.Net;
using FluentAssertions;

namespace Commerce.Modules.Sale.Tests.Integration;

public sealed class SaleHappyPathFlowSkeletonTests
{
    [Fact(Skip = "Skeleton only: wire WebApplicationFactory, auth, seeded catalog variant, and inventory before enabling.")]
    public Task Sale_HappyPath_CreatesDraft_Prices_Submits_AndLinksOrder()
    {
        // Arrange
        // TODO:
        // - create a WebApplicationFactory for Commerce.Host.WebApi
        // - seed one active catalog variant with matching inventory
        // - authenticate a user with Sale + Order permissions
        // - create an HttpClient

        HttpStatusCode createStatus = HttpStatusCode.Created;
        HttpStatusCode addItemStatus = HttpStatusCode.OK;
        HttpStatusCode updateCustomerStatus = HttpStatusCode.OK;
        HttpStatusCode updateAddressesStatus = HttpStatusCode.OK;
        HttpStatusCode repriceStatus = HttpStatusCode.OK;
        HttpStatusCode validateStockStatus = HttpStatusCode.OK;
        HttpStatusCode submitStatus = HttpStatusCode.OK;
        HttpStatusCode getSaleStatus = HttpStatusCode.OK;
        HttpStatusCode getOrderStatus = HttpStatusCode.OK;

        Guid saleId = Guid.Empty;
        Guid orderId = Guid.Empty;
        string saleStatus = "Submitted";

        // Act
        // TODO:
        // 1. POST /api/sale/sales and capture saleId
        // 2. POST /api/sale/sales/{saleId}/items
        // 3. PATCH /api/sale/sales/{saleId}/customer
        // 4. PATCH /api/sale/sales/{saleId}/addresses
        // 5. POST /api/sale/sales/{saleId}/reprice
        // 6. POST /api/sale/sales/{saleId}/validate-stock
        // 7. POST /api/sale/sales/{saleId}/submit and capture orderId
        // 8. GET /api/sale/sales/{saleId}
        // 9. GET /api/order/orders/{orderId}

        // Assert
        createStatus.Should().Be(HttpStatusCode.Created);
        addItemStatus.Should().Be(HttpStatusCode.OK);
        updateCustomerStatus.Should().Be(HttpStatusCode.OK);
        updateAddressesStatus.Should().Be(HttpStatusCode.OK);
        repriceStatus.Should().Be(HttpStatusCode.OK);
        validateStockStatus.Should().Be(HttpStatusCode.OK);
        submitStatus.Should().Be(HttpStatusCode.OK);
        getSaleStatus.Should().Be(HttpStatusCode.OK);
        getOrderStatus.Should().Be(HttpStatusCode.OK);
        saleId.Should().NotBe(Guid.Empty);
        orderId.Should().NotBe(Guid.Empty);
        saleStatus.Should().Be("Submitted");

        // TODO:
        // - assert sale detail still contains the expected item snapshot
        // - assert order totals match the repriced sale totals
        // - assert the order customer + address match the sale snapshot
        // - assert a second submit attempt does not create another order

        return Task.CompletedTask;
    }
}
