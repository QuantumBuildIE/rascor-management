import type { User } from "@/types/auth";

/**
 * Determines the appropriate home page route based on user's roles and permissions
 */
export function getHomeRoute(user: User | null): string {
  if (!user) {
    return "/login";
  }

  const { roles, permissions } = user;

  // Admin goes to dashboard
  if (roles.includes("Admin")) {
    return "/dashboard";
  }

  // Finance goes to dashboard (has view access to most things)
  if (roles.includes("Finance")) {
    return "/dashboard";
  }

  // WarehouseStaff goes to stock management
  if (roles.includes("WarehouseStaff")) {
    return "/stock";
  }

  // SiteManager - check if they have site attendance permissions first
  if (roles.includes("SiteManager")) {
    if (permissions.includes("SiteAttendance.View")) {
      return "/site-attendance";
    }
    if (permissions.includes("StockManagement.CreateOrders")) {
      return "/stock/orders";
    }
    return "/dashboard";
  }

  // OfficeStaff - proposals module isn't built yet, go to dashboard
  if (roles.includes("OfficeStaff")) {
    return "/dashboard";
  }

  // Fallback: check permissions directly
  if (permissions.includes("SiteAttendance.View")) {
    return "/site-attendance";
  }

  if (permissions.includes("StockManagement.Admin") || permissions.includes("StockManagement.ManageProducts")) {
    return "/stock";
  }

  if (permissions.includes("StockManagement.ReceiveGoods")) {
    return "/stock/goods-receipts";
  }

  if (permissions.includes("StockManagement.CreateOrders")) {
    return "/stock/orders";
  }

  if (permissions.includes("StockManagement.View")) {
    return "/stock";
  }

  // Default fallback
  return "/dashboard";
}
