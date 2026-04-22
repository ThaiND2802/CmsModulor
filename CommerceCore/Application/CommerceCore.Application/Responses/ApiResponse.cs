namespace CommerceCore.Application.Responses;

public sealed class ApiResponse<T>
{
    public int Status { get; init; }

    public T? Data { get; init; }
}
