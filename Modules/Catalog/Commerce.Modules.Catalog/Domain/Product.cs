using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommerceCore.Application.Abstractions;

namespace Commerce.Modules.Catalog.Domain;

[Table("catalog_products")]
public sealed class Product : IAuditableEntity, ISoftDelete
{
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("name")]
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("slug")]
    [MaxLength(500)]
    public string Slug { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(4000)]
    public string? Description { get; set; }

    [Column("short_description")]
    [MaxLength(1000)]
    public string? ShortDescription { get; set; }

    [Required]
    [Column("sku")]
    [MaxLength(100)]
    public string Sku { get; set; } = string.Empty;

    [Column("category_id")]
    public Guid CategoryId { get; set; }

    [Column("brand_id")]
    public Guid? BrandId { get; set; }

    [Column("status")]
    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    [Column("is_featured")]
    public bool IsFeatured { get; set; }

    [Column("tags")]
    [MaxLength(2000)]
    public string? Tags { get; set; }

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

    public Category Category { get; set; } = null!;

    public Brand? Brand { get; set; }

    public ICollection<ProductMedia> Media { get; set; } = new List<ProductMedia>();

    public ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}
