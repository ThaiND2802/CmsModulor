using Commerce.Modules.Sale.Application.DTOs.Responses;
using MediatR;

namespace Commerce.Modules.Sale.Application.Queries.GetSaleTimeline;

public sealed record GetSaleTimelineQuery(Guid SaleId) : IRequest<SaleTimelineDto>;
