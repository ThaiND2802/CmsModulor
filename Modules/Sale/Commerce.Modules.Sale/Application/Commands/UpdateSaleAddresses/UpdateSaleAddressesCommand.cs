using MediatR;

namespace Commerce.Modules.Sale.Application.Commands.UpdateSaleAddresses;

public sealed record UpdateSaleAddressRequest(
    string FullName,
    string PhoneNumber,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? State,
    string? PostalCode,
    string Country);

public sealed record UpdateSaleAddressesCommand : IRequest
{
    public Guid Id { get; init; }

    public UpdateSaleAddressRequest? ShippingAddress { get; init; }

    public UpdateSaleAddressRequest? BillingAddress { get; init; }
}
