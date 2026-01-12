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
  usePurchaseOrders,
  useConfirmPurchaseOrder,
  useCancelPurchaseOrder,
  useDeletePurchaseOrder,
} from "@/lib/api/stock/use-purchase-orders";
import type { PurchaseOrder, PurchaseOrderStatus } from "@/types/stock";
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
  { value: "Confirmed", label: "Confirmed" },
  { value: "PartiallyReceived", label: "Partially Received" },
  { value: "FullyReceived", label: "Fully Received" },
  { value: "Cancelled", label: "Cancelled" },
];

const statusVariants: Record<PurchaseOrderStatus, "default" | "secondary" | "destructive" | "outline"> = {
  Draft: "secondary",
  Confirmed: "default",
  PartiallyReceived: "outline",
  FullyReceived: "secondary",
  Cancelled: "destructive",
};

const statusLabels: Record<PurchaseOrderStatus, string> = {
  Draft: "Draft",
  Confirmed: "Confirmed",
  PartiallyReceived: "Partially Received",
  FullyReceived: "Fully Received",
  Cancelled: "Cancelled",
};

export default function PurchaseOrdersPage() {
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
  const [orderToDelete, setOrderToDelete] = React.useState<PurchaseOrder | null>(null);

  // Update URL when search changes (debounced)
  React.useEffect(() => {
    if (debouncedSearch !== searchParam) {
      updateUrlParams({ search: debouncedSearch || null });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedSearch]);

  const { data: orders, isLoading, error } = usePurchaseOrders();
  const confirmOrder = useConfirmPurchaseOrder();
  const cancelOrder = useCancelPurchaseOrder();
  const deleteOrder = useDeletePurchaseOrder();

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
  const handleConfirm = async (order: PurchaseOrder) => {
    try {
      await confirmOrder.mutateAsync(order.id);
      toast.success("Purchase order confirmed");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to confirm order", { description: message });
    }
  };

  const handleCancel = async (order: PurchaseOrder) => {
    try {
      await cancelOrder.mutateAsync(order.id);
      toast.success("Purchase order cancelled");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to cancel order", { description: message });
    }
  };

  const handleDeleteClick = (order: PurchaseOrder) => {
    setOrderToDelete(order);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!orderToDelete) return;

    try {
      await deleteOrder.mutateAsync(orderToDelete.id);
      toast.success("Purchase order deleted successfully");
      setDeleteDialogOpen(false);
      setOrderToDelete(null);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to delete order", { description: message });
    }
  };

  // Client-side filtering and sorting
  const filteredAndSortedData = React.useMemo(() => {
    let data = orders ?? [];

    // Filter by status
    if (statusFilter && statusFilter !== "all") {
      data = data.filter((item) => item.status === statusFilter);
    }

    // Filter by search (PO number or supplier name)
    if (searchParam) {
      const searchLower = searchParam.toLowerCase();
      data = data.filter(
        (item) =>
          item.poNumber.toLowerCase().includes(searchLower) ||
          item.supplierName.toLowerCase().includes(searchLower)
      );
    }

    // Sort
    if (sortColumn) {
      const sortDir = sortDirection === "desc" ? -1 : 1;
      data = [...data].sort((a, b) => {
        const aValue = a[sortColumn as keyof PurchaseOrder];
        const bValue = b[sortColumn as keyof PurchaseOrder];

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
  }, [orders, statusFilter, searchParam, sortColumn, sortDirection]);

  const renderActions = (order: PurchaseOrder) => {
    const actions: React.ReactNode[] = [];

    // View button always available
    actions.push(
      <Button key="view" variant="ghost" size="sm" asChild>
        <Link href={`/stock/purchase-orders/${order.id}`}>View</Link>
      </Button>
    );

    // Workflow actions based on status
    switch (order.status) {
      case "Draft":
        actions.push(
          <Button key="edit" variant="ghost" size="sm" asChild>
            <Link
              href={`/stock/purchase-orders/${order.id}/edit`}
              onClick={(e) => e.stopPropagation()}
            >
              Edit
            </Link>
          </Button>
        );
        actions.push(
          <Button
            key="confirm"
            variant="ghost"
            size="sm"
            onClick={(e) => {
              e.stopPropagation();
              handleConfirm(order);
            }}
            disabled={confirmOrder.isPending}
          >
            Confirm
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
              handleDeleteClick(order);
            }}
          >
            Delete
          </Button>
        );
        break;
      case "Confirmed":
      case "PartiallyReceived":
        actions.push(
          <Button
            key="cancel"
            variant="ghost"
            size="sm"
            className="text-destructive hover:text-destructive"
            onClick={(e) => {
              e.stopPropagation();
              handleCancel(order);
            }}
            disabled={cancelOrder.isPending}
          >
            Cancel
          </Button>
        );
        break;
    }

    return (
      <div className="flex items-center justify-end gap-1">
        {actions}
      </div>
    );
  };

  const columns: Column<PurchaseOrder>[] = [
    {
      key: "poNumber",
      header: "PO Number",
      sortable: true,
      className: "font-medium",
    },
    {
      key: "supplierName",
      header: "Supplier",
      sortable: true,
    },
    {
      key: "orderDate",
      header: "Order Date",
      sortable: true,
      render: (order) => new Date(order.orderDate).toLocaleDateString(),
    },
    {
      key: "expectedDate",
      header: "Expected Date",
      sortable: true,
      render: (order) =>
        order.expectedDate ? (
          new Date(order.expectedDate).toLocaleDateString()
        ) : (
          <span className="text-muted-foreground">-</span>
        ),
    },
    {
      key: "status",
      header: "Status",
      sortable: true,
      render: (order) => (
        <Badge variant={statusVariants[order.status]}>
          {statusLabels[order.status]}
        </Badge>
      ),
    },
    {
      key: "totalValue",
      header: "Total Value",
      sortable: true,
      headerClassName: "text-right",
      className: "text-right",
      render: (order) => `€${order.totalValue.toFixed(2)}`,
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
            <h1 className="text-2xl font-semibold tracking-tight">Purchase Orders</h1>
            <p className="text-muted-foreground">Orders placed with suppliers</p>
          </div>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load purchase orders. Please try again.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Purchase Orders</h1>
          <p className="text-muted-foreground">Orders placed with suppliers</p>
        </div>
        <Button asChild>
          <Link href="/stock/purchase-orders/new">New Purchase Order</Link>
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
            placeholder="Search by PO # or supplier..."
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
        emptyMessage="No purchase orders found"
        keyExtractor={(order) => order.id}
        sortColumn={sortColumn}
        sortDirection={sortDirection}
        onSort={handleSort}
        onRowClick={(order) => router.push(`/stock/purchase-orders/${order.id}`)}
      />

      <DeleteConfirmationDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        title="Delete Purchase Order"
        description={`Are you sure you want to delete order "${orderToDelete?.poNumber}"? This action cannot be undone.`}
        onConfirm={handleDeleteConfirm}
        isLoading={deleteOrder.isPending}
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
