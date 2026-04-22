namespace Commerce.Modules.Payment.Application.DTOs.Responses;

public sealed record PaymentCodPermissionsResponse
{
    public string? UserId { get; init; }

    public string? UserName { get; init; }

    public bool IsAuthenticated { get; init; }

    public PaymentCodPermissionFlags Permissions { get; init; } = default!;
}
