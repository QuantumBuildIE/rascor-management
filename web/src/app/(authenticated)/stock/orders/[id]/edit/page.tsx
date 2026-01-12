"use client";

import * as React from "react";
import Link from "next/link";
import { useRouter, useParams } from "next/navigation";
import { Button } from "@/components/ui/button";
import { StockOrderForm } from "@/components/stock/stock-order-form";
import { useStockOrder } from "@/lib/api/stock/use-stock-orders";
import type { StockOrder } from "@/types/stock";

export default function EditStockOrderPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;

  const { data: order, isLoading, error } = useStockOrder(id);

  const handleSuccess = (updatedOrder: StockOrder) => {
    router.push(`/stock/orders/${updatedOrder.id}`);
  };

  const handleCancel = () => {
    router.push(`/stock/orders/${id}`);
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href={`/stock/orders/${id}`}>
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

  if (order.status !== "Draft") {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href={`/stock/orders/${id}`}>
              <ChevronLeftIcon className="mr-1 h-4 w-4" />
              Back to Order
            </Link>
          </Button>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <h2 className="text-lg font-semibold">Cannot Edit Order</h2>
          <p className="mt-2 text-muted-foreground">
            Only orders with Draft status can be edited. This order has status:{" "}
            <span className="font-medium">{order.status}</span>
          </p>
          <Button asChild className="mt-4">
            <Link href={`/stock/orders/${id}`}>View Order</Link>
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" asChild>
          <Link href={`/stock/orders/${id}`}>
            <ChevronLeftIcon className="mr-1 h-4 w-4" />
            Back to Order
          </Link>
        </Button>
      </div>

      <div>
        <h1 className="text-2xl font-semibold tracking-tight">
          Edit Stock Order
        </h1>
        <p className="text-muted-foreground">
          Order {order.orderNumber} - {order.siteName}
        </p>
      </div>

      <StockOrderForm
        order={order}
        onSuccess={handleSuccess}
        onCancel={handleCancel}
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
