"use client";

import * as React from "react";
import { useSearchParams, useRouter, usePathname } from "next/navigation";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  DataTable,
  type Column,
} from "@/components/shared/data-table";
import {
  useDeviceMonitor,
  useDeviceMonitorSummary,
  type DeviceMonitorItem,
} from "@/lib/api/admin/use-device-monitor";
import { formatDistanceToNow } from "date-fns";

function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = React.useState(value);

  React.useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(timer);
    };
  }, [value, delay]);

  return debouncedValue;
}

export default function DeviceMonitorPage() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  // Parse URL params
  const searchParam = searchParams.get("search") || "";
  const statusFilter = searchParams.get("status") || "";

  const [searchInput, setSearchInput] = React.useState(searchParam);
  const debouncedSearch = useDebounce(searchInput, 300);

  // Update URL when search changes (debounced)
  React.useEffect(() => {
    if (debouncedSearch !== searchParam) {
      updateUrlParams({ search: debouncedSearch || null });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedSearch]);

  const { data: devices, isLoading, error } = useDeviceMonitor();
  const { data: summary, isLoading: summaryLoading } = useDeviceMonitorSummary();

  const updateUrlParams = (
    updates: Record<string, string | number | null | undefined>
  ) => {
    const params = new URLSearchParams(searchParams.toString());

    Object.entries(updates).forEach(([key, value]) => {
      if (value === null || value === undefined || value === "") {
        params.delete(key);
      } else {
        params.set(key, String(value));
      }
    });

    const queryString = params.toString();
    router.push(queryString ? `${pathname}?${queryString}` : pathname);
  };

  const handleStatusFilter = (value: string) => {
    updateUrlParams({ status: value === "all" ? null : value });
  };

  // Filter data based on URL params
  const filteredData = React.useMemo(() => {
    if (!devices) return [];

    let filtered = [...devices];

    // Filter by status
    if (statusFilter) {
      filtered = filtered.filter(
        (d) => d.status.toLowerCase() === statusFilter.toLowerCase()
      );
    }

    // Filter by search
    if (searchParam) {
      const search = searchParam.toLowerCase();
      filtered = filtered.filter(
        (d) =>
          d.deviceId.toLowerCase().includes(search) ||
          (d.employeeName && d.employeeName.toLowerCase().includes(search)) ||
          (d.deviceModel && d.deviceModel.toLowerCase().includes(search))
      );
    }

    return filtered;
  }, [devices, statusFilter, searchParam]);

  const formatLastSeen = (lastSeenAt: string | null) => {
    if (!lastSeenAt) return "Never";
    try {
      return formatDistanceToNow(new Date(lastSeenAt), { addSuffix: true });
    } catch {
      return "Unknown";
    }
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case "Online":
        return (
          <Badge className="bg-green-500 hover:bg-green-600 text-white">
            Online
          </Badge>
        );
      case "Stale":
        return (
          <Badge className="bg-amber-500 hover:bg-amber-600 text-white">
            Stale
          </Badge>
        );
      case "Offline":
        return (
          <Badge className="bg-red-500 hover:bg-red-600 text-white">
            Offline
          </Badge>
        );
      default:
        return <Badge variant="secondary">{status}</Badge>;
    }
  };

  const getBatteryDisplay = (level: number | null) => {
    if (level === null) return <span className="text-muted-foreground">-</span>;

    let colorClass = "text-green-600";
    if (level < 15) {
      colorClass = "text-red-600";
    } else if (level < 50) {
      colorClass = "text-amber-600";
    }

    return <span className={colorClass}>{level}%</span>;
  };

  const getLocationBadge = (locationMatch: string | null) => {
    switch (locationMatch) {
      case "OnSite":
        return (
          <span className="text-green-600 font-medium">On Site</span>
        );
      case "Near":
        return (
          <span className="text-amber-600 font-medium">Near</span>
        );
      case "Away":
        return (
          <span className="text-red-600 font-medium">Away</span>
        );
      case "Unknown":
      default:
        return <span className="text-muted-foreground">Unknown</span>;
    }
  };

  const columns: Column<DeviceMonitorItem>[] = [
    {
      key: "employee",
      header: "Worker",
      render: (item) =>
        item.employeeName ? (
          <div className="font-medium">{item.employeeName}</div>
        ) : (
          <span className="text-muted-foreground">-</span>
        ),
    },
    {
      key: "device",
      header: "Device",
      render: (item) => (
        <div>
          <div className="font-medium">{item.deviceModel || "Unknown"}</div>
          <div className="text-sm text-muted-foreground">{item.deviceId}</div>
        </div>
      ),
    },
    {
      key: "status",
      header: "Status",
      render: (item) => getStatusBadge(item.status),
    },
    {
      key: "lastSeen",
      header: "Last Seen",
      render: (item) => (
        <span className="text-muted-foreground">
          {formatLastSeen(item.lastSeenAt)}
        </span>
      ),
    },
    {
      key: "battery",
      header: "Battery",
      render: (item) => getBatteryDisplay(item.batteryLevel),
    },
    {
      key: "scheduledSite",
      header: "Scheduled Site",
      render: (item) =>
        item.scheduledSiteName ? (
          <span>{item.scheduledSiteName}</span>
        ) : (
          <span className="text-muted-foreground">-</span>
        ),
    },
    {
      key: "location",
      header: "Location",
      render: (item) => getLocationBadge(item.locationMatch),
    },
  ];

  if (error) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-semibold tracking-tight">
              Device Monitor
            </h1>
            <p className="text-muted-foreground">
              Real-time status of all geofence devices
            </p>
          </div>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load device data. Please try again.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">
            Device Monitor
          </h1>
          <p className="text-muted-foreground">
            Real-time status of all geofence devices
          </p>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid gap-4 grid-cols-2 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Devices
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {summaryLoading ? "-" : summary?.totalDevices ?? 0}
            </div>
          </CardContent>
        </Card>

        <Card className="border-green-200 bg-green-50 dark:border-green-900 dark:bg-green-950">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-green-700 dark:text-green-300">
              Online
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-700 dark:text-green-300">
              {summaryLoading ? "-" : summary?.online ?? 0}
            </div>
          </CardContent>
        </Card>

        <Card className="border-amber-200 bg-amber-50 dark:border-amber-900 dark:bg-amber-950">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-amber-700 dark:text-amber-300">
              Stale
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-amber-700 dark:text-amber-300">
              {summaryLoading ? "-" : summary?.stale ?? 0}
            </div>
          </CardContent>
        </Card>

        <Card className="border-red-200 bg-red-50 dark:border-red-900 dark:bg-red-950">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-red-700 dark:text-red-300">
              Offline
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-red-700 dark:text-red-300">
              {summaryLoading ? "-" : summary?.offline ?? 0}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-sm">
          <SearchIcon className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search by worker name or device ID..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="pl-9"
          />
        </div>

        <div className="flex items-center gap-2">
          <Select
            value={statusFilter || "all"}
            onValueChange={handleStatusFilter}
          >
            <SelectTrigger className="w-[140px]">
              <SelectValue placeholder="Status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Status</SelectItem>
              <SelectItem value="online">Online</SelectItem>
              <SelectItem value="stale">Stale</SelectItem>
              <SelectItem value="offline">Offline</SelectItem>
            </SelectContent>
          </Select>
        </div>

        {summary?.lastSyncedAt && (
          <div className="text-sm text-muted-foreground ml-auto">
            Last synced: {formatLastSeen(summary.lastSyncedAt)}
          </div>
        )}
      </div>

      {/* Data Table */}
      <DataTable
        columns={columns}
        data={filteredData}
        isLoading={isLoading}
        emptyMessage="No devices found"
        keyExtractor={(item) => item.deviceId}
        skeletonRows={10}
      />
    </div>
  );
}

function SearchIcon({ className }: { className?: string }) {
  return (
    <svg
      className={className}
      fill="none"
      stroke="currentColor"
      viewBox="0 0 24 24"
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth={2}
        d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
      />
    </svg>
  );
}
