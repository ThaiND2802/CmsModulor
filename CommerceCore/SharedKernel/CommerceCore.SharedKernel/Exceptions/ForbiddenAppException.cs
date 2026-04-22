namespace CommerceCore.SharedKernel.Exceptions;

public sealed class ForbiddenAppException : AppException
{
    public ForbiddenAppException(string message, string errorCode = "1403")
        : base(errorCode, message)
    {
    }
}
