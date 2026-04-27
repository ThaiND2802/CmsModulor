using Commerce.Modules.Sale.Application.DTOs.Responses;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Application.Queries.GetSalesDashboard;

public sealed class GetSalesDashboardHandler : IRequestHandler<GetSalesDashboardQuery, SalesDashboardDto>
{
    private readonly SaleDbContext _context;

    public GetSalesDashboardHandler(SaleDbContext context)
    {
        _context = context;
    }

    public async Task<SalesDashboardDto> Handle(GetSalesDashboardQuery request, CancellationToken cancellationToken)
    {
        var sales = await _context.Sales
            .Where(s => !s.IsDeleted)
            .ToListAsync(cancellationToken);

        var paidSales = sales.Where(s => s.Status == SaleStatus.Paid).ToList();
        var totalRevenue = paidSales.Sum(s => s.TotalAmount);
        var averageOrderValue = paidSales.Any() ? totalRevenue / paidSales.Count : 0;

        var recentSales = sales
            .OrderByDescending(s => s.CreatedAtUtc)
            .Take(10)
            .Select(s => new SaleSummaryDto
            {
                Id = s.Id,
                SaleNumber = s.SaleNumber,
                Status = s.Status.ToString(),
                TotalAmount = s.TotalAmount,
                Currency = s.Currency,
                CreatedAtUtc = s.CreatedAtUtc,
                CustomerEmail = s.CustomerEmail
            })
            .ToList();

        return new SalesDashboardDto
        {
            TotalDraftSales = sales.Count(s => s.Status == SaleStatus.Draft),
            TotalPricedSales = sales.Count(s => s.Status == SaleStatus.Priced),
            TotalAwaitingPayment = sales.Count(s => s.Status == SaleStatus.AwaitingPayment),
            TotalPaidSales = paidSales.Count,
            TotalSubmittedSales = sales.Count(s => s.Status == SaleStatus.Submitted),
            TotalExpiredSales = sales.Count(s => s.Status == SaleStatus.Expired),
            TotalRevenue = totalRevenue,
            AverageOrderValue = averageOrderValue,
            RecentSales = recentSales
        };
    }
}
