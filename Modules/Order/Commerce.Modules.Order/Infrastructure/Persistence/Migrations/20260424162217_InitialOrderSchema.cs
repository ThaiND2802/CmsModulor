using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Commerce.Modules.Order.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialOrderSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "order_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    customer_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    customer_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    shipping_full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    shipping_phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    shipping_address_line1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    shipping_address_line2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    shipping_city = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    shipping_state = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    shipping_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    shipping_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    billing_full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    billing_phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    billing_address_line1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    billing_address_line2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    billing_city = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    billing_state = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    billing_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    billing_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    subtotal_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    shipping_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    cancel_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    product_sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    variant_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    unit_price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_items_order_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "order_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    to_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    changed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    changed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_status_history_order_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "order_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_orders_order_number",
                table: "order_orders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_status_history_order_id",
                table: "order_status_history",
                column: "order_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "order_status_history");

            migrationBuilder.DropTable(
                name: "order_orders");
        }
    }
}
