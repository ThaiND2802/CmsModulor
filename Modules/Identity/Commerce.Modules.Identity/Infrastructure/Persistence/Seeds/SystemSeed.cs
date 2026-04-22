using Commerce.Modules.Identity.Domain;

namespace Commerce.Modules.Identity.Infrastructure.Persistence.Seeds;

internal static class SystemSeed
{
    private static readonly DateTime SeededAtUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private const string SystemActor = "system";
    internal const string AdminPassword = "Admin@123";
    internal const string ManagerPassword = "Manager@123";
    internal const string CashierPassword = "Cashier@123";

    internal static readonly Guid AdminUserId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    internal static readonly Guid ManagerUserId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    internal static readonly Guid CashierUserId = Guid.Parse("10000000-0000-0000-0000-000000000003");

    internal static readonly Guid AdminRoleId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    internal static readonly Guid ManagerRoleId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    internal static readonly Guid CashierRoleId = Guid.Parse("20000000-0000-0000-0000-000000000003");

    internal static readonly Guid PaymentCodReadPermissionId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    internal static readonly Guid PaymentCodCheckoutPermissionId = Guid.Parse("30000000-0000-0000-0000-000000000002");
    internal static readonly Guid OrderReadPermissionId = Guid.Parse("30000000-0000-0000-0000-000000000003");
    internal static readonly Guid OrderCreatePermissionId = Guid.Parse("30000000-0000-0000-0000-000000000004");
    internal static readonly Guid OrderUpdateStatusPermissionId = Guid.Parse("30000000-0000-0000-0000-000000000005");
    internal static readonly Guid ReportingViewPermissionId = Guid.Parse("30000000-0000-0000-0000-000000000006");
    internal static readonly Guid IdentityUsersReadPermissionId = Guid.Parse("30000000-0000-0000-0000-000000000007");
    internal static readonly Guid IdentityAuthorizationReadPermissionId = Guid.Parse("30000000-0000-0000-0000-000000000008");

    internal static readonly User AdminUser = new()
    {
        Id = AdminUserId,
        UserName = "admin",
        NormalizedUserName = "ADMIN",
        DisplayName = "System Administrator",
        Email = "admin@local.test",
        NormalizedEmail = "ADMIN@LOCAL.TEST",
        PasswordHash = "AQAAAAIAAYagAAAAELpUpm2DwqhM1M6LobcTMMcAxNXam+ecso5aOZ4h+L/th77+MbIvQ7HkPnKv4/FDHA==",
        PhoneNumber = "0000000001",
        IsActive = true,
        IsSystem = true,
        CreatedAtUtc = SeededAtUtc,
        CreatedBy = SystemActor,
        IsDeleted = false
    };

    internal static readonly User ManagerUser = new()
    {
        Id = ManagerUserId,
        UserName = "manager",
        NormalizedUserName = "MANAGER",
        DisplayName = "Store Manager",
        Email = "manager@local.test",
        NormalizedEmail = "MANAGER@LOCAL.TEST",
        PasswordHash = "AQAAAAIAAYagAAAAEDZ9RHaIQU/vmzwzxvlRIrrr02V8z06KgMjf0T4j5Sxngo0IvPT0YyQYW4mpn0/11Q==",
        PhoneNumber = "0000000002",
        IsActive = true,
        IsSystem = true,
        CreatedAtUtc = SeededAtUtc,
        CreatedBy = SystemActor,
        IsDeleted = false
    };

    internal static readonly User CashierUser = new()
    {
        Id = CashierUserId,
        UserName = "cashier",
        NormalizedUserName = "CASHIER",
        DisplayName = "Store Cashier",
        Email = "cashier@local.test",
        NormalizedEmail = "CASHIER@LOCAL.TEST",
        PasswordHash = "AQAAAAIAAYagAAAAEIZEJ+FKQ8ZmBbKpeOchLolGFSyJLgNhNasSqlVMWJgSR10QvS8fs/Mi98oh5TzfUw==",
        PhoneNumber = "0000000003",
        IsActive = true,
        IsSystem = true,
        CreatedAtUtc = SeededAtUtc,
        CreatedBy = SystemActor,
        IsDeleted = false
    };

    internal static readonly Role AdminRole = CreateRole(AdminRoleId, "ADMIN", "Administrator", "System-wide administration role");
    internal static readonly Role ManagerRole = CreateRole(ManagerRoleId, "MANAGER", "Manager", "Operational management role");
    internal static readonly Role CashierRole = CreateRole(CashierRoleId, "CASHIER", "Cashier", "Checkout and order creation role");

    internal static readonly Permission PaymentCodReadPermission = CreatePermission(
        PaymentCodReadPermissionId,
        "Payment.COD.Read",
        "Read COD payment capability",
        "Allows reading COD payment information",
        "Payment",
        "COD",
        "Payment",
        10);

    internal static readonly Permission PaymentCodCheckoutPermission = CreatePermission(
        PaymentCodCheckoutPermissionId,
        "Payment.COD.Checkout",
        "Checkout with COD",
        "Allows creating COD checkouts",
        "Payment",
        "COD",
        "Payment",
        20);

    internal static readonly Permission OrderReadPermission = CreatePermission(
        OrderReadPermissionId,
        "Order.Read",
        "Read orders",
        "Allows reading order records",
        "Order",
        null,
        "Order",
        30);

    internal static readonly Permission OrderCreatePermission = CreatePermission(
        OrderCreatePermissionId,
        "Order.Create",
        "Create orders",
        "Allows creating new orders",
        "Order",
        null,
        "Order",
        40);

    internal static readonly Permission OrderUpdateStatusPermission = CreatePermission(
        OrderUpdateStatusPermissionId,
        "Order.UpdateStatus",
        "Update order status",
        "Allows changing order status",
        "Order",
        null,
        "Order",
        50);

    internal static readonly Permission ReportingViewPermission = CreatePermission(
        ReportingViewPermissionId,
        "Reporting.View",
        "View reports",
        "Allows viewing reporting dashboards",
        "Reporting",
        null,
        "Reporting",
        60);

    internal static readonly Permission IdentityUsersReadPermission = CreatePermission(
        IdentityUsersReadPermissionId,
        "Identity.Users.Read",
        "Read identity users",
        "Allows reading identity user records",
        "Identity",
        "Users",
        "Identity",
        70);

    internal static readonly Permission IdentityAuthorizationReadPermission = CreatePermission(
        IdentityAuthorizationReadPermissionId,
        "Identity.Authorization.Read",
        "Read identity authorization overview",
        "Allows reading identity authorization configuration",
        "Identity",
        "Authorization",
        "Identity",
        80);

    internal static readonly UserRole AdminUserRole = CreateUserRole(
        Guid.Parse("40000000-0000-0000-0000-000000000001"),
        AdminUserId,
        AdminRoleId);

    internal static readonly UserRole ManagerUserRole = CreateUserRole(
        Guid.Parse("40000000-0000-0000-0000-000000000002"),
        ManagerUserId,
        ManagerRoleId);

    internal static readonly UserRole CashierUserRole = CreateUserRole(
        Guid.Parse("40000000-0000-0000-0000-000000000003"),
        CashierUserId,
        CashierRoleId);

    internal static readonly RolePermission[] RolePermissions =
    [
        CreateRolePermission(Guid.Parse("50000000-0000-0000-0000-000000000001"), AdminRoleId, PaymentCodReadPermissionId, PermissionEffect.Allow),
        CreateRolePermission(Guid.Parse("50000000-0000-0000-0000-000000000002"), AdminRoleId, PaymentCodCheckoutPermissionId, PermissionEffect.Allow),
        CreateRolePermission(Guid.Parse("50000000-0000-0000-0000-000000000003"), AdminRoleId, OrderReadPermissionId, PermissionEffect.Allow),
        CreateRolePermission(Guid.Parse("50000000-0000-0000-0000-000000000004"), AdminRoleId, OrderCreatePermissionId, PermissionEffect.Allow),
        CreateRolePermission(Guid.Parse("50000000-0000-0000-0000-000000000005"), AdminRoleId, OrderUpdateStatusPermissionId, PermissionEffect.Allow),
        CreateRolePermission(Guid.Parse("50000000-0000-0000-0000-000000000006"), AdminRoleId, ReportingViewPermissionId, PermissionEffect.Allow),
        CreateRolePermission(Guid.Parse("50000000-0000-0000-0000-000000000007"), ManagerRoleId, PaymentCodReadPermissionId, PermissionEffect.Allow),
        CreateRolePermission(Guid.Parse("50000000-0000-0000-0000-000000000008"), ManagerRoleId, OrderReadPermissionId, PermissionEffect.Allow),
        CreateRolePermission(Guid.Parse("50000000-0000-0000-0000-000000000009"), ManagerRoleId, OrderCreatePermissionId, PermissionEffect.Allow),
        CreateRolePermission(Guid.Parse("50000000-0000-0000-0000-000000000010"), ManagerRoleId, OrderUpdateStatusPermissionId, PermissionEffect.Allow),
        CreateRolePermission(Guid.Parse("50000000-0000-0000-0000-000000000011"), ManagerRoleId, ReportingViewPermissionId, PermissionEffect.Allow),
        CreateRolePermission(Guid.Parse("50000000-0000-0000-0000-000000000012"), CashierRoleId, PaymentCodReadPermissionId, PermissionEffect.Allow),
        CreateRolePermission(Guid.Parse("50000000-0000-0000-0000-000000000013"), CashierRoleId, OrderReadPermissionId, PermissionEffect.Allow),
        CreateRolePermission(Guid.Parse("50000000-0000-0000-0000-000000000014"), CashierRoleId, OrderCreatePermissionId, PermissionEffect.Allow),
        CreateRolePermission(Guid.Parse("50000000-0000-0000-0000-000000000015"), AdminRoleId, IdentityUsersReadPermissionId, PermissionEffect.Allow),
        CreateRolePermission(Guid.Parse("50000000-0000-0000-0000-000000000016"), AdminRoleId, IdentityAuthorizationReadPermissionId, PermissionEffect.Allow)
    ];

    internal static readonly UserPermission[] UserPermissions =
    [
        CreateUserPermission(Guid.Parse("60000000-0000-0000-0000-000000000001"), ManagerUserId, PaymentCodCheckoutPermissionId, PermissionEffect.Deny),
        CreateUserPermission(Guid.Parse("60000000-0000-0000-0000-000000000002"), CashierUserId, PaymentCodCheckoutPermissionId, PermissionEffect.Allow)
    ];

    internal static readonly (Guid UserId, string Password)[] SeedUserPasswords =
    [
        (AdminUserId, AdminPassword),
        (ManagerUserId, ManagerPassword),
        (CashierUserId, CashierPassword)
    ];

    private static Role CreateRole(Guid id, string code, string name, string description)
    {
        return new Role
        {
            Id = id,
            Code = code,
            Name = name,
            Description = description,
            IsActive = true,
            IsSystem = true,
            CreatedAtUtc = SeededAtUtc,
            CreatedBy = SystemActor,
            IsDeleted = false
        };
    }

    private static Permission CreatePermission(
        Guid id,
        string code,
        string name,
        string description,
        string module,
        string? feature,
        string groupName,
        int sortOrder)
    {
        return new Permission
        {
            Id = id,
            Code = code,
            Name = name,
            Description = description,
            Module = module,
            Feature = feature,
            GroupName = groupName,
            SortOrder = sortOrder,
            IsActive = true,
            IsSystem = true,
            CreatedAtUtc = SeededAtUtc,
            CreatedBy = SystemActor,
            IsDeleted = false
        };
    }

    private static UserRole CreateUserRole(Guid id, Guid userId, Guid roleId)
    {
        return new UserRole
        {
            Id = id,
            UserId = userId,
            RoleId = roleId,
            AssignedAtUtc = SeededAtUtc,
            AssignedBy = SystemActor,
            IsActive = true,
            CreatedAtUtc = SeededAtUtc,
            CreatedBy = SystemActor,
            IsDeleted = false
        };
    }

    private static RolePermission CreateRolePermission(Guid id, Guid roleId, Guid permissionId, PermissionEffect effect)
    {
        return new RolePermission
        {
            Id = id,
            RoleId = roleId,
            PermissionId = permissionId,
            Effect = effect,
            AssignedAtUtc = SeededAtUtc,
            AssignedBy = SystemActor,
            CreatedAtUtc = SeededAtUtc,
            CreatedBy = SystemActor,
            IsDeleted = false
        };
    }

    private static UserPermission CreateUserPermission(Guid id, Guid userId, Guid permissionId, PermissionEffect effect)
    {
        return new UserPermission
        {
            Id = id,
            UserId = userId,
            PermissionId = permissionId,
            Effect = effect,
            AssignedAtUtc = SeededAtUtc,
            AssignedBy = SystemActor,
            CreatedAtUtc = SeededAtUtc,
            CreatedBy = SystemActor,
            IsDeleted = false
        };
    }
}
