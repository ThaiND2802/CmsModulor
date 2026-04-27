namespace CommerceCore.Application.Responses;

public sealed class ErrorResponse
{
    public int Status { get; init; }

    public string ErrorCode { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public IReadOnlyDictionary<string, IReadOnlyList<string>>? Errors { get; init; }
}
