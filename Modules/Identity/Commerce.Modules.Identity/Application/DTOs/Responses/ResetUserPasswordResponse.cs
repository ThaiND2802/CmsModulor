namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record ResetUserPasswordResponse
{
    public Guid Id { get; init; }

    public bool PasswordReset { get; init; }
}
