namespace Commerce.Modules.Order.Contracts;

public interface IOrderModule
{
    Task<OrderCreationResponse> CreateOrderFromSaleAsync(CreateOrderFromSaleRequest request, CancellationToken cancellationToken);
}
