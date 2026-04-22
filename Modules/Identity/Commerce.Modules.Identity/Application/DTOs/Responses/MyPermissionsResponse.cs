namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record MyPermissionsResponse
{
    public Guid UserId { get; init; }

    public string UserName { get; init; } = default!;

    public bool IsAuthenticated { get; init; }

    public IReadOnlyList<string> Permissions { get; init; } = [];
}
