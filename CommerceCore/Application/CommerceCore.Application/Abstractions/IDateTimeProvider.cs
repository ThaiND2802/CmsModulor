namespace CommerceCore.Application.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
