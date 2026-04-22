namespace CommerceCore.SharedKernel.Exceptions;

public sealed class ConflictAppException : AppException
{
    public ConflictAppException(string message, string errorCode = "1409")
        : base(errorCode, message)
    {
    }
}
