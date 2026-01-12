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
  usePurchaseOrder,
  useConfirmPurchaseOrder,
  useCancelPurchaseOrder,
  useDeletePurchaseOrder,
} from "@/lib/api/stock/use-purchase-orders";
import type { PurchaseOrderStatus } from "@/types/stock";
import { toast } from "sonner";
import { DeleteConfirmationDialog } from "@/components/shared/delete-confirmation-dialog";

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

export default function PurchaseOrderDetailPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;

  const { data: order, isLoading, error } = usePurchaseOrder(id);
  const confirmOrder = useConfirmPurchaseOrder();
  const cancelOrder = useCancelPurchaseOrder();
  const deleteOrder = useDeletePurchaseOrder();

  // Dialog states
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false);

  const handleConfirm = async () => {
    if (!order) return;
    try {
      await confirmOrder.mutateAsync(order.id);
      toast.success("Purchase order confirmed");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to confirm order", { description: message });
    }
  };

  const handleCancel = async () => {
    if (!order) return;
    try {
      await cancelOrder.mutateAsync(order.id);
      toast.success("Purchase order cancelled");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to cancel order", { description: message });
    }
  };

  const handleDeleteConfirm = async () => {
    if (!order) return;
    try {
      await deleteOrder.mutateAsync(order.id);
      toast.success("Purchase order deleted successfully");
      router.push("/stock/purchase-orders");
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
            <Link href="/stock/purchase-orders">
              <ChevronLeftIcon className="mr-1 h-4 w-4" />
              Back to Purchase Orders
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
            <Link href="/stock/purchase-orders">
              <ChevronLeftIcon className="mr-1 h-4 w-4" />
              Back to Purchase Orders
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
            key="confirm"
            onClick={handleConfirm}
            disabled={confirmOrder.isPending}
          >
            {confirmOrder.isPending ? "Confirming..." : "Confirm Order"}
          </Button>
        );
        actions.push(
          <Button
            key="edit"
            variant="outline"
            asChild
          >
            <Link href={`/stock/purchase-orders/${order.id}/edit`}>Edit Order</Link>
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
      case "Confirmed":
      case "PartiallyReceived":
        actions.push(
          <Button key="receive" asChild>
            <Link href={`/stock/goods-receipts/new?purchaseOrderId=${order.id}`}>
              Receive Goods
            </Link>
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
          <Link href="/stock/purchase-orders">
            <ChevronLeftIcon className="mr-1 h-4 w-4" />
            Back to Purchase Orders
          </Link>
        </Button>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-semibold tracking-tight">
              {order.poNumber}
            </h1>
            <Badge variant={statusVariants[order.status]}>
              {statusLabels[order.status]}
            </Badge>
          </div>
          <p className="text-muted-foreground">
            {order.supplierName}
          </p>
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
                <p className="text-muted-foreground">PO Number</p>
                <p className="font-medium">{order.poNumber}</p>
              </div>
              <div>
                <p className="text-muted-foreground">Supplier</p>
                <p className="font-medium">{order.supplierName}</p>
              </div>
              <div>
                <p className="text-muted-foreground">Order Date</p>
                <p className="font-medium">
                  {new Date(order.orderDate).toLocaleDateString()}
                </p>
              </div>
              <div>
                <p className="text-muted-foreground">Expected Date</p>
                <p className="font-medium">
                  {order.expectedDate
                    ? new Date(order.expectedDate).toLocaleDateString()
                    : "-"}
                </p>
              </div>
              <div>
                <p className="text-muted-foreground">Status</p>
                <p className="font-medium">{statusLabels[order.status]}</p>
              </div>
              <div>
                <p className="text-muted-foreground">Total Value</p>
                <p className="font-medium text-lg">{order.totalValue.toFixed(2)}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Additional Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {order.notes ? (
              <div>
                <p className="text-muted-foreground text-sm">Notes</p>
                <p className="text-sm mt-1">{order.notes}</p>
              </div>
            ) : (
              <p className="text-muted-foreground text-sm">No additional notes</p>
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
                <TableHead className="text-right">Qty Ordered</TableHead>
                <TableHead className="text-right">Qty Received</TableHead>
                <TableHead className="text-right">Unit Price</TableHead>
                <TableHead className="text-right">Line Total</TableHead>
                <TableHead>Status</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {order.lines.map((line) => (
                <TableRow key={line.id}>
                  <TableCell className="font-medium">{line.productCode}</TableCell>
                  <TableCell>{line.productName}</TableCell>
                  <TableCell className="text-right">{line.quantityOrdered}</TableCell>
                  <TableCell className="text-right">{line.quantityReceived}</TableCell>
                  <TableCell className="text-right">{line.unitPrice.toFixed(2)}</TableCell>
                  <TableCell className="text-right">{line.lineTotal.toFixed(2)}</TableCell>
                  <TableCell>
                    <Badge variant={
                      line.quantityReceived === 0 ? "secondary" :
                      line.quantityReceived >= line.quantityOrdered ? "default" : "outline"
                    }>
                      {line.status}
                    </Badge>
                  </TableCell>
                </TableRow>
              ))}
              <TableRow>
                <TableCell colSpan={5} className="text-right font-medium">
                  Total
                </TableCell>
                <TableCell className="text-right font-bold">
                  {order.totalValue.toFixed(2)}
                </TableCell>
                <TableCell></TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <DeleteConfirmationDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        title="Delete Purchase Order"
        description={`Are you sure you want to delete order "${order.poNumber}"? This action cannot be undone.`}
        onConfirm={handleDeleteConfirm}
        isLoading={deleteOrder.isPending}
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
