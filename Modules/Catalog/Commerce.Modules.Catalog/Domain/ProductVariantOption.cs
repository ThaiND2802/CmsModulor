using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commerce.Modules.Catalog.Domain;

[Table("catalog_product_variant_options")]
public sealed class ProductVariantOption
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("variant_id")]
    public Guid VariantId { get; set; }

    [Column("attribute_id")]
    public Guid AttributeId { get; set; }

    [Required]
    [Column("value")]
    [MaxLength(500)]
    public string Value { get; set; } = string.Empty;

    public ProductVariant Variant { get; set; } = null!;

    public ProductAttribute Attribute { get; set; } = null!;
}
