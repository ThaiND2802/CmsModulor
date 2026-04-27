using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commerce.Modules.Inventory.Domain;

[Table("inventory_stock_reservation_items")]
public sealed class StockReservationItem
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("stock_reservation_id")]
    public Guid StockReservationId { get; set; }

    [Column("inventory_item_id")]
    public Guid InventoryItemId { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    public StockReservation StockReservation { get; set; } = null!;

    public InventoryItem InventoryItem { get; set; } = null!;

    public static StockReservationItem Create(Guid stockReservationId, Guid inventoryItemId, int quantity)
    {
        if (stockReservationId == Guid.Empty)
        {
            throw new ArgumentException("Stock reservation ID is required.", nameof(stockReservationId));
        }

        if (inventoryItemId == Guid.Empty)
        {
            throw new ArgumentException("Inventory item ID is required.", nameof(inventoryItemId));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        return new StockReservationItem
        {
            Id = Guid.NewGuid(),
            StockReservationId = stockReservationId,
            InventoryItemId = inventoryItemId,
            Quantity = quantity
        };
    }
}
