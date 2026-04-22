namespace CommerceCore.SharedKernel.Exceptions;

public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message, string errorCode = "1401")
        : base(errorCode, message)
    {
    }
}
