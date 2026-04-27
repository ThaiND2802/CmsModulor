using Commerce.Modules.Sale.Application.DTOs.Responses;
using Commerce.Modules.Sale.Domain;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Sale.Application.Queries.GetSales;

public sealed class GetSalesQuery : PagedListRequest, IRequest<PagedApiResponse<SaleListItemDto>>
{
    public Guid? CustomerId { get; init; }

    public SaleStatus? Status { get; init; }
}
