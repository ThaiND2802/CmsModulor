using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commerce.Modules.Sale.Domain;

[Table("sale_items")]
public sealed class SaleItem
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("sale_id")]
    public Guid SaleId { get; set; }

    [Column("product_id")]
    public Guid? ProductId { get; set; }

    [Required]
    [Column("product_name")]
    [MaxLength(500)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [Column("product_sku")]
    [MaxLength(100)]
    public string ProductSku { get; set; } = string.Empty;

    [Column("variant_id")]
    public Guid? VariantId { get; set; }

    [Column("variant_name")]
    [MaxLength(300)]
    public string? VariantName { get; set; }

    [Column("unit_price")]
    public decimal UnitPrice { get; set; }

    [Column("override_price")]
    public decimal? OverridePrice { get; set; }

    [Column("override_reason")]
    [MaxLength(500)]
    public string? OverrideReason { get; set; }

    [Column("overridden_by")]
    [MaxLength(100)]
    public string? OverriddenBy { get; set; }

    [Column("overridden_at_utc")]
    public DateTime? OverriddenAtUtc { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("discount_amount")]
    public decimal DiscountAmount { get; set; }

    [Column("total_amount")]
    public decimal TotalAmount { get; set; }

    public Sale Sale { get; set; } = null!;
}
