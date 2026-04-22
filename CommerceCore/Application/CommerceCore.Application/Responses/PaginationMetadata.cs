namespace CommerceCore.Application.Responses;

public sealed class PaginationMetadata
{
    public int Total { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }
}
