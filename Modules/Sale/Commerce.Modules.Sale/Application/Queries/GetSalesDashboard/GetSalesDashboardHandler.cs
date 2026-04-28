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

        var totalRevenue = sales
            .Where(s => s.PaymentStatus == PaymentStatus.Paid)
            .Sum(s => s.TotalAmount);

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
            DraftSales = sales.Count(s => s.Status == SaleStatus.Draft),
            PricedSales = sales.Count(s => s.Status == SaleStatus.Priced),
            SubmittedSales = sales.Count(s => s.Status == SaleStatus.Submitted),
            CancelledSales = sales.Count(s => s.Status == SaleStatus.Cancelled),
            ExpiredSales = sales.Count(s => s.Status == SaleStatus.Expired),
            TotalRevenue = totalRevenue,
            RecentSales = recentSales
        };
    }
}
