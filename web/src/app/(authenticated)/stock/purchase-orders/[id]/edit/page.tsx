"use client";

import Link from "next/link";
import { useRouter, useParams } from "next/navigation";
import { Button } from "@/components/ui/button";
import { PurchaseOrderForm } from "@/components/stock/purchase-order-form";
import { usePurchaseOrder } from "@/lib/api/stock/use-purchase-orders";
import type { PurchaseOrder } from "@/types/stock";

export default function EditPurchaseOrderPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;

  const { data: order, isLoading, error } = usePurchaseOrder(id);

  const handleSuccess = (updatedOrder: PurchaseOrder) => {
    router.push(`/stock/purchase-orders/${updatedOrder.id}`);
  };

  const handleCancel = () => {
    router.push(`/stock/purchase-orders/${id}`);
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href={`/stock/purchase-orders/${id}`}>
              <ChevronLeftIcon className="mr-1 h-4 w-4" />
              Back to Order
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

  // Can only edit draft orders
  if (order.status !== "Draft") {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href={`/stock/purchase-orders/${id}`}>
              <ChevronLeftIcon className="mr-1 h-4 w-4" />
              Back to Order
            </Link>
          </Button>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-muted-foreground">
            Only draft orders can be edited. This order is currently "{order.status}".
          </p>
          <Button className="mt-4" asChild>
            <Link href={`/stock/purchase-orders/${id}`}>View Order</Link>
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" asChild>
          <Link href={`/stock/purchase-orders/${id}`}>
            <ChevronLeftIcon className="mr-1 h-4 w-4" />
            Back to Order
          </Link>
        </Button>
      </div>

      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit Purchase Order</h1>
        <p className="text-muted-foreground">
          Update purchase order {order.poNumber}
        </p>
      </div>

      <PurchaseOrderForm order={order} onSuccess={handleSuccess} onCancel={handleCancel} />
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
