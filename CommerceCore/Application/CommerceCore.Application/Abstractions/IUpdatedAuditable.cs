namespace CommerceCore.Application.Abstractions;

public interface IUpdatedAuditable
{
    DateTime? UpdatedAtUtc { get; set; }

    string? UpdatedBy { get; set; }
}
