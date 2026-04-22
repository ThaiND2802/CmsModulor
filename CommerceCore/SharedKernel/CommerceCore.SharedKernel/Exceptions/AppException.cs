namespace CommerceCore.SharedKernel.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string errorCode, string message)
        : base(message)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            throw new ArgumentException("Error code is required.", nameof(errorCode));
        }

        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}
