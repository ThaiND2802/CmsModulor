namespace CommerceCore.SharedKernel.Exceptions;

public sealed class ValidationAppException : AppException
{
    public ValidationAppException(string message, string errorCode = "1001")
        : base(errorCode, message)
    {
    }
}
