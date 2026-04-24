using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommerceCore.Application.Abstractions;

namespace Commerce.Modules.Identity.Domain;

[Table("identity_user_permissions")]
public sealed class UserPermission : IAuditableEntity, ISoftDelete
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("permission_id")]
    public Guid PermissionId { get; set; }

    [Column("effect")]
    public PermissionEffect Effect { get; set; }

    [Column("assigned_at_utc")]
    public DateTime AssignedAtUtc { get; set; }

    [Column("assigned_by")]
    [MaxLength(100)]
    public string? AssignedBy { get; set; }

    [Column("expires_at_utc")]
    public DateTime? ExpiresAtUtc { get; set; }

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("created_by")]
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [Column("updated_at_utc")]
    public DateTime? UpdatedAtUtc { get; set; }

    [Column("updated_by")]
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    [Column("deleted_at_utc")]
    public DateTime? DeletedAtUtc { get; set; }

    [Column("deleted_by")]
    [MaxLength(100)]
    public string? DeletedBy { get; set; }

    public User User { get; set; } = null!;

    public Permission Permission { get; set; } = null!;
}
