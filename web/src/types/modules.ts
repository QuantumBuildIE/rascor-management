import { Package, Users, FileText, Settings, ClipboardList, ShieldCheck, type LucideIcon } from "lucide-react";

export interface Module {
  id: string;
  name: string;
  description: string;
  icon: LucideIcon;
  path: string;
  requiredPermission: string;
}

export const availableModules: Module[] = [
  {
    id: "stock-management",
    name: "Stock Management",
    description: "Manage inventory, orders, and purchasing",
    icon: Package,
    path: "/stock",
    requiredPermission: "StockManagement.View",
  },
  {
    id: "site-attendance",
    name: "Site Attendance",
    description: "Track site attendance and timesheets",
    icon: Users,
    path: "/site-attendance",
    requiredPermission: "SiteAttendance.View",
  },
  {
    id: "proposals",
    name: "Proposals",
    description: "Create and manage project proposals",
    icon: FileText,
    path: "/proposals",
    requiredPermission: "Proposals.View",
  },
  {
    id: "toolbox-talks",
    name: "Toolbox Talks",
    description: "Safety training talks and compliance tracking",
    icon: ClipboardList,
    path: "/toolbox-talks",
    requiredPermission: "ToolboxTalks.View",
  },
  {
    id: "rams",
    name: "RAMS",
    description: "Risk Assessments and Method Statements",
    icon: ShieldCheck,
    path: "/rams",
    requiredPermission: "Rams.View",
  },
  {
    id: "admin",
    name: "Administration",
    description: "Manage sites, employees, companies, and users",
    icon: Settings,
    path: "/admin",
    requiredPermission: "Core.Admin",
  },
];
