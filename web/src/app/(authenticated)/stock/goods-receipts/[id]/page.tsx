"use client";

import * as React from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useGoodsReceipt } from "@/lib/api/stock/use-goods-receipts";

export default function GoodsReceiptDetailPage() {
  const params = useParams();
  const id = params.id as string;

  const { data: receipt, isLoading, error } = useGoodsReceipt(id);

  if (isLoading) {
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

  if (error || !receipt) {
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
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load receipt details. Please try again.
          </p>
        </div>
      </div>
    );
  }

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

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">
            {receipt.grnNumber}
          </h1>
          <p className="text-muted-foreground">
            {receipt.supplierName}
          </p>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Receipt Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <p className="text-muted-foreground">GRN Number</p>
                <p className="font-medium">{receipt.grnNumber}</p>
              </div>
              <div>
                <p className="text-muted-foreground">Supplier</p>
                <p className="font-medium">{receipt.supplierName}</p>
              </div>
              <div>
                <p className="text-muted-foreground">Receipt Date</p>
                <p className="font-medium">
                  {new Date(receipt.receiptDate).toLocaleDateString()}
                </p>
              </div>
              <div>
                <p className="text-muted-foreground">Received By</p>
                <p className="font-medium">{receipt.receivedBy}</p>
              </div>
              <div>
                <p className="text-muted-foreground">Location</p>
                <p className="font-medium">{receipt.locationName}</p>
              </div>
              {receipt.deliveryNoteRef && (
                <div>
                  <p className="text-muted-foreground">Delivery Note Ref</p>
                  <p className="font-medium">{receipt.deliveryNoteRef}</p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Purchase Order</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {receipt.poNumber ? (
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <p className="text-muted-foreground">PO Number</p>
                  <Link
                    href={`/stock/purchase-orders/${receipt.purchaseOrderId}`}
                    className="font-medium text-primary hover:underline"
                  >
                    {receipt.poNumber}
                  </Link>
                </div>
              </div>
            ) : (
              <p className="text-muted-foreground text-sm">
                Not linked to a purchase order
              </p>
            )}
            {receipt.notes && (
              <div>
                <p className="text-muted-foreground text-sm">Notes</p>
                <p className="text-sm mt-1">{receipt.notes}</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Receipt Lines</CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Product Code</TableHead>
                <TableHead>Product Name</TableHead>
                <TableHead className="text-right">Qty Received</TableHead>
                <TableHead className="text-right">Qty Rejected</TableHead>
                <TableHead>Rejection Reason</TableHead>
                <TableHead>Batch</TableHead>
                <TableHead>Notes</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {receipt.lines.map((line) => (
                <TableRow key={line.id}>
                  <TableCell className="font-medium">{line.productCode}</TableCell>
                  <TableCell>{line.productName}</TableCell>
                  <TableCell className="text-right">{line.quantityReceived}</TableCell>
                  <TableCell className="text-right">
                    {line.quantityRejected > 0 ? (
                      <span className="text-destructive">{line.quantityRejected}</span>
                    ) : (
                      "-"
                    )}
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {line.rejectionReason || "-"}
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {line.batchNumber || "-"}
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {line.notes || "-"}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
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
