using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommerceCore.Application.Abstractions;

namespace Commerce.Modules.Identity.Domain;

[Table("identity_user_sessions")]
public sealed class UserSession : IAuditableEntity
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("refresh_token_hash")]
    [MaxLength(128)]
    public string RefreshTokenHash { get; set; } = string.Empty;

    [Column("expires_at_utc")]
    public DateTime ExpiresAtUtc { get; set; }

    [Column("revoked_at_utc")]
    public DateTime? RevokedAtUtc { get; set; }

    [Column("last_used_at_utc")]
    public DateTime? LastUsedAtUtc { get; set; }

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

    public User User { get; set; } = null!;
}
