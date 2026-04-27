using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commerce.Modules.Catalog.Domain;

[Table("catalog_product_attribute_values")]
public sealed class ProductAttributeValue
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Column("attribute_id")]
    public Guid AttributeId { get; set; }

    [Required]
    [Column("value")]
    [MaxLength(500)]
    public string Value { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;

    public ProductAttribute Attribute { get; set; } = null!;
}
