"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { Button } from "@/components/ui/button";
import { GoodsReceiptForm } from "@/components/stock/goods-receipt-form";
import { usePurchaseOrder } from "@/lib/api/stock/use-purchase-orders";
import type { GoodsReceipt } from "@/types/stock";

export default function NewGoodsReceiptPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const purchaseOrderId = searchParams.get("purchaseOrderId") || undefined;

  // If we have a purchaseOrderId, fetch the PO to pre-populate the form
  const { data: purchaseOrder, isLoading } = usePurchaseOrder(purchaseOrderId ?? "");

  const handleSuccess = (receipt: GoodsReceipt) => {
    router.push(`/stock/goods-receipts/${receipt.id}`);
  };

  const handleCancel = () => {
    if (purchaseOrderId) {
      router.push(`/stock/purchase-orders/${purchaseOrderId}`);
    } else {
      router.push("/stock/goods-receipts");
    }
  };

  // Show loading state if we're fetching a PO
  if (purchaseOrderId && isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href="/stock/goods-receipts">
              <ChevronLeftIcon className="mr-1 h-4 w-4" />
              Back to Goods Receipts
            </Link>
          </Button>
        </div>
        <div className="flex items-center justify-center py-12">
          <LoadingSpinner className="h-8 w-8" />
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" asChild>
          <Link href={purchaseOrderId ? `/stock/purchase-orders/${purchaseOrderId}` : "/stock/goods-receipts"}>
            <ChevronLeftIcon className="mr-1 h-4 w-4" />
            {purchaseOrderId ? "Back to Purchase Order" : "Back to Goods Receipts"}
          </Link>
        </Button>
      </div>

      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Receive Goods</h1>
        <p className="text-muted-foreground">
          {purchaseOrder
            ? `Record goods received against ${purchaseOrder.poNumber}`
            : "Record incoming goods from a supplier"}
        </p>
      </div>

      <GoodsReceiptForm
        initialPurchaseOrderId={purchaseOrderId}
        initialPurchaseOrder={purchaseOrder}
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
