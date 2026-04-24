namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record ChangePasswordResponse
{
    public bool Changed { get; init; }
}
