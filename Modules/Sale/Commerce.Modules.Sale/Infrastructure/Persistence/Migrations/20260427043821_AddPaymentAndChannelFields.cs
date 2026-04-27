using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Commerce.Modules.Sale.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentAndChannelFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "base_discount_amount",
                table: "sale_sales",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "base_shipping_amount",
                table: "sale_sales",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "channel",
                table: "sale_sales",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "coupon_code",
                table: "sale_sales",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "coupon_discount_amount",
                table: "sale_sales",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "coupon_shipping_discount_amount",
                table: "sale_sales",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "coupon_type",
                table: "sale_sales",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "coupon_value",
                table: "sale_sales",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "expires_at_utc",
                table: "sale_sales",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "paid_amount",
                table: "sale_sales",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "paid_at_utc",
                table: "sale_sales",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "payment_initiated_at_utc",
                table: "sale_sales",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "sale_sales",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "payment_reference",
                table: "sale_sales",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_status",
                table: "sale_sales",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "overridden_at_utc",
                table: "sale_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "overridden_by",
                table: "sale_items",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "override_price",
                table: "sale_items",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "override_reason",
                table: "sale_items",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "base_discount_amount",
                table: "sale_sales");

            migrationBuilder.DropColumn(
                name: "base_shipping_amount",
                table: "sale_sales");

            migrationBuilder.DropColumn(
                name: "channel",
                table: "sale_sales");

            migrationBuilder.DropColumn(
                name: "coupon_code",
                table: "sale_sales");

            migrationBuilder.DropColumn(
                name: "coupon_discount_amount",
                table: "sale_sales");

            migrationBuilder.DropColumn(
                name: "coupon_shipping_discount_amount",
                table: "sale_sales");

            migrationBuilder.DropColumn(
                name: "coupon_type",
                table: "sale_sales");

            migrationBuilder.DropColumn(
                name: "coupon_value",
                table: "sale_sales");

            migrationBuilder.DropColumn(
                name: "expires_at_utc",
                table: "sale_sales");

            migrationBuilder.DropColumn(
                name: "paid_amount",
                table: "sale_sales");

            migrationBuilder.DropColumn(
                name: "paid_at_utc",
                table: "sale_sales");

            migrationBuilder.DropColumn(
                name: "payment_initiated_at_utc",
                table: "sale_sales");

            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "sale_sales");

            migrationBuilder.DropColumn(
                name: "payment_reference",
                table: "sale_sales");

            migrationBuilder.DropColumn(
                name: "payment_status",
                table: "sale_sales");

            migrationBuilder.DropColumn(
                name: "overridden_at_utc",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "overridden_by",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "override_price",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "override_reason",
                table: "sale_items");
        }
    }
}
