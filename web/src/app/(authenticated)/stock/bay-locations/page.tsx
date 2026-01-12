"use client";

import * as React from "react";
import Link from "next/link";
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
  DataTable,
  type Column,
  type SortDirection,
} from "@/components/shared/data-table";
import { DeleteConfirmationDialog } from "@/components/shared/delete-confirmation-dialog";
import { useBayLocations, useDeleteBayLocation } from "@/lib/api/stock/use-bay-locations";
import { useLocations } from "@/lib/api/stock/use-locations";
import type { BayLocation } from "@/types/stock";
import { toast } from "sonner";

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

export default function BayLocationsPage() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  // Parse URL params
  const sortColumn = searchParams.get("sortColumn") || undefined;
  const sortDirection =
    (searchParams.get("sortDirection") as SortDirection) || undefined;
  const searchParam = searchParams.get("search") || "";
  const locationFilter = searchParams.get("location") || "";

  const [searchInput, setSearchInput] = React.useState(searchParam);
  const debouncedSearch = useDebounce(searchInput, 300);

  // Delete dialog state
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false);
  const [bayLocationToDelete, setBayLocationToDelete] = React.useState<BayLocation | null>(null);

  // Update URL when search changes (debounced)
  React.useEffect(() => {
    if (debouncedSearch !== searchParam) {
      updateUrlParams({ search: debouncedSearch || null });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedSearch]);

  const { data: bayLocations, isLoading, error } = useBayLocations();
  const { data: locations = [] } = useLocations();
  const deleteBayLocation = useDeleteBayLocation();

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

  const handleSort = (column: string, direction: SortDirection) => {
    updateUrlParams({ sortColumn: column, sortDirection: direction });
  };

  const handleLocationFilter = (value: string) => {
    updateUrlParams({ location: value === "all" ? null : value });
  };

  const handleDeleteClick = (bayLocation: BayLocation) => {
    setBayLocationToDelete(bayLocation);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!bayLocationToDelete) return;

    try {
      await deleteBayLocation.mutateAsync(bayLocationToDelete.id);
      toast.success("Bay location deleted successfully");
      setDeleteDialogOpen(false);
      setBayLocationToDelete(null);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to delete bay location", {
        description: message,
      });
    }
  };

  // Client-side filtering and sorting
  const filteredAndSortedData = React.useMemo(() => {
    let data = bayLocations ?? [];

    // Filter by location
    if (locationFilter) {
      data = data.filter((item) => item.stockLocationId === locationFilter);
    }

    // Filter by search
    if (searchParam) {
      const searchLower = searchParam.toLowerCase();
      data = data.filter(
        (item) =>
          item.bayCode.toLowerCase().includes(searchLower) ||
          (item.bayName?.toLowerCase().includes(searchLower) ?? false) ||
          item.stockLocationName.toLowerCase().includes(searchLower)
      );
    }

    // Sort
    if (sortColumn) {
      const sortDir = sortDirection === "desc" ? -1 : 1;
      data = [...data].sort((a, b) => {
        const aValue = a[sortColumn as keyof BayLocation];
        const bValue = b[sortColumn as keyof BayLocation];

        if (aValue === null || aValue === undefined) return 1;
        if (bValue === null || bValue === undefined) return -1;

        if (typeof aValue === "string" && typeof bValue === "string") {
          return aValue.localeCompare(bValue) * sortDir;
        }
        if (typeof aValue === "number" && typeof bValue === "number") {
          return (aValue - bValue) * sortDir;
        }
        if (typeof aValue === "boolean" && typeof bValue === "boolean") {
          return (Number(aValue) - Number(bValue)) * sortDir;
        }
        return String(aValue).localeCompare(String(bValue)) * sortDir;
      });
    }

    return data;
  }, [bayLocations, locationFilter, searchParam, sortColumn, sortDirection]);

  const columns: Column<BayLocation>[] = [
    {
      key: "bayCode",
      header: "Bay Code",
      sortable: true,
      className: "font-medium",
    },
    {
      key: "bayName",
      header: "Bay Name",
      sortable: true,
      render: (bayLocation) => bayLocation.bayName || "-",
    },
    {
      key: "stockLocationName",
      header: "Stock Location",
      sortable: true,
      render: (bayLocation) => (
        <div className="flex flex-col">
          <span className="font-medium">{bayLocation.stockLocationCode}</span>
          <span className="text-sm text-muted-foreground">
            {bayLocation.stockLocationName}
          </span>
        </div>
      ),
    },
    {
      key: "capacity",
      header: "Capacity",
      sortable: true,
      className: "text-right",
      headerClassName: "text-right",
      render: (bayLocation) => bayLocation.capacity ?? "-",
    },
    {
      key: "isActive",
      header: "Status",
      sortable: true,
      render: (bayLocation) =>
        bayLocation.isActive ? (
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
      render: (bayLocation) => (
        <div className="flex items-center justify-end gap-2">
          <Button variant="ghost" size="sm" asChild>
            <Link href={`/stock/bay-locations/${bayLocation.id}/edit`}>Edit</Link>
          </Button>
          <Button
            variant="ghost"
            size="sm"
            className="text-destructive hover:text-destructive"
            onClick={(e) => {
              e.stopPropagation();
              handleDeleteClick(bayLocation);
            }}
          >
            Delete
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
            <h1 className="text-2xl font-semibold tracking-tight">Bay Locations</h1>
            <p className="text-muted-foreground">
              Manage warehouse bay locations
            </p>
          </div>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load bay locations. Please try again.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Bay Locations</h1>
          <p className="text-muted-foreground">Manage warehouse bay locations for stock tracking</p>
        </div>
        <Button asChild>
          <Link href="/stock/bay-locations/new">Add Bay Location</Link>
        </Button>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-sm">
          <SearchIcon className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search bay locations..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="pl-9"
          />
        </div>
        <Select value={locationFilter || "all"} onValueChange={handleLocationFilter}>
          <SelectTrigger className="w-[250px]">
            <SelectValue placeholder="Filter by location" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Locations</SelectItem>
            {locations
              .filter((l) => l.isActive)
              .map((location) => (
                <SelectItem key={location.id} value={location.id}>
                  {location.locationCode} - {location.locationName}
                </SelectItem>
              ))}
          </SelectContent>
        </Select>
      </div>

      <DataTable
        columns={columns}
        data={filteredAndSortedData}
        isLoading={isLoading}
        emptyMessage="No bay locations found"
        keyExtractor={(bayLocation) => bayLocation.id}
        sortColumn={sortColumn}
        sortDirection={sortDirection}
        onSort={handleSort}
      />

      <DeleteConfirmationDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        title="Delete Bay Location"
        description={`Are you sure you want to delete bay "${bayLocationToDelete?.bayCode}"? This action cannot be undone.`}
        onConfirm={handleDeleteConfirm}
        isLoading={deleteBayLocation.isPending}
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
