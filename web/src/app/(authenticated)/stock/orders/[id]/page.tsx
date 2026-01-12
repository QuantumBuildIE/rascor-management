"use client";

import * as React from "react";
import Link from "next/link";
import { useRouter, useParams } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  useStockOrder,
  useSubmitStockOrder,
  useReadyForCollectionStockOrder,
  useCancelStockOrder,
  useDeleteStockOrder,
} from "@/lib/api/stock/use-stock-orders";
import type { StockOrderStatus } from "@/types/stock";
import { toast } from "sonner";
import { DeleteConfirmationDialog } from "@/components/shared/delete-confirmation-dialog";
import { ApproveOrderDialog } from "@/components/stock/approve-order-dialog";
import { RejectOrderDialog } from "@/components/stock/reject-order-dialog";
import { CollectOrderDialog } from "@/components/stock/collect-order-dialog";
import { useHasAnyPermission } from "@/lib/auth/use-auth";

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

export default function StockOrderDetailPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;

  const { data: order, isLoading, error } = useStockOrder(id);
  const submitOrder = useSubmitStockOrder();
  const readyForCollection = useReadyForCollectionStockOrder();
  const cancelOrder = useCancelStockOrder();
  const deleteOrder = useDeleteStockOrder();

  // Check if user can view site details
  const canViewSites = useHasAnyPermission(["Core.Admin", "Core.ManageSites"]);

  // Dialog states
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false);
  const [approveDialogOpen, setApproveDialogOpen] = React.useState(false);
  const [rejectDialogOpen, setRejectDialogOpen] = React.useState(false);
  const [collectDialogOpen, setCollectDialogOpen] = React.useState(false);

  const handleSubmit = async () => {
    if (!order) return;
    try {
      await submitOrder.mutateAsync(order.id);
      toast.success("Order submitted for approval");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to submit order", { description: message });
    }
  };

  const handleReadyForCollection = async () => {
    if (!order) return;
    try {
      await readyForCollection.mutateAsync(order.id);
      toast.success("Order marked as ready for collection");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to update order", { description: message });
    }
  };

  const handleCancel = async () => {
    if (!order) return;
    try {
      await cancelOrder.mutateAsync({ id: order.id });
      toast.success("Order cancelled");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to cancel order", { description: message });
    }
  };

  const handleDeleteConfirm = async () => {
    if (!order) return;
    try {
      await deleteOrder.mutateAsync(order.id);
      toast.success("Order deleted successfully");
      router.push("/stock/orders");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to delete order", { description: message });
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href="/stock/orders">
              <ChevronLeftIcon className="mr-1 h-4 w-4" />
              Back to Orders
            </Link>
          </Button>
        </div>
        <div className="flex items-center justify-center py-12">
          <LoadingSpinner className="h-8 w-8" />
        </div>
      </div>
    );
  }

  if (error || !order) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href="/stock/orders">
              <ChevronLeftIcon className="mr-1 h-4 w-4" />
              Back to Orders
            </Link>
          </Button>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load order details. Please try again.
          </p>
        </div>
      </div>
    );
  }

  const renderWorkflowActions = () => {
    const actions: React.ReactNode[] = [];

    switch (order.status) {
      case "Draft":
        actions.push(
          <Button
            key="submit"
            onClick={handleSubmit}
            disabled={submitOrder.isPending}
          >
            {submitOrder.isPending ? "Submitting..." : "Submit for Approval"}
          </Button>
        );
        actions.push(
          <Button
            key="edit"
            variant="outline"
            asChild
          >
            <Link href={`/stock/orders/${order.id}/edit`}>Edit Order</Link>
          </Button>
        );
        actions.push(
          <Button
            key="delete"
            variant="destructive"
            onClick={() => setDeleteDialogOpen(true)}
          >
            Delete Order
          </Button>
        );
        break;
      case "PendingApproval":
        actions.push(
          <Button key="approve" onClick={() => setApproveDialogOpen(true)}>
            Approve Order
          </Button>
        );
        actions.push(
          <Button
            key="reject"
            variant="destructive"
            onClick={() => setRejectDialogOpen(true)}
          >
            Reject Order
          </Button>
        );
        break;
      case "Approved":
      case "AwaitingPick":
        actions.push(
          <Button
            key="ready"
            onClick={handleReadyForCollection}
            disabled={readyForCollection.isPending}
          >
            {readyForCollection.isPending ? "Processing..." : "Mark Ready for Collection"}
          </Button>
        );
        actions.push(
          <Button
            key="print"
            variant="outline"
            onClick={() => window.open(`/stock/orders/${order.id}/print`, "_blank")}
          >
            <PrinterIcon className="mr-2 h-4 w-4" />
            Print Docket
          </Button>
        );
        actions.push(
          <Button
            key="cancel"
            variant="destructive"
            onClick={handleCancel}
            disabled={cancelOrder.isPending}
          >
            {cancelOrder.isPending ? "Cancelling..." : "Cancel Order"}
          </Button>
        );
        break;
      case "ReadyForCollection":
        actions.push(
          <Button key="collect" onClick={() => setCollectDialogOpen(true)}>
            Mark as Collected
          </Button>
        );
        actions.push(
          <Button
            key="print"
            variant="outline"
            onClick={() => window.open(`/stock/orders/${order.id}/print`, "_blank")}
          >
            <PrinterIcon className="mr-2 h-4 w-4" />
            Print Docket
          </Button>
        );
        actions.push(
          <Button
            key="cancel"
            variant="destructive"
            onClick={handleCancel}
            disabled={cancelOrder.isPending}
          >
            {cancelOrder.isPending ? "Cancelling..." : "Cancel Order"}
          </Button>
        );
        break;
    }

    return actions.length > 0 ? (
      <div className="flex items-center gap-2">
        {actions}
      </div>
    ) : null;
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" asChild>
          <Link href="/stock/orders">
            <ChevronLeftIcon className="mr-1 h-4 w-4" />
            Back to Orders
          </Link>
        </Button>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-semibold tracking-tight">
              Order {order.orderNumber}
            </h1>
            <Badge variant={statusVariants[order.status]}>
              {statusLabels[order.status]}
            </Badge>
          </div>
          {canViewSites ? (
            <Link
              href={`/admin/sites/${order.siteId}`}
              className="text-muted-foreground hover:text-primary hover:underline"
            >
              {order.siteName}
            </Link>
          ) : (
            <p className="text-muted-foreground">{order.siteName}</p>
          )}
        </div>
        {renderWorkflowActions()}
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Order Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <p className="text-muted-foreground">Order Number</p>
                <p className="font-medium">{order.orderNumber}</p>
              </div>
              <div>
                <p className="text-muted-foreground">Site</p>
                {canViewSites ? (
                  <Link
                    href={`/admin/sites/${order.siteId}`}
                    className="font-medium text-primary hover:underline"
                  >
                    {order.siteName}
                  </Link>
                ) : (
                  <p className="font-medium">{order.siteName}</p>
                )}
              </div>
              <div>
                <p className="text-muted-foreground">Source Location</p>
                <p className="font-medium">{order.sourceLocationName}</p>
              </div>
              <div>
                <p className="text-muted-foreground">Order Date</p>
                <p className="font-medium">
                  {new Date(order.orderDate).toLocaleDateString()}
                </p>
              </div>
              <div>
                <p className="text-muted-foreground">Required Date</p>
                <p className="font-medium">
                  {order.requiredDate
                    ? new Date(order.requiredDate).toLocaleDateString()
                    : "-"}
                </p>
              </div>
              <div>
                <p className="text-muted-foreground">Requested By</p>
                <p className="font-medium">{order.requestedBy}</p>
              </div>
              <div>
                <p className="text-muted-foreground">Order Total</p>
                <p className="font-medium text-lg">€{order.orderTotal.toFixed(2)}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Status Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <p className="text-muted-foreground">Status</p>
                <p className="font-medium">{statusLabels[order.status]}</p>
              </div>
              {order.approvedBy && (
                <div>
                  <p className="text-muted-foreground">Approved By</p>
                  <p className="font-medium">{order.approvedBy}</p>
                </div>
              )}
              {order.approvedDate && (
                <div>
                  <p className="text-muted-foreground">Approved Date</p>
                  <p className="font-medium">
                    {new Date(order.approvedDate).toLocaleDateString()}
                  </p>
                </div>
              )}
              {order.collectedDate && (
                <div>
                  <p className="text-muted-foreground">Collected Date</p>
                  <p className="font-medium">
                    {new Date(order.collectedDate).toLocaleDateString()}
                  </p>
                </div>
              )}
            </div>
            {order.notes && (
              <div>
                <p className="text-muted-foreground text-sm">Notes</p>
                <p className="text-sm mt-1">{order.notes}</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Order Lines</CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Product Code</TableHead>
                <TableHead>Product Name</TableHead>
                <TableHead className="text-right">Quantity</TableHead>
                <TableHead className="text-right">Unit Price</TableHead>
                <TableHead className="text-right">Line Total</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {order.lines.map((line) => (
                <TableRow key={line.id}>
                  <TableCell className="font-medium">{line.productCode}</TableCell>
                  <TableCell>{line.productName}</TableCell>
                  <TableCell className="text-right">{line.quantityRequested}</TableCell>
                  <TableCell className="text-right">€{line.unitPrice.toFixed(2)}</TableCell>
                  <TableCell className="text-right">€{line.lineTotal.toFixed(2)}</TableCell>
                </TableRow>
              ))}
              <TableRow>
                <TableCell colSpan={4} className="text-right font-medium">
                  Total
                </TableCell>
                <TableCell className="text-right font-bold">
                  €{order.orderTotal.toFixed(2)}
                </TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <DeleteConfirmationDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        title="Delete Stock Order"
        description={`Are you sure you want to delete order "${order.orderNumber}"? This action cannot be undone.`}
        onConfirm={handleDeleteConfirm}
        isLoading={deleteOrder.isPending}
      />

      <ApproveOrderDialog
        open={approveDialogOpen}
        onOpenChange={setApproveDialogOpen}
        order={order}
      />

      <RejectOrderDialog
        open={rejectDialogOpen}
        onOpenChange={setRejectDialogOpen}
        order={order}
      />

      <CollectOrderDialog
        open={collectDialogOpen}
        onOpenChange={setCollectDialogOpen}
        order={order}
      />
    </div>
  );
}

function ChevronLeftIcon({ className }: { className?: string }) {
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
        d="M15 19l-7-7 7-7"
      />
    </svg>
  );
}

function LoadingSpinner({ className }: { className?: string }) {
  return (
    <svg
      className={`animate-spin ${className}`}
      xmlns="http://www.w3.org/2000/svg"
      fill="none"
      viewBox="0 0 24 24"
    >
      <circle
        className="opacity-25"
        cx="12"
        cy="12"
        r="10"
        stroke="currentColor"
        strokeWidth="4"
      />
      <path
        className="opacity-75"
        fill="currentColor"
        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
      />
    </svg>
  );
}

function PrinterIcon({ className }: { className?: string }) {
  return (
    <svg
      className={className}
      fill="none"
      stroke="currentColor"
      viewBox="0 0 24 24"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth={2}
        d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z"
      />
    </svg>
  );
}
