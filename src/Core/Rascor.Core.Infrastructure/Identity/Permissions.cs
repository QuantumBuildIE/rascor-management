namespace Rascor.Core.Infrastructure.Identity;

/// <summary>
/// Static class containing all permission constants for the application
/// Simplified permission naming convention: {Module}.{PermissionName}
/// </summary>
public static class Permissions
{
    /// <summary>
    /// Stock Management module permissions
    /// </summary>
    public static class StockManagement
    {
        public const string View = "StockManagement.View";
        public const string CreateOrders = "StockManagement.CreateOrders";
        public const string ApproveOrders = "StockManagement.ApproveOrders";
        public const string ViewCostings = "StockManagement.ViewCostings";
        public const string ManageProducts = "StockManagement.ManageProducts";
        public const string ManageSuppliers = "StockManagement.ManageSuppliers";
        public const string ReceiveGoods = "StockManagement.ReceiveGoods";
        public const string Stocktake = "StockManagement.Stocktake";
        public const string Admin = "StockManagement.Admin";
    }

    /// <summary>
    /// Site Attendance module permissions (placeholder for future module)
    /// </summary>
    public static class SiteAttendance
    {
        public const string View = "SiteAttendance.View";
        public const string MarkAttendance = "SiteAttendance.MarkAttendance";
        public const string Admin = "SiteAttendance.Admin";
    }

    /// <summary>
    /// Proposals module permissions
    /// </summary>
    public static class Proposals
    {
        public const string View = "Proposals.View";
        public const string Create = "Proposals.Create";
        public const string Edit = "Proposals.Edit";
        public const string Delete = "Proposals.Delete";
        public const string Submit = "Proposals.Submit";
        public const string Approve = "Proposals.Approve";
        public const string ViewCostings = "Proposals.ViewCostings";
        public const string Admin = "Proposals.Admin";
    }

    /// <summary>
    /// Toolbox Talks module permissions
    /// </summary>
    public static class ToolboxTalks
    {
        public const string View = "ToolboxTalks.View";
        public const string Create = "ToolboxTalks.Create";
        public const string Edit = "ToolboxTalks.Edit";
        public const string Delete = "ToolboxTalks.Delete";
        public const string Schedule = "ToolboxTalks.Schedule";
        public const string ViewReports = "ToolboxTalks.ViewReports";
        public const string Admin = "ToolboxTalks.Admin";
    }

    /// <summary>
    /// RAMS (Risk Assessment Method Statement) module permissions
    /// </summary>
    public static class Rams
    {
        public const string View = "Rams.View";
        public const string Create = "Rams.Create";
        public const string Edit = "Rams.Edit";
        public const string Delete = "Rams.Delete";
        public const string Submit = "Rams.Submit";
        public const string Approve = "Rams.Approve";
        public const string Admin = "Rams.Admin";
    }

    /// <summary>
    /// Core module permissions (Sites, Employees, Companies, Users, Roles)
    /// </summary>
    public static class Core
    {
        public const string ManageSites = "Core.ManageSites";
        public const string ManageEmployees = "Core.ManageEmployees";
        public const string ManageCompanies = "Core.ManageCompanies";
        public const string ManageUsers = "Core.ManageUsers";
        public const string ManageRoles = "Core.ManageRoles";
        public const string Admin = "Core.Admin";
    }

    /// <summary>
    /// Get all permissions as a list (useful for seeding and policy registration)
    /// </summary>
    public static IEnumerable<string> GetAll()
    {
        return new[]
        {
            // Stock Management permissions
            StockManagement.View,
            StockManagement.CreateOrders,
            StockManagement.ApproveOrders,
            StockManagement.ViewCostings,
            StockManagement.ManageProducts,
            StockManagement.ManageSuppliers,
            StockManagement.ReceiveGoods,
            StockManagement.Stocktake,
            StockManagement.Admin,

            // Site Attendance permissions
            SiteAttendance.View,
            SiteAttendance.MarkAttendance,
            SiteAttendance.Admin,

            // Proposals permissions
            Proposals.View,
            Proposals.Create,
            Proposals.Edit,
            Proposals.Delete,
            Proposals.Submit,
            Proposals.Approve,
            Proposals.ViewCostings,
            Proposals.Admin,

            // Toolbox Talks permissions
            ToolboxTalks.View,
            ToolboxTalks.Create,
            ToolboxTalks.Edit,
            ToolboxTalks.Delete,
            ToolboxTalks.Schedule,
            ToolboxTalks.ViewReports,
            ToolboxTalks.Admin,

            // RAMS permissions
            Rams.View,
            Rams.Create,
            Rams.Edit,
            Rams.Delete,
            Rams.Submit,
            Rams.Approve,
            Rams.Admin,

            // Core permissions
            Core.ManageSites,
            Core.ManageEmployees,
            Core.ManageCompanies,
            Core.ManageUsers,
            Core.ManageRoles,
            Core.Admin
        };
    }

    /// <summary>
    /// Get permissions by module name
    /// </summary>
    public static IEnumerable<string> GetByModule(string moduleName)
    {
        return GetAll().Where(p => p.StartsWith(moduleName + "."));
    }

    /// <summary>
    /// Get the module name from a permission string
    /// </summary>
    public static string GetModuleName(string permission)
    {
        var parts = permission.Split('.');
        return parts.Length > 0 ? parts[0] : string.Empty;
    }
}
