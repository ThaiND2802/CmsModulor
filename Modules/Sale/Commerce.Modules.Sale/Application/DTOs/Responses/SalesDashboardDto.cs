namespace Commerce.Modules.Sale.Application.DTOs.Responses;

public sealed record SalesDashboardDto
{
    public int DraftSales { get; init; }
    public int PricedSales { get; init; }
    public int SubmittedSales { get; init; }
    public int CancelledSales { get; init; }
    public int ExpiredSales { get; init; }
    public decimal TotalRevenue { get; init; }
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
