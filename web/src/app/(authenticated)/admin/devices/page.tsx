"use client";

import * as React from "react";
import { useSearchParams, useRouter, usePathname } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import {
  DataTable,
  type Column,
} from "@/components/shared/data-table";
import {
  useDevices,
  useDeactivateDevice,
  useReactivateDevice,
} from "@/lib/api/admin/use-devices";
import { LinkDeviceDialog } from "@/components/admin/link-device-dialog";
import { UnlinkDeviceDialog } from "@/components/admin/unlink-device-dialog";
import { toast } from "sonner";
import type { AdminDevice } from "@/types/admin";
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

export default function DevicesPage() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  // Parse URL params
  const pageNumber = Number(searchParams.get("page")) || 1;
  const pageSize = Number(searchParams.get("size")) || 20;
  const searchParam = searchParams.get("search") || "";
  const linkedFilter = searchParams.get("linked") || "";
  const statusFilter = searchParams.get("status") || "";

  const [searchInput, setSearchInput] = React.useState(searchParam);
  const debouncedSearch = useDebounce(searchInput, 300);

  // Dialog states
  const [linkDevice, setLinkDevice] = React.useState<AdminDevice | null>(null);
  const [unlinkDevice, setUnlinkDevice] = React.useState<AdminDevice | null>(null);
  const [toggleStatusDevice, setToggleStatusDevice] = React.useState<AdminDevice | null>(null);

  // Update URL when search changes (debounced)
  React.useEffect(() => {
    if (debouncedSearch !== searchParam) {
      updateUrlParams({ search: debouncedSearch || null, page: 1 });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedSearch]);

  const { data, isLoading, error } = useDevices({
    page: pageNumber,
    pageSize,
    search: searchParam || undefined,
    isLinked:
      linkedFilter === "linked"
        ? true
        : linkedFilter === "unlinked"
        ? false
        : undefined,
    isActive:
      statusFilter === "active"
        ? true
        : statusFilter === "inactive"
        ? false
        : undefined,
  });

  const deactivateDevice = useDeactivateDevice();
  const reactivateDevice = useReactivateDevice();

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

    // Remove page param if it's 1
    if (params.get("page") === "1") {
      params.delete("page");
    }

    const queryString = params.toString();
    router.push(queryString ? `${pathname}?${queryString}` : pathname);
  };

  const handlePageChange = (page: number) => {
    updateUrlParams({ page });
  };

  const handlePageSizeChange = (size: number) => {
    updateUrlParams({ size, page: 1 });
  };

  const handleLinkedFilter = (value: string) => {
    updateUrlParams({ linked: value === "all" ? null : value, page: 1 });
  };

  const handleStatusFilter = (value: string) => {
    updateUrlParams({ status: value === "all" ? null : value, page: 1 });
  };

  const handleToggleStatusConfirm = async () => {
    if (!toggleStatusDevice) return;

    const action = toggleStatusDevice.isActive ? "deactivated" : "reactivated";
    try {
      if (toggleStatusDevice.isActive) {
        await deactivateDevice.mutateAsync(toggleStatusDevice.id);
      } else {
        await reactivateDevice.mutateAsync(toggleStatusDevice.id);
      }
      toast.success(`Device ${action} successfully`, {
        description: `${toggleStatusDevice.deviceIdentifier} has been ${action}`,
      });
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error(
        `Failed to ${toggleStatusDevice.isActive ? "deactivate" : "reactivate"} device`,
        {
          description: message,
        }
      );
    } finally {
      setToggleStatusDevice(null);
    }
  };

  const formatLastSeen = (lastActiveAt: string | undefined) => {
    if (!lastActiveAt) return "Never";
    try {
      return formatDistanceToNow(new Date(lastActiveAt), { addSuffix: true });
    } catch {
      return "Unknown";
    }
  };

  const columns: Column<AdminDevice>[] = [
    {
      key: "deviceIdentifier",
      header: "Device",
      render: (device) => (
        <div>
          <div className="font-medium">{device.deviceIdentifier}</div>
          <div className="text-sm text-muted-foreground">
            {device.platform || "Unknown"}{" "}
            {device.deviceName && `(${device.deviceName})`}
          </div>
        </div>
      ),
    },
    {
      key: "employee",
      header: "Employee",
      render: (device) =>
        device.isLinked ? (
          <div>
            <div className="font-medium">{device.employeeName}</div>
            {device.employeeEmail && (
              <div className="text-sm text-muted-foreground">
                {device.employeeEmail}
              </div>
            )}
          </div>
        ) : (
          <div className="flex items-center gap-2">
            <Badge variant="outline" className="text-amber-600 border-amber-600">
              Unlinked
            </Badge>
            <Button
              variant="link"
              size="sm"
              className="h-auto p-0 text-primary"
              onClick={(e) => {
                e.stopPropagation();
                setLinkDevice(device);
              }}
            >
              Link Device
            </Button>
          </div>
        ),
    },
    {
      key: "lastActiveAt",
      header: "Last Seen",
      render: (device) => (
        <span className="text-muted-foreground">
          {formatLastSeen(device.lastActiveAt)}
        </span>
      ),
    },
    {
      key: "isActive",
      header: "Status",
      render: (device) =>
        device.isActive ? (
          <Badge variant="default">Active</Badge>
        ) : (
          <Badge variant="secondary">Inactive</Badge>
        ),
    },
    {
      key: "actions",
      header: "Actions",
      headerClassName: "text-right",
      className: "text-right",
      render: (device) => (
        <div className="flex items-center justify-end gap-2">
          {device.isLinked && (
            <Button
              variant="ghost"
              size="sm"
              onClick={(e) => {
                e.stopPropagation();
                setUnlinkDevice(device);
              }}
            >
              Unlink
            </Button>
          )}
          {!device.isLinked && (
            <Button
              variant="ghost"
              size="sm"
              onClick={(e) => {
                e.stopPropagation();
                setLinkDevice(device);
              }}
            >
              Link
            </Button>
          )}
          <Button
            variant="ghost"
            size="sm"
            onClick={(e) => {
              e.stopPropagation();
              setToggleStatusDevice(device);
            }}
            disabled={deactivateDevice.isPending || reactivateDevice.isPending}
          >
            {device.isActive ? "Deactivate" : "Reactivate"}
          </Button>
        </div>
      ),
    },
  ];

  if (error) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-semibold tracking-tight">
              Registered Devices
            </h1>
            <p className="text-muted-foreground">
              Manage device registrations and employee linking
            </p>
          </div>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load devices. Please try again.
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
            Registered Devices
          </h1>
          <p className="text-muted-foreground">
            Manage device registrations and employee linking
          </p>
        </div>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-sm">
          <SearchIcon className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search by device ID or employee..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="pl-9"
          />
        </div>

        <div className="flex items-center gap-2">
          <Select
            value={linkedFilter || "all"}
            onValueChange={handleLinkedFilter}
          >
            <SelectTrigger className="w-[140px]">
              <SelectValue placeholder="Link Status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Devices</SelectItem>
              <SelectItem value="linked">Linked</SelectItem>
              <SelectItem value="unlinked">Unlinked</SelectItem>
            </SelectContent>
          </Select>

          <Select
            value={statusFilter || "all"}
            onValueChange={handleStatusFilter}
          >
            <SelectTrigger className="w-[130px]">
              <SelectValue placeholder="Status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Status</SelectItem>
              <SelectItem value="active">Active</SelectItem>
              <SelectItem value="inactive">Inactive</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      <DataTable
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        emptyMessage="No devices found"
        keyExtractor={(device) => device.id}
        skeletonRows={pageSize}
        pagination={
          data
            ? {
                pageNumber: data.pageNumber,
                pageSize: data.pageSize,
                totalCount: data.totalCount,
                totalPages: data.totalPages,
              }
            : undefined
        }
        onPageChange={handlePageChange}
        onPageSizeChange={handlePageSizeChange}
      />

      {/* Link Device Dialog */}
      {linkDevice && (
        <LinkDeviceDialog
          open={!!linkDevice}
          onOpenChange={(open) => !open && setLinkDevice(null)}
          device={linkDevice}
        />
      )}

      {/* Unlink Device Dialog */}
      {unlinkDevice && (
        <UnlinkDeviceDialog
          open={!!unlinkDevice}
          onOpenChange={(open) => !open && setUnlinkDevice(null)}
          device={unlinkDevice}
        />
      )}

      {/* Activate/Deactivate Confirmation Dialog */}
      <AlertDialog
        open={!!toggleStatusDevice}
        onOpenChange={(open) => !open && setToggleStatusDevice(null)}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              {toggleStatusDevice?.isActive ? "Deactivate" : "Reactivate"} Device
            </AlertDialogTitle>
            <AlertDialogDescription>
              {toggleStatusDevice?.isActive
                ? `Are you sure you want to deactivate ${toggleStatusDevice?.deviceIdentifier}? The device will no longer be able to send attendance events.`
                : `Are you sure you want to reactivate ${toggleStatusDevice?.deviceIdentifier}? The device will be able to send attendance events again.`}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleToggleStatusConfirm}>
              {toggleStatusDevice?.isActive ? "Deactivate" : "Reactivate"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
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
