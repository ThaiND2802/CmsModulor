using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Commerce.Modules.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialInventorySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inventory_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    on_hand_quantity = table.Column<int>(type: "integer", nullable: false),
                    reserved_quantity = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_items", x => x.id);
                    table.CheckConstraint("ck_inventory_items_on_hand_quantity_non_negative", "on_hand_quantity >= 0");
                    table.CheckConstraint("ck_inventory_items_reserved_quantity_non_negative", "reserved_quantity >= 0");
                    table.CheckConstraint("ck_inventory_items_reserved_quantity_within_on_hand", "reserved_quantity <= on_hand_quantity");
                });

            migrationBuilder.CreateTable(
                name: "inventory_stock_reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    released_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_stock_reservations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_stock_movements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    movement_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    quantity_delta = table.Column<int>(type: "integer", nullable: false),
                    on_hand_after = table.Column<int>(type: "integer", nullable: false),
                    reserved_after = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    reference_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_stock_movements", x => x.id);
                    table.CheckConstraint("ck_inventory_stock_movements_on_hand_after_non_negative", "on_hand_after >= 0");
                    table.CheckConstraint("ck_inventory_stock_movements_reserved_after_non_negative", "reserved_after >= 0");
                    table.CheckConstraint("ck_inventory_stock_movements_reserved_after_within_on_hand", "reserved_after <= on_hand_after");
                    table.ForeignKey(
                        name: "FK_inventory_stock_movements_inventory_items_inventory_item_id",
                        column: x => x.inventory_item_id,
                        principalTable: "inventory_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inventory_stock_reservation_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_reservation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_stock_reservation_items", x => x.id);
                    table.CheckConstraint("ck_inventory_stock_reservation_items_quantity_positive", "quantity > 0");
                    table.ForeignKey(
                        name: "FK_inventory_stock_reservation_items_inventory_items_inventory~",
                        column: x => x.inventory_item_id,
                        principalTable: "inventory_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventory_stock_reservation_items_inventory_stock_reservati~",
                        column: x => x.stock_reservation_id,
                        principalTable: "inventory_stock_reservations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inventory_items_sku",
                table: "inventory_items",
                column: "sku");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_items_variant_id",
                table: "inventory_items",
                column: "variant_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inventory_stock_movements_inventory_item_id",
                table: "inventory_stock_movements",
                column: "inventory_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_stock_movements_reference_type_reference_id",
                table: "inventory_stock_movements",
                columns: new[] { "reference_type", "reference_id" });

            migrationBuilder.CreateIndex(
                name: "IX_inventory_stock_reservation_items_inventory_item_id",
                table: "inventory_stock_reservation_items",
                column: "inventory_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_stock_reservation_items_stock_reservation_id_inve~",
                table: "inventory_stock_reservation_items",
                columns: new[] { "stock_reservation_id", "inventory_item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inventory_stock_reservations_order_id",
                table: "inventory_stock_reservations",
                column: "order_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventory_stock_movements");

            migrationBuilder.DropTable(
                name: "inventory_stock_reservation_items");

            migrationBuilder.DropTable(
                name: "inventory_items");

            migrationBuilder.DropTable(
                name: "inventory_stock_reservations");
        }
    }
}
