using Commerce.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Infrastructure.Persistence.Seeds;

internal static class ModelBuilderExtensions
{
    internal static void ApplyIdentityAuthorizationSeed(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<User>()
            .HasData(SystemSeed.AdminUser, SystemSeed.ManagerUser, SystemSeed.CashierUser);

        modelBuilder.Entity<Role>()
            .HasData(SystemSeed.AdminRole, SystemSeed.ManagerRole, SystemSeed.CashierRole);

        modelBuilder.Entity<Permission>()
            .HasData(
                SystemSeed.PaymentCodReadPermission,
                SystemSeed.PaymentCodCheckoutPermission,
                SystemSeed.OrderReadPermission,
                SystemSeed.OrderCreatePermission,
                SystemSeed.OrderUpdateStatusPermission,
                SystemSeed.ReportingViewPermission,
                SystemSeed.IdentityUsersReadPermission,
                SystemSeed.IdentityAuthorizationReadPermission,
                SystemSeed.IdentityMeReadPermission,
                SystemSeed.IdentityMyPermissionsReadPermission,
                SystemSeed.IdentityUsersCreatePermission,
                SystemSeed.IdentityUsersUpdatePermission,
                SystemSeed.IdentityUsersDeletePermission,
                SystemSeed.IdentityRolesReadPermission,
                SystemSeed.IdentityRolesCreatePermission,
                SystemSeed.IdentityRolesUpdatePermission,
                SystemSeed.IdentityRolesDeletePermission,
                SystemSeed.IdentityPermissionsReadPermission,
                SystemSeed.IdentityPermissionsCreatePermission,
                SystemSeed.IdentityPermissionsUpdatePermission,
                SystemSeed.IdentityPermissionsDeletePermission,
                SystemSeed.IdentityUserRolesReadPermission,
                SystemSeed.IdentityUserRolesAssignPermission,
                SystemSeed.IdentityUserPermissionsReadPermission,
                SystemSeed.IdentityUserPermissionsAssignPermission);

        modelBuilder.Entity<UserRole>()
            .HasData(SystemSeed.AdminUserRole, SystemSeed.ManagerUserRole, SystemSeed.CashierUserRole);

        modelBuilder.Entity<RolePermission>()
            .HasData(SystemSeed.RolePermissions);

        modelBuilder.Entity<UserPermission>()
            .HasData(SystemSeed.UserPermissions);
    }
}
