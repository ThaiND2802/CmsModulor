using Commerce.Modules.Sale.Application.DTOs.Responses;
using MediatR;

namespace Commerce.Modules.Sale.Application.Queries.GetSaleById;

public sealed record GetSaleByIdQuery(Guid Id) : IRequest<SaleDto>;
