namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed class LoginResponse
{
    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }

    public required string TokenType { get; init; }

    public required DateTime ExpiresAtUtc { get; init; }

    public required DateTime RefreshExpiresAtUtc { get; init; }
}
