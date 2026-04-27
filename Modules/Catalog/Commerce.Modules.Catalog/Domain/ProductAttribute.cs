using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommerceCore.Application.Abstractions;

namespace Commerce.Modules.Catalog.Domain;

[Table("catalog_product_attributes")]
public sealed class ProductAttribute : IAuditableEntity
{
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("name")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("display_name")]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

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

    public ICollection<ProductAttributeValue> ProductAttributeValues { get; set; } = new List<ProductAttributeValue>();

    public ICollection<ProductVariantOption> ProductVariantOptions { get; set; } = new List<ProductVariantOption>();
}
