"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { PurchaseOrderForm } from "@/components/stock/purchase-order-form";
import type { PurchaseOrder } from "@/types/stock";

export default function NewPurchaseOrderPage() {
  const router = useRouter();

  const handleSuccess = (order: PurchaseOrder) => {
    router.push(`/stock/purchase-orders/${order.id}`);
  };

  const handleCancel = () => {
    router.push("/stock/purchase-orders");
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

      <div>
        <h1 className="text-2xl font-semibold tracking-tight">New Purchase Order</h1>
        <p className="text-muted-foreground">
          Create a new purchase order for a supplier
        </p>
      </div>

      <PurchaseOrderForm onSuccess={handleSuccess} onCancel={handleCancel} />
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
