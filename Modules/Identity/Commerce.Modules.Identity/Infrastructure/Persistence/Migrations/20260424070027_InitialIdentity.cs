using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Commerce.Modules.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "identity_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    feature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    group_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_identity_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "identity_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_identity_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "identity_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    normalized_user_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
                    last_login_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_identity_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "identity_role_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    effect = table.Column<int>(type: "integer", nullable: false),
                    assigned_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    assigned_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_identity_role_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_identity_role_permissions_identity_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "identity_permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_identity_role_permissions_identity_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "identity_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    effect = table.Column<int>(type: "integer", nullable: false),
                    assigned_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    assigned_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_identity_user_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_identity_user_permissions_identity_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "identity_permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_identity_user_permissions_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    assigned_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_identity_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_identity_user_roles_identity_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "identity_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_identity_user_roles_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    refresh_token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_used_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identity_user_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_identity_user_sessions_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "identity_permissions",
                columns: new[] { "id", "code", "created_at_utc", "created_by", "deleted_at_utc", "deleted_by", "description", "feature", "group_name", "is_active", "is_deleted", "is_system", "module", "name", "sort_order", "updated_at_utc", "updated_by" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000001"), "PAYMENT.COD.READ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows reading COD payment information", "COD", "Payment", true, false, true, "Payment", "Read COD payment capability", 10, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000002"), "PAYMENT.COD.CHECKOUT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows creating COD checkouts", "COD", "Payment", true, false, true, "Payment", "Checkout with COD", 20, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000003"), "ORDER.READ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows reading order records", null, "Order", true, false, true, "Order", "Read orders", 30, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000004"), "ORDER.CREATE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows creating new orders", null, "Order", true, false, true, "Order", "Create orders", 40, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000005"), "ORDER.UPDATESTATUS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows changing order status", null, "Order", true, false, true, "Order", "Update order status", 50, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000006"), "REPORTING.VIEW", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows viewing reporting dashboards", null, "Reporting", true, false, true, "Reporting", "View reports", 60, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000007"), "IDENTITY.USERS.READ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows reading identity user records", "Users", "Identity", true, false, true, "Identity", "Read identity users", 70, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000008"), "IDENTITY.AUTHORIZATION.READ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows reading identity authorization configuration", "Authorization", "Identity", true, false, true, "Identity", "Read identity authorization overview", 80, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000009"), "IDENTITY.ME.READ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows reading the current authenticated user profile", "Me", "Identity", true, false, true, "Identity", "Read current identity user", 90, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000010"), "IDENTITY.MYPERMISSIONS.READ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows reading the current authenticated user permission set", "MyPermissions", "Identity", true, false, true, "Identity", "Read current identity permissions", 100, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000011"), "IDENTITY.USERS.CREATE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows creating identity user records", "Users", "Identity", true, false, true, "Identity", "Create identity users", 110, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000012"), "IDENTITY.USERS.UPDATE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows updating identity user records", "Users", "Identity", true, false, true, "Identity", "Update identity users", 120, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000013"), "IDENTITY.USERS.DELETE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows deleting identity user records", "Users", "Identity", true, false, true, "Identity", "Delete identity users", 130, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000014"), "IDENTITY.ROLES.READ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows reading identity role records", "Roles", "Identity", true, false, true, "Identity", "Read identity roles", 140, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000015"), "IDENTITY.ROLES.CREATE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows creating identity role records", "Roles", "Identity", true, false, true, "Identity", "Create identity roles", 150, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000016"), "IDENTITY.ROLES.UPDATE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows updating identity role records", "Roles", "Identity", true, false, true, "Identity", "Update identity roles", 160, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000017"), "IDENTITY.ROLES.DELETE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows deleting identity role records", "Roles", "Identity", true, false, true, "Identity", "Delete identity roles", 170, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000018"), "IDENTITY.PERMISSIONS.READ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows reading identity permission records", "Permissions", "Identity", true, false, true, "Identity", "Read identity permissions", 180, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000019"), "IDENTITY.PERMISSIONS.CREATE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows creating identity permission records", "Permissions", "Identity", true, false, true, "Identity", "Create identity permissions", 190, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000020"), "IDENTITY.PERMISSIONS.UPDATE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows updating identity permission records", "Permissions", "Identity", true, false, true, "Identity", "Update identity permissions", 200, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000021"), "IDENTITY.PERMISSIONS.DELETE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows deleting identity permission records", "Permissions", "Identity", true, false, true, "Identity", "Delete identity permissions", 210, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000022"), "IDENTITY.USERROLES.READ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows reading identity user role assignments", "UserRoles", "Identity", true, false, true, "Identity", "Read identity user roles", 220, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000023"), "IDENTITY.USERROLES.ASSIGN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows assigning identity user role memberships", "UserRoles", "Identity", true, false, true, "Identity", "Assign identity user roles", 230, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000024"), "IDENTITY.USERPERMISSIONS.READ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows reading identity user direct permission assignments", "UserPermissions", "Identity", true, false, true, "Identity", "Read identity user permissions", 240, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000025"), "IDENTITY.USERPERMISSIONS.ASSIGN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows assigning identity user direct permissions", "UserPermissions", "Identity", true, false, true, "Identity", "Assign identity user permissions", 250, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000026"), "IDENTITY.ROLEPERMISSIONS.READ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows reading identity role permission assignments", "RolePermissions", "Identity", true, false, true, "Identity", "Read identity role permissions", 260, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000027"), "IDENTITY.ROLEPERMISSIONS.ASSIGN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows assigning identity role permissions", "RolePermissions", "Identity", true, false, true, "Identity", "Assign identity role permissions", 270, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000028"), "IDENTITY.USERS.RESETPASSWORD", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Allows resetting identity user passwords", "Users", "Identity", true, false, true, "Identity", "Reset identity user passwords", 280, null, null }
                });

            migrationBuilder.InsertData(
                table: "identity_roles",
                columns: new[] { "id", "code", "created_at_utc", "created_by", "deleted_at_utc", "deleted_by", "description", "is_active", "is_deleted", "is_system", "name", "updated_at_utc", "updated_by" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), "ADMIN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "System-wide administration role", true, false, true, "Administrator", null, null },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "MANAGER", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Operational management role", true, false, true, "Manager", null, null },
                    { new Guid("20000000-0000-0000-0000-000000000003"), "CASHIER", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Checkout and order creation role", true, false, true, "Cashier", null, null }
                });

            migrationBuilder.InsertData(
                table: "identity_users",
                columns: new[] { "id", "created_at_utc", "created_by", "deleted_at_utc", "deleted_by", "display_name", "email", "is_active", "is_deleted", "is_system", "last_login_at_utc", "normalized_email", "normalized_user_name", "password_hash", "phone_number", "updated_at_utc", "updated_by", "user_name" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "System Administrator", "admin@local.test", true, false, true, null, "ADMIN@LOCAL.TEST", "ADMIN", null, "0000000001", null, null, "admin" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Store Manager", "manager@local.test", true, false, true, null, "MANAGER@LOCAL.TEST", "MANAGER", null, "0000000002", null, null, "manager" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Store Cashier", "cashier@local.test", true, false, true, null, "CASHIER@LOCAL.TEST", "CASHIER", null, "0000000003", null, null, "cashier" }
                });

            migrationBuilder.InsertData(
                table: "identity_role_permissions",
                columns: new[] { "id", "assigned_at_utc", "assigned_by", "created_at_utc", "created_by", "deleted_at_utc", "deleted_by", "effect", "is_deleted", "permission_id", "role_id", "updated_at_utc", "updated_by" },
                values: new object[,]
                {
                    { new Guid("50000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000001"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000002"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000003"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000004"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000005"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000006"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000006"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000007"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000001"), new Guid("20000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000008"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000003"), new Guid("20000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000009"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000004"), new Guid("20000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000010"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000005"), new Guid("20000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000011"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000006"), new Guid("20000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000012"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000001"), new Guid("20000000-0000-0000-0000-000000000003"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000013"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000003"), new Guid("20000000-0000-0000-0000-000000000003"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000014"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000004"), new Guid("20000000-0000-0000-0000-000000000003"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000015"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000007"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000016"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000008"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000017"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000009"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000018"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000010"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000019"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000011"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000020"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000012"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000021"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000013"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000022"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000014"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000023"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000015"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000024"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000016"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000025"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000017"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000026"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000018"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000027"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000019"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000028"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000020"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000029"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000021"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000030"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000022"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000031"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000023"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000032"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000024"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000033"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000025"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000034"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000026"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000035"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000027"), new Guid("20000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("50000000-0000-0000-0000-000000000036"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, false, new Guid("30000000-0000-0000-0000-000000000028"), new Guid("20000000-0000-0000-0000-000000000001"), null, null }
                });

            migrationBuilder.InsertData(
                table: "identity_user_permissions",
                columns: new[] { "id", "assigned_at_utc", "assigned_by", "created_at_utc", "created_by", "deleted_at_utc", "deleted_by", "effect", "expires_at_utc", "is_deleted", "permission_id", "updated_at_utc", "updated_by", "user_id" },
                values: new object[,]
                {
                    { new Guid("60000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 2, null, false, new Guid("30000000-0000-0000-0000-000000000002"), null, null, new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("60000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, 1, null, false, new Guid("30000000-0000-0000-0000-000000000002"), null, null, new Guid("10000000-0000-0000-0000-000000000003") }
                });

            migrationBuilder.InsertData(
                table: "identity_user_roles",
                columns: new[] { "id", "assigned_at_utc", "assigned_by", "created_at_utc", "created_by", "deleted_at_utc", "deleted_by", "expires_at_utc", "is_active", "is_deleted", "role_id", "updated_at_utc", "updated_by", "user_id" },
                values: new object[,]
                {
                    { new Guid("40000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, null, true, false, new Guid("20000000-0000-0000-0000-000000000001"), null, null, new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("40000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, null, true, false, new Guid("20000000-0000-0000-0000-000000000002"), null, null, new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("40000000-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, null, true, false, new Guid("20000000-0000-0000-0000-000000000003"), null, null, new Guid("10000000-0000-0000-0000-000000000003") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_identity_permissions_code",
                table: "identity_permissions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_identity_permissions_group_name",
                table: "identity_permissions",
                column: "group_name");

            migrationBuilder.CreateIndex(
                name: "IX_identity_permissions_module_feature",
                table: "identity_permissions",
                columns: new[] { "module", "feature" });

            migrationBuilder.CreateIndex(
                name: "IX_identity_role_permissions_permission_id",
                table: "identity_role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "IX_identity_role_permissions_role_id_permission_id",
                table: "identity_role_permissions",
                columns: new[] { "role_id", "permission_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_identity_roles_code",
                table: "identity_roles",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_identity_user_permissions_permission_id",
                table: "identity_user_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "IX_identity_user_permissions_user_id_permission_id",
                table: "identity_user_permissions",
                columns: new[] { "user_id", "permission_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_identity_user_roles_role_id",
                table: "identity_user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_identity_user_roles_user_id_role_id",
                table: "identity_user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_identity_user_sessions_refresh_token_hash",
                table: "identity_user_sessions",
                column: "refresh_token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_identity_user_sessions_user_id",
                table: "identity_user_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_identity_users_normalized_email",
                table: "identity_users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "IX_identity_users_normalized_user_name",
                table: "identity_users",
                column: "normalized_user_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "identity_role_permissions");

            migrationBuilder.DropTable(
                name: "identity_user_permissions");

            migrationBuilder.DropTable(
                name: "identity_user_roles");

            migrationBuilder.DropTable(
                name: "identity_user_sessions");

            migrationBuilder.DropTable(
                name: "identity_permissions");

            migrationBuilder.DropTable(
                name: "identity_roles");

            migrationBuilder.DropTable(
                name: "identity_users");
        }
    }
}
