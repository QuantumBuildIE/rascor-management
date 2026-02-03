import { apiClient } from "@/lib/api/client";

export interface DeviceMonitorItem {
  deviceId: string;
  deviceModel: string | null;
  employeeName: string | null;
  employeeId: string | null;
  status: "Online" | "Stale" | "Offline";
  lastSeenAt: string | null;
  batteryLevel: number | null;
  lastLatitude: number | null;
  lastLongitude: number | null;
  scheduledSiteName: string | null;
  scheduledSiteLatitude: number | null;
  scheduledSiteLongitude: number | null;
  locationMatch: "OnSite" | "Near" | "Away" | "Unknown" | null;
}

export interface DeviceMonitorSummary {
  totalDevices: number;
  online: number;
  stale: number;
  offline: number;
  onSite: number;
  lastSyncedAt: string | null;
}

export async function getDeviceMonitor(): Promise<DeviceMonitorItem[]> {
  const response = await apiClient.get<DeviceMonitorItem[]>("/admin/device-monitor");
  return response.data || [];
}

export async function getDeviceMonitorSummary(): Promise<DeviceMonitorSummary> {
  const response = await apiClient.get<DeviceMonitorSummary>("/admin/device-monitor/summary");
  return response.data || {
    totalDevices: 0,
    online: 0,
    stale: 0,
    offline: 0,
    onSite: 0,
    lastSyncedAt: null,
  };
}
