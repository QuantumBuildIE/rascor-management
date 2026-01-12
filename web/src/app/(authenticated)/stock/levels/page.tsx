"use client";

import * as React from "react";
import { useSearchParams, useRouter, usePathname } from "next/navigation";
import { Input } from "@/components/ui/input";
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
import { useStockLevels } from "@/lib/api/stock/use-stock-levels";
import { useLocations } from "@/lib/api/stock/use-locations";
import type { StockLevel } from "@/types/stock";
import { cn } from "@/lib/utils";

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

function formatDate(dateString: string | null): string {
  if (!dateString) return "-";
  return new Intl.DateTimeFormat("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
  }).format(new Date(dateString));
}

export default function StockLevelsPage() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  // Parse URL params
  const sortColumn = searchParams.get("sortColumn") || undefined;
  const sortDirection =
    (searchParams.get("sortDirection") as SortDirection) || undefined;
  const searchParam = searchParams.get("search") || "";
  const locationIdParam = searchParams.get("locationId") || "";

  const [searchInput, setSearchInput] = React.useState(searchParam);
  const debouncedSearch = useDebounce(searchInput, 300);

  // Update URL when search changes (debounced)
  React.useEffect(() => {
    if (debouncedSearch !== searchParam) {
      updateUrlParams({ search: debouncedSearch || null });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedSearch]);

  const { data: locations, isLoading: locationsLoading } = useLocations();
  const { data, isLoading, error } = useStockLevels({
    sortColumn,
    sortDirection,
    search: searchParam || undefined,
    locationId: locationIdParam || undefined,
  });

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

  const handleLocationChange = (value: string) => {
    updateUrlParams({ locationId: value === "all" ? null : value });
  };

  const columns: Column<StockLevel>[] = [
    {
      key: "productCode",
      header: "Product Code",
      sortable: true,
      className: "font-medium",
    },
    {
      key: "productName",
      header: "Product Name",
      sortable: true,
    },
    {
      key: "locationName",
      header: "Location",
      sortable: true,
    },
    {
      key: "bayCode",
      header: "Bay",
      sortable: true,
      render: (item) =>
        item.bayCode ? (
          <div className="flex flex-col">
            <span className="font-medium">{item.bayCode}</span>
            {item.bayName && (
              <span className="text-xs text-muted-foreground truncate max-w-[150px]">
                {item.bayName}
              </span>
            )}
          </div>
        ) : (
          <span className="text-muted-foreground">-</span>
        ),
    },
    {
      key: "quantityOnHand",
      header: "Qty On Hand",
      sortable: true,
      className: "text-right",
      headerClassName: "text-right",
      render: (item) => (
        <span className="font-medium">{item.quantityOnHand}</span>
      ),
    },
    {
      key: "quantityReserved",
      header: "Qty Reserved",
      sortable: true,
      className: "text-right",
      headerClassName: "text-right",
      render: (item) => <span>{item.quantityReserved}</span>,
    },
    {
      key: "quantityAvailable",
      header: "Qty Available",
      sortable: true,
      className: "text-right",
      headerClassName: "text-right",
      render: (item) => (
        <span
          className={cn(
            "font-medium",
            item.quantityAvailable <= 0 && "text-destructive"
          )}
        >
          {item.quantityAvailable}
        </span>
      ),
    },
    {
      key: "quantityOnOrder",
      header: "Qty On Order",
      sortable: true,
      className: "text-right",
      headerClassName: "text-right",
      render: (item) => <span>{item.quantityOnOrder}</span>,
    },
    {
      key: "lastMovementDate",
      header: "Last Movement",
      sortable: true,
      render: (item) => (
        <span className="text-muted-foreground">
          {formatDate(item.lastMovementDate)}
        </span>
      ),
    },
  ];

  // Check if item is low stock (at or below reorder level)
  const isLowStock = (item: StockLevel) =>
    item.quantityOnHand <= item.reorderLevel;

  if (error) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Stock Levels</h1>
          <p className="text-muted-foreground">
            View stock quantities across locations
          </p>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load stock levels. Please try again.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Stock Levels</h1>
        <p className="text-muted-foreground">
          View stock quantities across locations
        </p>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-sm">
          <SearchIcon className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search by product name or code..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="pl-9"
          />
        </div>
        <div className="w-full sm:w-[200px]">
          <Select
            value={locationIdParam || "all"}
            onValueChange={handleLocationChange}
            disabled={locationsLoading}
          >
            <SelectTrigger>
              <SelectValue placeholder="All Locations" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Locations</SelectItem>
              {locations?.map((location) => (
                <SelectItem key={location.id} value={location.id}>
                  {location.locationName}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      <StockLevelsTable
        columns={columns}
        data={data ?? []}
        isLoading={isLoading}
        sortColumn={sortColumn}
        sortDirection={sortDirection}
        onSort={handleSort}
        isLowStock={isLowStock}
      />
    </div>
  );
}

// Custom table component to handle row highlighting
function StockLevelsTable({
  columns,
  data,
  isLoading,
  sortColumn,
  sortDirection,
  onSort,
  isLowStock,
}: {
  columns: Column<StockLevel>[];
  data: StockLevel[];
  isLoading: boolean;
  sortColumn?: string;
  sortDirection?: SortDirection;
  onSort: (column: string, direction: SortDirection) => void;
  isLowStock: (item: StockLevel) => boolean;
}) {
  // Wrap the render functions to add row highlighting
  const enhancedColumns = columns.map((col) => ({
    ...col,
    render: col.render
      ? (item: StockLevel) => col.render!(item)
      : undefined,
  }));

  return (
    <div className="rounded-lg border bg-card">
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead>
            <tr className="border-b bg-muted/50">
              {enhancedColumns.map((column) => (
                <th
                  key={column.key}
                  className={cn(
                    "h-10 px-4 text-left text-sm font-medium text-muted-foreground",
                    column.sortable && "cursor-pointer select-none",
                    column.headerClassName
                  )}
                  onClick={() => {
                    if (column.sortable) {
                      if (sortColumn !== column.key) {
                        onSort(column.key, "asc");
                      } else if (sortDirection === "asc") {
                        onSort(column.key, "desc");
                      } else {
                        onSort(column.key, "asc");
                      }
                    }
                  }}
                >
                  <div className="flex items-center">
                    {column.header}
                    {column.sortable && (
                      <SortIcon
                        column={column.key}
                        sortColumn={sortColumn}
                        sortDirection={sortDirection}
                      />
                    )}
                  </div>
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              Array.from({ length: 10 }).map((_, rowIndex) => (
                <tr key={rowIndex} className="border-b">
                  {enhancedColumns.map((column) => (
                    <td key={column.key} className="h-12 px-4">
                      <div className="h-5 w-full max-w-[200px] animate-pulse rounded bg-muted" />
                    </td>
                  ))}
                </tr>
              ))
            ) : data.length === 0 ? (
              <tr>
                <td
                  colSpan={enhancedColumns.length}
                  className="h-32 text-center text-muted-foreground"
                >
                  No stock levels found
                </td>
              </tr>
            ) : (
              data.map((item) => (
                <tr
                  key={item.id}
                  className={cn(
                    "border-b transition-colors",
                    isLowStock(item)
                      ? "bg-amber-50 hover:bg-amber-100 dark:bg-amber-950/30 dark:hover:bg-amber-950/50"
                      : "hover:bg-muted/50"
                  )}
                >
                  {enhancedColumns.map((column) => (
                    <td key={column.key} className={cn("h-12 px-4", column.className)}>
                      {column.render
                        ? column.render(item)
                        : String(
                            (item as unknown as Record<string, unknown>)[column.key] ?? ""
                          )}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function SortIcon({
  column,
  sortColumn,
  sortDirection,
}: {
  column: string;
  sortColumn?: string;
  sortDirection?: SortDirection;
}) {
  if (sortColumn !== column) {
    return (
      <svg
        className="ml-1 h-4 w-4 text-muted-foreground/50"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          strokeWidth={2}
          d="M7 16V4m0 0L3 8m4-4l4 4m6 0v12m0 0l4-4m-4 4l-4-4"
        />
      </svg>
    );
  }
  if (sortDirection === "asc") {
    return (
      <svg
        className="ml-1 h-4 w-4"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          strokeWidth={2}
          d="M5 15l7-7 7 7"
        />
      </svg>
    );
  }
  return (
    <svg
      className="ml-1 h-4 w-4"
      fill="none"
      stroke="currentColor"
      viewBox="0 0 24 24"
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth={2}
        d="M19 9l-7 7-7-7"
      />
    </svg>
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
