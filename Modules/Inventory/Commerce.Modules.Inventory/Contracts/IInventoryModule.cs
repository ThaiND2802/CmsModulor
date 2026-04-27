using Commerce.Modules.Inventory.Contracts.Requests;
using Commerce.Modules.Inventory.Contracts.Responses;

namespace Commerce.Modules.Inventory.Contracts;

public interface IInventoryModule
{
    Task<InventoryAvailabilityResponse?> ReserveStockAsync(ReserveStockRequest request, CancellationToken cancellationToken);

    Task<InventoryAvailabilityResponse?> ReleaseStockAsync(ReleaseStockRequest request, CancellationToken cancellationToken);
}
