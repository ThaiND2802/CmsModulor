using System.ComponentModel.DataAnnotations.Schema;
using CommerceCore.Application.Abstractions;

namespace Commerce.Modules.Identity.Domain;

[Table("identity_role_permissions")]
public sealed class RolePermission : IAuditableEntity, ISoftDelete
{
    public Guid Id { get; set; }

    public Guid RoleId { get; set; }

    public Guid PermissionId { get; set; }

    public PermissionEffect Effect { get; set; }

    public DateTime AssignedAtUtc { get; set; }

    public string? AssignedBy { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public string? DeletedBy { get; set; }

    public Role Role { get; set; } = null!;

    public Permission Permission { get; set; } = null!;
}
