namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record DeleteRoleResponse
{
    public Guid Id { get; init; }

    public bool Deleted { get; init; }
}
