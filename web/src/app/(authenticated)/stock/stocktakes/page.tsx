"use client";

import * as React from "react";
import Link from "next/link";
import { useSearchParams, useRouter, usePathname } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  DataTable,
  type Column,
  type SortDirection,
} from "@/components/shared/data-table";
import { DeleteConfirmationDialog } from "@/components/shared/delete-confirmation-dialog";
import {
  useStocktakes,
  useStartStocktake,
  useCancelStocktake,
  useDeleteStocktake,
} from "@/lib/api/stock/use-stocktakes";
import type { Stocktake, StocktakeStatus } from "@/types/stock";
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

const statusTabs: { value: string; label: string }[] = [
  { value: "all", label: "All" },
  { value: "Draft", label: "Draft" },
  { value: "InProgress", label: "In Progress" },
  { value: "Completed", label: "Completed" },
  { value: "Cancelled", label: "Cancelled" },
];

const statusVariants: Record<
  StocktakeStatus,
  "default" | "secondary" | "destructive" | "outline"
> = {
  Draft: "secondary",
  InProgress: "default",
  Completed: "outline",
  Cancelled: "destructive",
};

const statusLabels: Record<StocktakeStatus, string> = {
  Draft: "Draft",
  InProgress: "In Progress",
  Completed: "Completed",
  Cancelled: "Cancelled",
};

export default function StocktakesPage() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  // Parse URL params
  const sortColumn = searchParams.get("sortColumn") || undefined;
  const sortDirection =
    (searchParams.get("sortDirection") as SortDirection) || undefined;
  const searchParam = searchParams.get("search") || "";
  const statusFilter = searchParams.get("status") || "all";

  const [searchInput, setSearchInput] = React.useState(searchParam);
  const debouncedSearch = useDebounce(searchInput, 300);

  // Dialog states
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false);
  const [stocktakeToDelete, setStocktakeToDelete] =
    React.useState<Stocktake | null>(null);

  // Update URL when search changes (debounced)
  React.useEffect(() => {
    if (debouncedSearch !== searchParam) {
      updateUrlParams({ search: debouncedSearch || null });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedSearch]);

  const { data: stocktakes, isLoading, error } = useStocktakes();
  const startStocktake = useStartStocktake();
  const cancelStocktake = useCancelStocktake();
  const deleteStocktake = useDeleteStocktake();

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

  const handleStatusChange = (status: string) => {
    updateUrlParams({ status: status === "all" ? null : status });
  };

  // Workflow actions
  const handleStart = async (stocktake: Stocktake) => {
    try {
      await startStocktake.mutateAsync(stocktake.id);
      toast.success("Stocktake started", {
        description: "You can now enter counted quantities.",
      });
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to start stocktake", { description: message });
    }
  };

  const handleCancel = async (stocktake: Stocktake) => {
    try {
      await cancelStocktake.mutateAsync(stocktake.id);
      toast.success("Stocktake cancelled");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to cancel stocktake", { description: message });
    }
  };

  const handleDeleteClick = (stocktake: Stocktake) => {
    setStocktakeToDelete(stocktake);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!stocktakeToDelete) return;

    try {
      await deleteStocktake.mutateAsync(stocktakeToDelete.id);
      toast.success("Stocktake deleted successfully");
      setDeleteDialogOpen(false);
      setStocktakeToDelete(null);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to delete stocktake", { description: message });
    }
  };

  // Client-side filtering and sorting
  const filteredAndSortedData = React.useMemo(() => {
    let data = stocktakes ?? [];

    // Filter by status
    if (statusFilter && statusFilter !== "all") {
      data = data.filter((item) => item.status === statusFilter);
    }

    // Filter by search (stocktake number or location name)
    if (searchParam) {
      const searchLower = searchParam.toLowerCase();
      data = data.filter(
        (item) =>
          item.stocktakeNumber.toLowerCase().includes(searchLower) ||
          item.locationName.toLowerCase().includes(searchLower) ||
          item.countedBy.toLowerCase().includes(searchLower)
      );
    }

    // Sort
    if (sortColumn) {
      const sortDir = sortDirection === "desc" ? -1 : 1;
      data = [...data].sort((a, b) => {
        const aValue = a[sortColumn as keyof Stocktake];
        const bValue = b[sortColumn as keyof Stocktake];

        if (aValue === null || aValue === undefined) return 1;
        if (bValue === null || bValue === undefined) return -1;

        if (typeof aValue === "string" && typeof bValue === "string") {
          return aValue.localeCompare(bValue) * sortDir;
        }
        if (typeof aValue === "number" && typeof bValue === "number") {
          return (aValue - bValue) * sortDir;
        }
        return String(aValue).localeCompare(String(bValue)) * sortDir;
      });
    }

    return data;
  }, [stocktakes, statusFilter, searchParam, sortColumn, sortDirection]);

  const renderActions = (stocktake: Stocktake) => {
    const actions: React.ReactNode[] = [];

    // View button always available
    actions.push(
      <Button key="view" variant="ghost" size="sm" asChild>
        <Link href={`/stock/stocktakes/${stocktake.id}`}>View</Link>
      </Button>
    );

    // Workflow actions based on status
    switch (stocktake.status) {
      case "Draft":
        actions.push(
          <Button
            key="start"
            variant="ghost"
            size="sm"
            onClick={(e) => {
              e.stopPropagation();
              handleStart(stocktake);
            }}
            disabled={startStocktake.isPending}
          >
            Start Count
          </Button>
        );
        actions.push(
          <Button
            key="cancel"
            variant="ghost"
            size="sm"
            className="text-destructive hover:text-destructive"
            onClick={(e) => {
              e.stopPropagation();
              handleCancel(stocktake);
            }}
            disabled={cancelStocktake.isPending}
          >
            Cancel
          </Button>
        );
        actions.push(
          <Button
            key="delete"
            variant="ghost"
            size="sm"
            className="text-destructive hover:text-destructive"
            onClick={(e) => {
              e.stopPropagation();
              handleDeleteClick(stocktake);
            }}
          >
            Delete
          </Button>
        );
        break;
      case "InProgress":
        actions.push(
          <Button
            key="cancel"
            variant="ghost"
            size="sm"
            className="text-destructive hover:text-destructive"
            onClick={(e) => {
              e.stopPropagation();
              handleCancel(stocktake);
            }}
            disabled={cancelStocktake.isPending}
          >
            Cancel
          </Button>
        );
        break;
      // Completed and Cancelled - only view
    }

    return (
      <div className="flex items-center justify-end gap-1">{actions}</div>
    );
  };

  const columns: Column<Stocktake>[] = [
    {
      key: "stocktakeNumber",
      header: "Stocktake #",
      sortable: true,
      className: "font-medium",
    },
    {
      key: "locationName",
      header: "Location",
      sortable: true,
    },
    {
      key: "countDate",
      header: "Count Date",
      sortable: true,
      render: (stocktake) =>
        new Date(stocktake.countDate).toLocaleDateString(),
    },
    {
      key: "status",
      header: "Status",
      sortable: true,
      render: (stocktake) => (
        <Badge variant={statusVariants[stocktake.status]}>
          {statusLabels[stocktake.status]}
        </Badge>
      ),
    },
    {
      key: "countedBy",
      header: "Counted By",
      sortable: true,
    },
    {
      key: "actions",
      header: "Actions",
      headerClassName: "text-right",
      className: "text-right",
      render: renderActions,
    },
  ];

  if (error) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-semibold tracking-tight">
              Stocktakes
            </h1>
            <p className="text-muted-foreground">
              Stock count and inventory verification
            </p>
          </div>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load stocktakes. Please try again.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Stocktakes</h1>
          <p className="text-muted-foreground">
            Stock count and inventory verification
          </p>
        </div>
        <Button asChild>
          <Link href="/stock/stocktakes/new">New Stocktake</Link>
        </Button>
      </div>

      <Tabs value={statusFilter} onValueChange={handleStatusChange}>
        <TabsList>
          {statusTabs.map((tab) => (
            <TabsTrigger key={tab.value} value={tab.value}>
              {tab.label}
            </TabsTrigger>
          ))}
        </TabsList>
      </Tabs>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-sm">
          <SearchIcon className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search by number, location, or counter..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="pl-9"
          />
        </div>
      </div>

      <DataTable
        columns={columns}
        data={filteredAndSortedData}
        isLoading={isLoading}
        emptyMessage="No stocktakes found"
        keyExtractor={(stocktake) => stocktake.id}
        sortColumn={sortColumn}
        sortDirection={sortDirection}
        onSort={handleSort}
        onRowClick={(stocktake) =>
          router.push(`/stock/stocktakes/${stocktake.id}`)
        }
      />

      <DeleteConfirmationDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        title="Delete Stocktake"
        description={`Are you sure you want to delete stocktake "${stocktakeToDelete?.stocktakeNumber}"? This action cannot be undone.`}
        onConfirm={handleDeleteConfirm}
        isLoading={deleteStocktake.isPending}
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
