using Commerce.Modules.Sale.Application.DTOs.Responses;
using MediatR;

namespace Commerce.Modules.Sale.Application.Queries.GetSalesDashboard;

public sealed record GetSalesDashboardQuery : IRequest<SalesDashboardDto>;
