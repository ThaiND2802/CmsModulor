using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Commerce.Modules.Sale.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleOrderTrackingAndSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "order_id",
                table: "sale_sales",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "order_id",
                table: "sale_sales");
        }
    }
}
