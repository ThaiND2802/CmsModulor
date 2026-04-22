namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record DeletePermissionResponse
{
    public Guid Id { get; init; }

    public bool Deleted { get; init; }
}
