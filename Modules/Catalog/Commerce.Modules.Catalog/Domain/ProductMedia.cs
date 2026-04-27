using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commerce.Modules.Catalog.Domain;

[Table("catalog_product_media")]
public sealed class ProductMedia
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Required]
    [Column("url")]
    [MaxLength(2000)]
    public string Url { get; set; } = string.Empty;

    [Column("alt_text")]
    [MaxLength(500)]
    public string? AltText { get; set; }

    [Column("media_type")]
    public MediaType MediaType { get; set; }

    [Column("display_order")]
    public int DisplayOrder { get; set; }

    [Column("is_primary")]
    public bool IsPrimary { get; set; }

    public Product Product { get; set; } = null!;
}
