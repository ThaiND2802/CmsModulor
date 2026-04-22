namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record GetUsersResult(
    IReadOnlyCollection<UserDto> Users,
    int Total,
    int Page,
    int PageSize);
