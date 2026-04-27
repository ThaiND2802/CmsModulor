namespace Commerce.Modules.Sale.Application.DTOs.Responses;

public sealed record SalesDashboardDto
{
    public int TotalDraftSales { get; init; }
    public int TotalPricedSales { get; init; }
    public int TotalAwaitingPayment { get; init; }
    public int TotalPaidSales { get; init; }
    public int TotalSubmittedSales { get; init; }
    public int TotalExpiredSales { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal AverageOrderValue { get; init; }
    public List<SaleSummaryDto> RecentSales { get; init; } = new();
}

public sealed record SaleSummaryDto
{
    public Guid Id { get; init; }
    public string SaleNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public string? CustomerEmail { get; init; }
}
