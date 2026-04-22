namespace CommerceCore.Application.Abstractions;

public interface ICreatedAuditable
{
    DateTime CreatedAtUtc { get; set; }

    string? CreatedBy { get; set; }
}
