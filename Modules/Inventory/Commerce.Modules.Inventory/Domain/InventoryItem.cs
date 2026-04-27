using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommerceCore.Application.Abstractions;

namespace Commerce.Modules.Inventory.Domain;

[Table("inventory_items")]
public sealed class InventoryItem : IAuditableEntity
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("variant_id")]
    public Guid VariantId { get; set; }

    [Required]
    [Column("sku")]
    [MaxLength(100)]
    public string Sku { get; set; } = string.Empty;

    [Column("on_hand_quantity")]
    public int OnHandQuantity { get; set; }

    [Column("reserved_quantity")]
    public int ReservedQuantity { get; set; }

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

    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public ICollection<StockReservationItem> ReservationItems { get; set; } = new List<StockReservationItem>();

    [NotMapped]
    public int AvailableQuantity => OnHandQuantity - ReservedQuantity;

    public static InventoryItem Create(Guid variantId, string sku, int initialQuantity = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        if (variantId == Guid.Empty)
        {
            throw new ArgumentException("Variant ID is required.", nameof(variantId));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(initialQuantity);

        return new InventoryItem
        {
            Id = Guid.NewGuid(),
            VariantId = variantId,
            Sku = sku.Trim(),
            OnHandQuantity = initialQuantity,
            ReservedQuantity = 0
        };
    }

    public void Receive(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        OnHandQuantity += quantity;
    }

    public void Adjust(int quantityDelta)
    {
        if (quantityDelta == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantityDelta), "Adjustment quantity cannot be zero.");
        }

        var updatedOnHandQuantity = OnHandQuantity + quantityDelta;
        if (updatedOnHandQuantity < 0)
        {
            throw new InvalidOperationException("On-hand quantity cannot be negative.");
        }

        if (updatedOnHandQuantity < ReservedQuantity)
        {
            throw new InvalidOperationException("On-hand quantity cannot be less than reserved quantity.");
        }

        OnHandQuantity = updatedOnHandQuantity;
    }

    public void Reserve(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        if (AvailableQuantity < quantity)
        {
            throw new InvalidOperationException("Available quantity is insufficient for reservation.");
        }

        ReservedQuantity += quantity;
    }

    public void Release(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        if (ReservedQuantity < quantity)
        {
            throw new InvalidOperationException("Reserved quantity cannot be negative.");
        }

        ReservedQuantity -= quantity;
    }
}
