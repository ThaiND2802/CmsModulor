namespace CommerceCore.SharedKernel.Exceptions;

public sealed class NotFoundAppException : AppException
{
    public NotFoundAppException(string message, string errorCode = "1404")
        : base(errorCode, message)
    {
    }
}
