import { useQuery } from "@tanstack/react-query";
import {
  getDeviceMonitor,
  getDeviceMonitorSummary,
  type DeviceMonitorItem,
  type DeviceMonitorSummary,
} from "./device-monitor";

export const DEVICE_MONITOR_KEY = ["admin-device-monitor"];

export function useDeviceMonitor() {
  return useQuery<DeviceMonitorItem[]>({
    queryKey: [...DEVICE_MONITOR_KEY, "list"],
    queryFn: () => getDeviceMonitor(),
    refetchInterval: 5 * 60 * 1000, // Auto-refresh every 5 minutes
  });
}

export function useDeviceMonitorSummary() {
  return useQuery<DeviceMonitorSummary>({
    queryKey: [...DEVICE_MONITOR_KEY, "summary"],
    queryFn: () => getDeviceMonitorSummary(),
    refetchInterval: 5 * 60 * 1000, // Auto-refresh every 5 minutes
  });
}

export type { DeviceMonitorItem, DeviceMonitorSummary } from "./device-monitor";
