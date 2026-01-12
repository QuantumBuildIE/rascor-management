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
  useStockOrders,
  useSubmitStockOrder,
  useReadyForCollectionStockOrder,
  useCancelStockOrder,
  useDeleteStockOrder,
} from "@/lib/api/stock/use-stock-orders";
import type { StockOrder, StockOrderStatus } from "@/types/stock";
import { toast } from "sonner";
import { ApproveOrderDialog } from "@/components/stock/approve-order-dialog";
import { RejectOrderDialog } from "@/components/stock/reject-order-dialog";
import { CollectOrderDialog } from "@/components/stock/collect-order-dialog";

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
  { value: "PendingApproval", label: "Pending Approval" },
  { value: "Approved", label: "Approved" },
  { value: "AwaitingPick", label: "Awaiting Pick" },
  { value: "ReadyForCollection", label: "Ready" },
  { value: "Collected", label: "Collected" },
  { value: "Cancelled", label: "Cancelled" },
];

const statusVariants: Record<StockOrderStatus, "default" | "secondary" | "destructive" | "outline"> = {
  Draft: "secondary",
  PendingApproval: "outline",
  Approved: "default",
  AwaitingPick: "default",
  ReadyForCollection: "default",
  Collected: "secondary",
  Cancelled: "destructive",
};

const statusLabels: Record<StockOrderStatus, string> = {
  Draft: "Draft",
  PendingApproval: "Pending Approval",
  Approved: "Approved",
  AwaitingPick: "Awaiting Pick",
  ReadyForCollection: "Ready for Collection",
  Collected: "Collected",
  Cancelled: "Cancelled",
};

export default function StockOrdersPage() {
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
  const [orderToDelete, setOrderToDelete] = React.useState<StockOrder | null>(null);
  const [approveDialogOpen, setApproveDialogOpen] = React.useState(false);
  const [orderToApprove, setOrderToApprove] = React.useState<StockOrder | null>(null);
  const [rejectDialogOpen, setRejectDialogOpen] = React.useState(false);
  const [orderToReject, setOrderToReject] = React.useState<StockOrder | null>(null);
  const [collectDialogOpen, setCollectDialogOpen] = React.useState(false);
  const [orderToCollect, setOrderToCollect] = React.useState<StockOrder | null>(null);

  // Update URL when search changes (debounced)
  React.useEffect(() => {
    if (debouncedSearch !== searchParam) {
      updateUrlParams({ search: debouncedSearch || null });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedSearch]);

  const { data: orders, isLoading, error } = useStockOrders();
  const submitOrder = useSubmitStockOrder();
  const readyForCollection = useReadyForCollectionStockOrder();
  const cancelOrder = useCancelStockOrder();
  const deleteOrder = useDeleteStockOrder();

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
  const handleSubmit = async (order: StockOrder) => {
    try {
      await submitOrder.mutateAsync(order.id);
      toast.success("Order submitted for approval");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to submit order", { description: message });
    }
  };

  const handleApproveClick = (order: StockOrder) => {
    setOrderToApprove(order);
    setApproveDialogOpen(true);
  };

  const handleRejectClick = (order: StockOrder) => {
    setOrderToReject(order);
    setRejectDialogOpen(true);
  };

  const handleReadyForCollection = async (order: StockOrder) => {
    try {
      await readyForCollection.mutateAsync(order.id);
      toast.success("Order marked as ready for collection");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to update order", { description: message });
    }
  };

  const handleCollectClick = (order: StockOrder) => {
    setOrderToCollect(order);
    setCollectDialogOpen(true);
  };

  const handleCancel = async (order: StockOrder) => {
    try {
      await cancelOrder.mutateAsync({ id: order.id });
      toast.success("Order cancelled");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to cancel order", { description: message });
    }
  };

  const handleDeleteClick = (order: StockOrder) => {
    setOrderToDelete(order);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!orderToDelete) return;

    try {
      await deleteOrder.mutateAsync(orderToDelete.id);
      toast.success("Order deleted successfully");
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

    // Filter by search (order number or site name)
    if (searchParam) {
      const searchLower = searchParam.toLowerCase();
      data = data.filter(
        (item) =>
          item.orderNumber.toLowerCase().includes(searchLower) ||
          item.siteName.toLowerCase().includes(searchLower)
      );
    }

    // Sort
    if (sortColumn) {
      const sortDir = sortDirection === "desc" ? -1 : 1;
      data = [...data].sort((a, b) => {
        const aValue = a[sortColumn as keyof StockOrder];
        const bValue = b[sortColumn as keyof StockOrder];

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

  const renderActions = (order: StockOrder) => {
    const actions: React.ReactNode[] = [];

    // View button always available
    actions.push(
      <Button key="view" variant="ghost" size="sm" asChild>
        <Link href={`/stock/orders/${order.id}`}>View</Link>
      </Button>
    );

    // Workflow actions based on status
    switch (order.status) {
      case "Draft":
        actions.push(
          <Button
            key="submit"
            variant="ghost"
            size="sm"
            onClick={(e) => {
              e.stopPropagation();
              handleSubmit(order);
            }}
            disabled={submitOrder.isPending}
          >
            Submit
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
      case "PendingApproval":
        actions.push(
          <Button
            key="approve"
            variant="ghost"
            size="sm"
            onClick={(e) => {
              e.stopPropagation();
              handleApproveClick(order);
            }}
          >
            Approve
          </Button>
        );
        actions.push(
          <Button
            key="reject"
            variant="ghost"
            size="sm"
            className="text-destructive hover:text-destructive"
            onClick={(e) => {
              e.stopPropagation();
              handleRejectClick(order);
            }}
          >
            Reject
          </Button>
        );
        break;
      case "Approved":
      case "AwaitingPick":
        actions.push(
          <Button
            key="ready"
            variant="ghost"
            size="sm"
            onClick={(e) => {
              e.stopPropagation();
              handleReadyForCollection(order);
            }}
            disabled={readyForCollection.isPending}
          >
            Mark Ready
          </Button>
        );
        actions.push(
          <Button
            key="print"
            variant="ghost"
            size="sm"
            onClick={(e) => {
              e.stopPropagation();
              window.open(`/stock/orders/${order.id}/print`, "_blank");
            }}
          >
            Print
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
              handleCancel(order);
            }}
            disabled={cancelOrder.isPending}
          >
            Cancel
          </Button>
        );
        break;
      case "ReadyForCollection":
        actions.push(
          <Button
            key="collect"
            variant="ghost"
            size="sm"
            onClick={(e) => {
              e.stopPropagation();
              handleCollectClick(order);
            }}
          >
            Collect
          </Button>
        );
        actions.push(
          <Button
            key="print"
            variant="ghost"
            size="sm"
            onClick={(e) => {
              e.stopPropagation();
              window.open(`/stock/orders/${order.id}/print`, "_blank");
            }}
          >
            Print
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

  const columns: Column<StockOrder>[] = [
    {
      key: "orderNumber",
      header: "Order #",
      sortable: true,
      className: "font-medium",
    },
    {
      key: "siteName",
      header: "Site",
      sortable: true,
    },
    {
      key: "sourceLocationName",
      header: "Source Location",
      sortable: true,
      className: "hidden lg:table-cell",
      headerClassName: "hidden lg:table-cell",
    },
    {
      key: "orderDate",
      header: "Order Date",
      sortable: true,
      className: "hidden md:table-cell",
      headerClassName: "hidden md:table-cell",
      render: (order) => new Date(order.orderDate).toLocaleDateString(),
    },
    {
      key: "requiredDate",
      header: "Required Date",
      sortable: true,
      className: "hidden xl:table-cell",
      headerClassName: "hidden xl:table-cell",
      render: (order) =>
        order.requiredDate ? (
          new Date(order.requiredDate).toLocaleDateString()
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
      key: "orderTotal",
      header: "Total",
      sortable: true,
      headerClassName: "text-right hidden sm:table-cell",
      className: "text-right hidden sm:table-cell",
      render: (order) => `€${order.orderTotal.toFixed(2)}`,
    },
    {
      key: "requestedBy",
      header: "Requested By",
      sortable: true,
      className: "hidden xl:table-cell",
      headerClassName: "hidden xl:table-cell",
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
            <h1 className="text-2xl font-semibold tracking-tight">Stock Orders</h1>
            <p className="text-muted-foreground">Manage stock orders from sites</p>
          </div>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load stock orders. Please try again.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Stock Orders</h1>
          <p className="text-muted-foreground">Manage stock orders from sites</p>
        </div>
        <Button asChild className="w-full sm:w-auto">
          <Link href="/stock/orders/new">New Stock Order</Link>
        </Button>
      </div>

      <Tabs value={statusFilter} onValueChange={handleStatusChange}>
        <TabsList className="flex-wrap h-auto">
          {statusTabs.map((tab) => (
            <TabsTrigger key={tab.value} value={tab.value} className="text-xs sm:text-sm">
              {tab.label}
            </TabsTrigger>
          ))}
        </TabsList>
      </Tabs>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 sm:max-w-sm">
          <SearchIcon className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search by order # or site..."
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
        emptyMessage="No stock orders found"
        keyExtractor={(order) => order.id}
        sortColumn={sortColumn}
        sortDirection={sortDirection}
        onSort={handleSort}
        onRowClick={(order) => router.push(`/stock/orders/${order.id}`)}
      />

      <DeleteConfirmationDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        title="Delete Stock Order"
        description={`Are you sure you want to delete order "${orderToDelete?.orderNumber}"? This action cannot be undone.`}
        onConfirm={handleDeleteConfirm}
        isLoading={deleteOrder.isPending}
      />

      <ApproveOrderDialog
        open={approveDialogOpen}
        onOpenChange={setApproveDialogOpen}
        order={orderToApprove}
      />

      <RejectOrderDialog
        open={rejectDialogOpen}
        onOpenChange={setRejectDialogOpen}
        order={orderToReject}
      />

      <CollectOrderDialog
        open={collectDialogOpen}
        onOpenChange={setCollectDialogOpen}
        order={orderToCollect}
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
