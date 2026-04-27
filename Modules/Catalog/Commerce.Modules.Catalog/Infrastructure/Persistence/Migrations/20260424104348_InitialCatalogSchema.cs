using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Commerce.Modules.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalogSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "catalog_brands",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    logo_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    website = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_catalog_brands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "catalog_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_catalog_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_catalog_categories_catalog_categories_parent_id",
                        column: x => x.parent_id,
                        principalTable: "catalog_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "catalog_product_attributes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_product_attributes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "catalog_products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    slug = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    short_description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    brand_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_featured = table.Column<bool>(type: "boolean", nullable: false),
                    tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_catalog_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_catalog_products_catalog_brands_brand_id",
                        column: x => x.brand_id,
                        principalTable: "catalog_brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_catalog_products_catalog_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "catalog_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "catalog_product_attribute_values",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attribute_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_product_attribute_values", x => x.id);
                    table.ForeignKey(
                        name: "FK_catalog_product_attribute_values_catalog_product_attributes~",
                        column: x => x.attribute_id,
                        principalTable: "catalog_product_attributes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_catalog_product_attribute_values_catalog_products_product_id",
                        column: x => x.product_id,
                        principalTable: "catalog_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "catalog_product_media",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    alt_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    media_type = table.Column<int>(type: "integer", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_product_media", x => x.id);
                    table.ForeignKey(
                        name: "FK_catalog_product_media_catalog_products_product_id",
                        column: x => x.product_id,
                        principalTable: "catalog_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "catalog_product_variants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    compare_at_price = table.Column<decimal>(type: "numeric", nullable: true),
                    stock_quantity = table.Column<int>(type: "integer", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_catalog_product_variants", x => x.id);
                    table.ForeignKey(
                        name: "FK_catalog_product_variants_catalog_products_product_id",
                        column: x => x.product_id,
                        principalTable: "catalog_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "catalog_product_variant_options",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attribute_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_product_variant_options", x => x.id);
                    table.ForeignKey(
                        name: "FK_catalog_product_variant_options_catalog_product_attributes_~",
                        column: x => x.attribute_id,
                        principalTable: "catalog_product_attributes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_catalog_product_variant_options_catalog_product_variants_va~",
                        column: x => x.variant_id,
                        principalTable: "catalog_product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_catalog_brands_slug",
                table: "catalog_brands",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_catalog_categories_parent_id",
                table: "catalog_categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_categories_slug",
                table: "catalog_categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_catalog_product_attribute_values_attribute_id",
                table: "catalog_product_attribute_values",
                column: "attribute_id");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_product_attribute_values_product_id_attribute_id",
                table: "catalog_product_attribute_values",
                columns: new[] { "product_id", "attribute_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_catalog_product_attributes_name",
                table: "catalog_product_attributes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_catalog_product_media_product_id",
                table: "catalog_product_media",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_product_variant_options_attribute_id",
                table: "catalog_product_variant_options",
                column: "attribute_id");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_product_variant_options_variant_id",
                table: "catalog_product_variant_options",
                column: "variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_product_variants_product_id",
                table: "catalog_product_variants",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_product_variants_sku",
                table: "catalog_product_variants",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_catalog_products_brand_id",
                table: "catalog_products",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_products_category_id",
                table: "catalog_products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_products_sku",
                table: "catalog_products",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_catalog_products_slug",
                table: "catalog_products",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "catalog_product_attribute_values");

            migrationBuilder.DropTable(
                name: "catalog_product_media");

            migrationBuilder.DropTable(
                name: "catalog_product_variant_options");

            migrationBuilder.DropTable(
                name: "catalog_product_attributes");

            migrationBuilder.DropTable(
                name: "catalog_product_variants");

            migrationBuilder.DropTable(
                name: "catalog_products");

            migrationBuilder.DropTable(
                name: "catalog_brands");

            migrationBuilder.DropTable(
                name: "catalog_categories");
        }
    }
}
