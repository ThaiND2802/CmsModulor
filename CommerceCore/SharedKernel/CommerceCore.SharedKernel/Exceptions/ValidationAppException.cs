namespace CommerceCore.SharedKernel.Exceptions;

public sealed class ValidationAppException : AppException
{
    public ValidationAppException(
        string message,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? errors = null,
        string errorCode = "1001")
        : base(errorCode, message)
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, IReadOnlyList<string>>? Errors { get; }
}
