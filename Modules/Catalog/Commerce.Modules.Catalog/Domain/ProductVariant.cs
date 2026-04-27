using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommerceCore.Application.Abstractions;

namespace Commerce.Modules.Catalog.Domain;

[Table("catalog_product_variants")]
public sealed class ProductVariant : IAuditableEntity, ISoftDelete
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Required]
    [Column("sku")]
    [MaxLength(100)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [Column("name")]
    [MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    [Column("price")]
    public decimal Price { get; set; }

    [Column("compare_at_price")]
    public decimal? CompareAtPrice { get; set; }

    [Column("stock_quantity")]
    public int StockQuantity { get; set; }

    [Column("is_default")]
    public bool IsDefault { get; set; }

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

    public Product Product { get; set; } = null!;

    public ICollection<ProductVariantOption> Options { get; set; } = new List<ProductVariantOption>();
}
