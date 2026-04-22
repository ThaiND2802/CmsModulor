using CommerceCore.Application.Abstractions;

namespace Commerce.Modules.Identity.Domain;

public sealed class UserRole : IAuditableEntity, ISoftDelete
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public DateTime AssignedAtUtc { get; set; }

    public string? AssignedBy { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public string? DeletedBy { get; set; }

    public User User { get; set; } = null!;

    public Role Role { get; set; } = null!;
}
