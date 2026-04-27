namespace Commerce.Modules.Sale.Application.DTOs.Responses;

public sealed record SaleTimelineDto
{
    public Guid SaleId { get; init; }
    public string SaleNumber { get; init; } = string.Empty;
    public List<SaleTimelineEventDto> Events { get; init; } = new();
}
