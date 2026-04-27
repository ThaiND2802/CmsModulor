using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commerce.Modules.Sale.Domain;

[Table("sale_status_history")]
public sealed class SaleStatusHistory
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("sale_id")]
    public Guid SaleId { get; set; }

    [Column("from_status")]
    public SaleStatus? FromStatus { get; set; }

    [Column("to_status")]
    public SaleStatus ToStatus { get; set; }

    [Column("note")]
    [MaxLength(1000)]
    public string? Note { get; set; }

    [Column("changed_by")]
    [MaxLength(100)]
    public string? ChangedBy { get; set; }

    [Column("changed_at_utc")]
    public DateTime ChangedAtUtc { get; set; }

    public Sale Sale { get; set; } = null!;
}
