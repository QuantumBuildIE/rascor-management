"use client";

import * as React from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { StockOrderForm } from "@/components/stock/stock-order-form";
import type { StockOrder } from "@/types/stock";

export default function NewStockOrderPage() {
  const router = useRouter();

  const handleSuccess = (order: StockOrder) => {
    router.push(`/stock/orders/${order.id}`);
  };

  const handleCancel = () => {
    router.push("/stock/orders");
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

      <div>
        <h1 className="text-2xl font-semibold tracking-tight">
          Create Stock Order
        </h1>
        <p className="text-muted-foreground">
          Create a new stock order to request items from the warehouse.
        </p>
      </div>

      <StockOrderForm onSuccess={handleSuccess} onCancel={handleCancel} />
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
