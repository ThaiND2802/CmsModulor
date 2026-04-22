namespace CommerceCore.SharedKernel.Exceptions;

public sealed class BusinessRuleAppException : AppException
{
    public BusinessRuleAppException(string message, string errorCode = "1422")
        : base(errorCode, message)
    {
    }
}
