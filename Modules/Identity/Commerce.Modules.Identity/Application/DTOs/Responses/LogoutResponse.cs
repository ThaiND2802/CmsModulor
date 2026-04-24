namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record LogoutResponse
{
    public bool LoggedOut { get; init; }
}
