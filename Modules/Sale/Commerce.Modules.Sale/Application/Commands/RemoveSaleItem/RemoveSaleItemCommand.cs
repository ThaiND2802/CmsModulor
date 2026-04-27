using Commerce.Modules.Sale.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Sale.Application.Commands.RemoveSaleItem;

public sealed record RemoveSaleItemCommand : IRequest<ApiResponse<SaleDto>>
{
    public Guid SaleId { get; init; }

    public Guid ItemId { get; init; }
}
