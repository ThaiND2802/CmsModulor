namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record DeleteUserResponse
{
    public Guid Id { get; init; }

    public bool Deleted { get; init; }
}
