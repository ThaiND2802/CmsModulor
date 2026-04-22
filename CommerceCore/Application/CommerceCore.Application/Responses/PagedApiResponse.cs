namespace CommerceCore.Application.Responses;

public sealed class PagedApiResponse<T>
{
    public int Status { get; init; }

    public IReadOnlyCollection<T> Data { get; init; } = Array.Empty<T>();

    public PaginationMetadata Pagination { get; init; } = new();
}
