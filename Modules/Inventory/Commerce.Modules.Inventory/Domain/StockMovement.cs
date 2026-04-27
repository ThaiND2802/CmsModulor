using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commerce.Modules.Inventory.Domain;

[Table("inventory_stock_movements")]
public sealed class StockMovement
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("inventory_item_id")]
    public Guid InventoryItemId { get; set; }

    [Column("movement_type")]
    public StockMovementType MovementType { get; set; }

    [Column("quantity_delta")]
    public int QuantityDelta { get; set; }

    [Column("on_hand_after")]
    public int OnHandAfter { get; set; }

    [Column("reserved_after")]
    public int ReservedAfter { get; set; }

    [Required]
    [Column("reason")]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [Column("reference_type")]
    [MaxLength(100)]
    public string? ReferenceType { get; set; }

    [Column("reference_id")]
    public Guid? ReferenceId { get; set; }

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("created_by_user_id")]
    public Guid? CreatedByUserId { get; set; }

    public InventoryItem InventoryItem { get; set; } = null!;

    public static StockMovement Create(
        Guid inventoryItemId,
        StockMovementType movementType,
        int quantityDelta,
        int onHandAfter,
        int reservedAfter,
        string reason,
        string? referenceType = null,
        Guid? referenceId = null,
        Guid? createdByUserId = null)
    {
        if (inventoryItemId == Guid.Empty)
        {
            throw new ArgumentException("Inventory item ID is required.", nameof(inventoryItemId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (onHandAfter < 0)
        {
            throw new InvalidOperationException("On-hand quantity cannot be negative.");
        }

        if (reservedAfter < 0)
        {
            throw new InvalidOperationException("Reserved quantity cannot be negative.");
        }

        if (reservedAfter > onHandAfter)
        {
            throw new InvalidOperationException("Reserved quantity cannot exceed on-hand quantity.");
        }

        return new StockMovement
        {
            Id = Guid.NewGuid(),
            InventoryItemId = inventoryItemId,
            MovementType = movementType,
            QuantityDelta = quantityDelta,
            OnHandAfter = onHandAfter,
            ReservedAfter = reservedAfter,
            Reason = reason.Trim(),
            ReferenceType = string.IsNullOrWhiteSpace(referenceType) ? null : referenceType.Trim(),
            ReferenceId = referenceId,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };
    }
}
