using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommerceCore.Application.Abstractions;

namespace Commerce.Modules.Catalog.Domain;

[Table("catalog_brands")]
public sealed class Brand : IAuditableEntity, ISoftDelete
{
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("name")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("slug")]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    [Column("logo_url")]
    [MaxLength(2000)]
    public string? LogoUrl { get; set; }

    [Column("website")]
    [MaxLength(2000)]
    public string? Website { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

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

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
