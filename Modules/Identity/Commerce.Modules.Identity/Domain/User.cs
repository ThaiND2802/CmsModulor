using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommerceCore.Application.Abstractions;

namespace Commerce.Modules.Identity.Domain;

[Table("identity_users")]
public sealed class User : IAuditableEntity, ISoftDelete
{
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("user_name")]
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [Column("normalized_user_name")]
    [MaxLength(100)]
    public string NormalizedUserName { get; set; } = string.Empty;

    [Required]
    [Column("display_name")]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [Column("email")]
    [MaxLength(256)]
    public string? Email { get; set; }

    [Column("normalized_email")]
    [MaxLength(256)]
    public string? NormalizedEmail { get; set; }

    [Column("password_hash")]
    [MaxLength(512)]
    public string? PasswordHash { get; set; }

    [Column("phone_number")]
    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("is_system")]
    public bool IsSystem { get; set; }

    [Column("last_login_at_utc")]
    public DateTime? LastLoginAtUtc { get; set; }

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

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
}
