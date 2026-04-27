namespace Commerce.Modules.Sale.Application.DTOs.Responses;

public sealed record SaleTimelineEventDto
{
    public DateTime OccurredAtUtc { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Actor { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}
