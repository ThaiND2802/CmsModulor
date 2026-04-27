namespace Commerce.Modules.Order.Contracts;

public sealed record OrderCreationResponse(
    bool Success,
    Guid? OrderId,
    string? OrderNumber,
    string? ErrorCode,
    string? ErrorMessage);
