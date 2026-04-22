namespace Commerce.Modules.Payment.Application.DTOs.Responses;

public sealed record PaymentCodPermissionsResponse(
    string? UserId,
    string? UserName,
    bool IsAuthenticated,
    PaymentCodPermissionFlags Permissions);
