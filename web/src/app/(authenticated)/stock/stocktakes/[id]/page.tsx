"use client";

import * as React from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { QRCodeSVG } from "qrcode.react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
  useStocktake,
  useStartStocktake,
  useUpdateStocktakeLine,
  useCompleteStocktake,
  useCancelStocktake,
} from "@/lib/api/stock/use-stocktakes";
import type { StocktakeLine, StocktakeStatus } from "@/types/stock";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

const statusVariants: Record<
  StocktakeStatus,
  "default" | "secondary" | "destructive" | "outline"
> = {
  Draft: "secondary",
  InProgress: "default",
  Completed: "outline",
  Cancelled: "destructive",
};

const statusLabels: Record<StocktakeStatus, string> = {
  Draft: "Draft",
  InProgress: "In Progress",
  Completed: "Completed",
  Cancelled: "Cancelled",
};

function getCountUrl(stocktakeId: string, lineId: string): string {
  if (typeof window !== "undefined") {
    return `${window.location.origin}/stock/stocktakes/${stocktakeId}/count/${lineId}`;
  }
  return `/stock/stocktakes/${stocktakeId}/count/${lineId}`;
}

export default function StocktakeDetailPage() {
  const params = useParams();
  const router = useRouter();
  const id = params.id as string;

  const { data: stocktake, isLoading, error } = useStocktake(id);
  const startStocktake = useStartStocktake();
  const updateLine = useUpdateStocktakeLine();
  const completeStocktake = useCompleteStocktake();
  const cancelStocktake = useCancelStocktake();

  // Local state for counted quantities (for optimistic updates and editing)
  const [localCounts, setLocalCounts] = React.useState<
    Record<string, string>
  >({});

  // Initialize local counts from stocktake data
  React.useEffect(() => {
    if (stocktake?.lines) {
      const initialCounts: Record<string, string> = {};
      stocktake.lines.forEach((line) => {
        if (line.countedQuantity !== null) {
          initialCounts[line.id] = String(line.countedQuantity);
        }
      });
      setLocalCounts(initialCounts);
    }
  }, [stocktake?.lines]);

  const handleStart = async () => {
    try {
      await startStocktake.mutateAsync(id);
      toast.success("Stocktake started", {
        description: "You can now enter counted quantities.",
      });
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to start stocktake", { description: message });
    }
  };

  const handleCountChange = (lineId: string, value: string) => {
    setLocalCounts((prev) => ({
      ...prev,
      [lineId]: value,
    }));
  };

  const handleCountBlur = async (lineId: string, value: string) => {
    // Parse the value
    const countedQuantity =
      value === "" ? null : parseInt(value, 10);

    // Validate
    if (value !== "" && (isNaN(countedQuantity!) || countedQuantity! < 0)) {
      toast.error("Invalid quantity", {
        description: "Please enter a valid non-negative number.",
      });
      return;
    }

    try {
      await updateLine.mutateAsync({
        stocktakeId: id,
        lineId,
        data: { countedQuantity },
      });
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to update count", { description: message });
    }
  };

  const handleComplete = async () => {
    // Check if all lines have been counted
    const uncountedLines =
      stocktake?.lines.filter(
        (line) =>
          line.countedQuantity === null &&
          localCounts[line.id] === undefined
      ) ?? [];

    if (uncountedLines.length > 0) {
      toast.error("Incomplete count", {
        description: `${uncountedLines.length} product(s) have not been counted yet.`,
      });
      return;
    }

    try {
      await completeStocktake.mutateAsync(id);
      toast.success("Stocktake completed", {
        description: "Stock adjustments have been created for any variances.",
      });
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to complete stocktake", { description: message });
    }
  };

  const handleCancel = async () => {
    try {
      await cancelStocktake.mutateAsync(id);
      toast.success("Stocktake cancelled");
      router.push("/stock/stocktakes");
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to cancel stocktake", { description: message });
    }
  };

  const getVariance = (line: StocktakeLine): number | null => {
    const countedStr = localCounts[line.id];
    const counted =
      countedStr !== undefined
        ? parseInt(countedStr, 10)
        : line.countedQuantity;

    if (counted === null || counted === undefined || isNaN(counted)) {
      return null;
    }

    return counted - line.systemQuantity;
  };

  const getVarianceClass = (variance: number | null): string => {
    if (variance === null) return "";
    if (variance > 0) return "text-green-600 font-medium";
    if (variance < 0) return "text-red-600 font-medium";
    return "text-muted-foreground";
  };

  const formatVariance = (variance: number | null): string => {
    if (variance === null) return "-";
    if (variance > 0) return `+${variance}`;
    return String(variance);
  };

  // Calculate summary stats
  const summaryStats = React.useMemo(() => {
    if (!stocktake?.lines) return null;

    let totalItems = stocktake.lines.length;
    let countedItems = 0;
    let positiveVariances = 0;
    let negativeVariances = 0;
    let zeroVariances = 0;

    stocktake.lines.forEach((line) => {
      const variance = getVariance(line);
      if (variance !== null) {
        countedItems++;
        if (variance > 0) positiveVariances++;
        else if (variance < 0) negativeVariances++;
        else zeroVariances++;
      }
    });

    return {
      totalItems,
      countedItems,
      positiveVariances,
      negativeVariances,
      zeroVariances,
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [stocktake?.lines, localCounts]);

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href="/stock/stocktakes">
              <ChevronLeftIcon className="mr-1 h-4 w-4" />
              Back to Stocktakes
            </Link>
          </Button>
        </div>
        <div className="flex items-center justify-center py-12">
          <LoadingSpinner className="h-8 w-8" />
        </div>
      </div>
    );
  }

  if (error || !stocktake) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href="/stock/stocktakes">
              <ChevronLeftIcon className="mr-1 h-4 w-4" />
              Back to Stocktakes
            </Link>
          </Button>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load stocktake details. Please try again.
          </p>
        </div>
      </div>
    );
  }

  const isEditable = stocktake.status === "InProgress";
  const canStart = stocktake.status === "Draft";
  const canComplete = stocktake.status === "InProgress";
  const canCancel =
    stocktake.status === "Draft" || stocktake.status === "InProgress";
  const canPrint =
    stocktake.status === "Draft" || stocktake.status === "InProgress";

  const handlePrintCountSheet = () => {
    window.open(`/stock/stocktakes/${id}/print`, "_blank");
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" asChild>
          <Link href="/stock/stocktakes">
            <ChevronLeftIcon className="mr-1 h-4 w-4" />
            Back to Stocktakes
          </Link>
        </Button>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-semibold tracking-tight">
              {stocktake.stocktakeNumber}
            </h1>
            <Badge variant={statusVariants[stocktake.status]}>
              {statusLabels[stocktake.status]}
            </Badge>
          </div>
          <p className="text-muted-foreground">{stocktake.locationName}</p>
        </div>
        <div className="flex items-center gap-2">
          {canPrint && (
            <Button
              variant="outline"
              onClick={handlePrintCountSheet}
            >
              <PrinterIcon className="mr-2 h-4 w-4" />
              Print Count Sheet
            </Button>
          )}
          {canStart && (
            <Button
              onClick={handleStart}
              disabled={startStocktake.isPending}
            >
              {startStocktake.isPending ? (
                <>
                  <LoadingSpinner className="mr-2 h-4 w-4" />
                  Starting...
                </>
              ) : (
                "Start Count"
              )}
            </Button>
          )}
          {canComplete && (
            <Button
              onClick={handleComplete}
              disabled={completeStocktake.isPending}
            >
              {completeStocktake.isPending ? (
                <>
                  <LoadingSpinner className="mr-2 h-4 w-4" />
                  Completing...
                </>
              ) : (
                "Complete Stocktake"
              )}
            </Button>
          )}
          {canCancel && (
            <Button
              variant="destructive"
              onClick={handleCancel}
              disabled={cancelStocktake.isPending}
            >
              Cancel
            </Button>
          )}
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Stocktake Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <p className="text-muted-foreground">Stocktake Number</p>
                <p className="font-medium">{stocktake.stocktakeNumber}</p>
              </div>
              <div>
                <p className="text-muted-foreground">Location</p>
                <p className="font-medium">
                  {stocktake.locationCode} - {stocktake.locationName}
                </p>
              </div>
              <div>
                <p className="text-muted-foreground">Count Date</p>
                <p className="font-medium">
                  {new Date(stocktake.countDate).toLocaleDateString()}
                </p>
              </div>
              <div>
                <p className="text-muted-foreground">Counted By</p>
                <p className="font-medium">{stocktake.countedBy}</p>
              </div>
            </div>
            {stocktake.notes && (
              <div>
                <p className="text-muted-foreground text-sm">Notes</p>
                <p className="text-sm mt-1">{stocktake.notes}</p>
              </div>
            )}
          </CardContent>
        </Card>

        {summaryStats && (
          <Card>
            <CardHeader>
              <CardTitle>Count Summary</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <p className="text-muted-foreground">Total Products</p>
                  <p className="font-medium text-lg">
                    {summaryStats.totalItems}
                  </p>
                </div>
                <div>
                  <p className="text-muted-foreground">Counted</p>
                  <p className="font-medium text-lg">
                    {summaryStats.countedItems} / {summaryStats.totalItems}
                  </p>
                </div>
                <div>
                  <p className="text-muted-foreground">Over Stock</p>
                  <p className="font-medium text-lg text-green-600">
                    {summaryStats.positiveVariances}
                  </p>
                </div>
                <div>
                  <p className="text-muted-foreground">Under Stock</p>
                  <p className="font-medium text-lg text-red-600">
                    {summaryStats.negativeVariances}
                  </p>
                </div>
              </div>
              {isEditable && summaryStats.countedItems < summaryStats.totalItems && (
                <p className="text-amber-600 text-sm mt-4">
                  {summaryStats.totalItems - summaryStats.countedItems} product(s) remaining to count
                </p>
              )}
            </CardContent>
          </Card>
        )}
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Count Lines</CardTitle>
        </CardHeader>
        <CardContent>
          {stocktake.lines.length === 0 ? (
            <div className="rounded-lg border border-dashed p-8 text-center">
              <p className="text-muted-foreground">
                No products found at this location.
              </p>
            </div>
          ) : (
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    {isEditable && (
                      <TableHead className="w-[160px] text-center">
                        Scan
                      </TableHead>
                    )}
                    <TableHead className="w-[120px]">Product Code</TableHead>
                    <TableHead>Product Name</TableHead>
                    <TableHead className="w-[80px]">Bay</TableHead>
                    <TableHead className="w-[100px] text-right">
                      System Qty
                    </TableHead>
                    <TableHead className="w-[120px] text-right">
                      Counted Qty
                    </TableHead>
                    <TableHead className="w-[100px] text-right">
                      Variance
                    </TableHead>
                    <TableHead className="w-[130px]">
                      Variance Reason
                    </TableHead>
                    {stocktake.status === "Completed" && (
                      <TableHead className="w-[100px] text-center">
                        Adjusted
                      </TableHead>
                    )}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {stocktake.lines.map((line, index) => {
                    const variance = getVariance(line);
                    const countValue = localCounts[line.id] ??
                      (line.countedQuantity !== null ? String(line.countedQuantity) : "");

                    return (
                      <TableRow
                        key={line.id}
                        className={cn(
                          variance !== null && variance !== 0 && "bg-muted/30"
                        )}
                      >
                        {isEditable && (
                          <TableCell className="text-center py-2">
                            <Link
                              href={`/stock/stocktakes/${id}/count/${line.id}`}
                              className="inline-block hover:opacity-80 transition-opacity"
                              title="Scan to count this product"
                            >
                              <QRCodeSVG
                                value={getCountUrl(id, line.id)}
                                size={150}
                                level="L"
                                includeMargin={false}
                              />
                            </Link>
                          </TableCell>
                        )}
                        <TableCell className="font-medium">
                          {line.productCode}
                        </TableCell>
                        <TableCell>{line.productName}</TableCell>
                        <TableCell>
                          {line.bayCode ? (
                            <span className="font-medium text-sm">{line.bayCode}</span>
                          ) : (
                            <span className="text-muted-foreground">-</span>
                          )}
                        </TableCell>
                        <TableCell className="text-right">
                          {line.systemQuantity}
                        </TableCell>
                        <TableCell className="text-right">
                          {isEditable ? (
                            <Input
                              type="number"
                              min="0"
                              className="w-full text-right"
                              value={countValue}
                              onChange={(e) =>
                                handleCountChange(line.id, e.target.value)
                              }
                              onBlur={(e) =>
                                handleCountBlur(line.id, e.target.value)
                              }
                              onKeyDown={(e) => {
                                if (e.key === "Tab" || e.key === "Enter") {
                                  // Let the default tab behavior work
                                  // But save on blur will handle the update
                                }
                              }}
                              tabIndex={index + 1}
                              placeholder="Enter count"
                            />
                          ) : (
                            <span>
                              {line.countedQuantity !== null
                                ? line.countedQuantity
                                : "-"}
                            </span>
                          )}
                        </TableCell>
                        <TableCell
                          className={cn(
                            "text-right",
                            getVarianceClass(variance)
                          )}
                        >
                          {formatVariance(variance)}
                        </TableCell>
                        <TableCell>
                          {line.varianceReason ? (
                            <Badge variant="secondary" className="text-xs">
                              {line.varianceReason}
                            </Badge>
                          ) : (
                            <span className="text-muted-foreground">-</span>
                          )}
                        </TableCell>
                        {stocktake.status === "Completed" && (
                          <TableCell className="text-center">
                            {line.adjustmentCreated ? (
                              <Badge variant="outline" className="text-xs">
                                Yes
                              </Badge>
                            ) : (
                              <span className="text-muted-foreground">-</span>
                            )}
                          </TableCell>
                        )}
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>

      {stocktake.status === "Completed" && (
        <Card>
          <CardHeader>
            <CardTitle>Adjustments</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground">
              Stock adjustments have been automatically created for products
              with variances. These adjustments update the stock levels to
              match the counted quantities.
            </p>
            {stocktake.lines.some((line) => line.adjustmentCreated) && (
              <div className="mt-4">
                <p className="text-sm font-medium">
                  Products adjusted:{" "}
                  {stocktake.lines.filter((l) => l.adjustmentCreated).length}
                </p>
              </div>
            )}
          </CardContent>
        </Card>
      )}
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

function PrinterIcon({ className }: { className?: string }) {
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
        d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z"
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
