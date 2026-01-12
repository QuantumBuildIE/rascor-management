"use client";

import * as React from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useStocktake, useUpdateStocktakeLine } from "@/lib/api/stock/use-stocktakes";
import { toast } from "sonner";

const VARIANCE_REASONS = [
  "Damaged",
  "Missing",
  "Found",
  "Data Entry Error",
  "Theft",
  "Other",
] as const;

export default function QuickCountPage() {
  const params = useParams();
  const router = useRouter();
  const stocktakeId = params.id as string;
  const lineId = params.lineId as string;

  const { data: stocktake, isLoading, error } = useStocktake(stocktakeId);
  const updateLine = useUpdateStocktakeLine();

  const [countValue, setCountValue] = React.useState("");
  const [varianceReason, setVarianceReason] = React.useState<string | null>(null);
  const [saved, setSaved] = React.useState(false);

  // Find the specific line
  const line = stocktake?.lines.find((l) => l.id === lineId);

  // Initialize count value and variance reason from line data
  React.useEffect(() => {
    if (line?.countedQuantity !== null && line?.countedQuantity !== undefined) {
      setCountValue(String(line.countedQuantity));
    }
    if (line?.varianceReason) {
      setVarianceReason(line.varianceReason);
    }
  }, [line?.countedQuantity, line?.varianceReason]);

  const handleSave = async () => {
    if (!line) return;

    const countedQuantity = countValue === "" ? null : parseInt(countValue, 10);

    if (countValue !== "" && (isNaN(countedQuantity!) || countedQuantity! < 0)) {
      toast.error("Invalid quantity", {
        description: "Please enter a valid non-negative number.",
      });
      return;
    }

    // Check if variance reason is required
    const hasVariance = countedQuantity !== null && countedQuantity !== line.systemQuantity;
    if (hasVariance && !varianceReason) {
      toast.error("Variance reason required", {
        description: "Please select a reason for the variance.",
      });
      return;
    }

    try {
      await updateLine.mutateAsync({
        stocktakeId,
        lineId,
        data: {
          countedQuantity,
          varianceReason: hasVariance ? varianceReason : null,
        },
      });
      setSaved(true);
      toast.success("Count saved!", {
        description: `${line?.productName} counted as ${countedQuantity ?? "not counted"}`,
      });
    } catch (err) {
      const message = err instanceof Error ? err.message : "An error occurred";
      toast.error("Failed to save count", { description: message });
    }
  };

  const handleSaveAndNext = async () => {
    await handleSave();

    // Find next uncounted line
    if (stocktake?.lines) {
      const currentIndex = stocktake.lines.findIndex((l) => l.id === lineId);
      const nextLine = stocktake.lines.find(
        (l, index) => index > currentIndex && l.countedQuantity === null
      );

      if (nextLine) {
        router.push(`/stock/stocktakes/${stocktakeId}/count/${nextLine.id}`);
      } else {
        toast.info("All products counted!", {
          description: "You have counted all products in this stocktake.",
        });
        router.push(`/stock/stocktakes/${stocktakeId}`);
      }
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center p-4">
        <div className="text-center">
          <LoadingSpinner className="h-8 w-8 mx-auto" />
          <p className="mt-2 text-muted-foreground">Loading...</p>
        </div>
      </div>
    );
  }

  if (error || !stocktake) {
    return (
      <div className="min-h-screen flex items-center justify-center p-4">
        <div className="text-center">
          <p className="text-destructive mb-4">Failed to load stocktake.</p>
          <Link href="/stock/stocktakes" className="text-primary underline">
            Back to Stocktakes
          </Link>
        </div>
      </div>
    );
  }

  if (!line) {
    return (
      <div className="min-h-screen flex items-center justify-center p-4">
        <div className="text-center">
          <p className="text-destructive mb-4">Product not found in this stocktake.</p>
          <Link
            href={`/stock/stocktakes/${stocktakeId}`}
            className="text-primary underline"
          >
            Back to Stocktake
          </Link>
        </div>
      </div>
    );
  }

  const isEditable = stocktake.status === "InProgress";
  const canCount = stocktake.status === "Draft" || stocktake.status === "InProgress";

  return (
    <div className="min-h-screen bg-gray-50 p-4">
      {/* Header */}
      <div className="mb-6">
        <Link
          href={`/stock/stocktakes/${stocktakeId}`}
          className="inline-flex items-center text-sm text-primary hover:underline"
        >
          <ChevronLeftIcon className="h-4 w-4 mr-1" />
          Back to Stocktake
        </Link>
      </div>

      {/* Stocktake Info */}
      <div className="mb-4 text-sm text-muted-foreground">
        <p>
          <span className="font-medium">{stocktake.stocktakeNumber}</span> at{" "}
          <span className="font-medium">{stocktake.locationName}</span>
        </p>
      </div>

      {/* Product Card */}
      <Card className="mb-6">
        <CardHeader className="pb-3">
          <div className="flex items-start justify-between">
            <div>
              <CardTitle className="text-xl">{line.productName}</CardTitle>
              <p className="text-sm text-muted-foreground mt-1">{line.productCode}</p>
            </div>
            {saved && (
              <Badge variant="outline" className="bg-green-50 text-green-700 border-green-200">
                Saved
              </Badge>
            )}
          </div>
        </CardHeader>
        <CardContent>
          {/* Expected Quantity */}
          <div className="mb-6 p-4 bg-gray-100 rounded-lg">
            <p className="text-sm text-muted-foreground">Expected Quantity (System)</p>
            <p className="text-3xl font-bold text-gray-900">{line.systemQuantity}</p>
          </div>

          {/* Count Input */}
          {canCount ? (
            <>
              <div className="mb-6">
                <label className="block text-sm font-medium mb-2">
                  Counted Quantity
                </label>
                <Input
                  type="number"
                  min="0"
                  inputMode="numeric"
                  pattern="[0-9]*"
                  className="text-center text-3xl font-bold h-20"
                  placeholder="0"
                  value={countValue}
                  onChange={(e) => {
                    setCountValue(e.target.value);
                    setSaved(false);
                  }}
                  disabled={!isEditable && stocktake.status !== "Draft"}
                  autoFocus
                />
              </div>

              {/* Variance Display */}
              {countValue !== "" && !isNaN(parseInt(countValue)) && (
                <div className="mb-6 p-4 rounded-lg border">
                  <p className="text-sm text-muted-foreground">Variance</p>
                  <p
                    className={`text-2xl font-bold ${
                      parseInt(countValue) - line.systemQuantity > 0
                        ? "text-green-600"
                        : parseInt(countValue) - line.systemQuantity < 0
                        ? "text-red-600"
                        : "text-gray-600"
                    }`}
                  >
                    {parseInt(countValue) - line.systemQuantity > 0 ? "+" : ""}
                    {parseInt(countValue) - line.systemQuantity}
                  </p>
                </div>
              )}

              {/* Variance Reason - show when there's a variance */}
              {countValue !== "" &&
                !isNaN(parseInt(countValue)) &&
                parseInt(countValue) !== line.systemQuantity && (
                  <div className="mb-6">
                    <label className="block text-sm font-medium mb-2">
                      Variance Reason *
                    </label>
                    <Select
                      value={varianceReason ?? ""}
                      onValueChange={(value) => {
                        setVarianceReason(value || null);
                        setSaved(false);
                      }}
                    >
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Select reason for variance" />
                      </SelectTrigger>
                      <SelectContent>
                        {VARIANCE_REASONS.map((reason) => (
                          <SelectItem key={reason} value={reason}>
                            {reason}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                )}

              {/* Action Buttons */}
              <div className="space-y-3">
                {!isEditable && stocktake.status === "Draft" ? (
                  <div className="p-4 bg-amber-50 border border-amber-200 rounded-lg text-center">
                    <p className="text-amber-800 text-sm">
                      Start the stocktake first to begin counting.
                    </p>
                    <Link
                      href={`/stock/stocktakes/${stocktakeId}`}
                      className="text-primary underline text-sm"
                    >
                      Go to Stocktake
                    </Link>
                  </div>
                ) : (
                  <>
                    <Button
                      onClick={handleSave}
                      disabled={updateLine.isPending}
                      className="w-full h-14 text-lg bg-violet-600 hover:bg-violet-700"
                    >
                      {updateLine.isPending ? (
                        <>
                          <LoadingSpinner className="mr-2 h-5 w-5" />
                          Saving...
                        </>
                      ) : (
                        "Save Count"
                      )}
                    </Button>
                    <Button
                      variant="outline"
                      onClick={handleSaveAndNext}
                      disabled={updateLine.isPending}
                      className="w-full h-12"
                    >
                      Save & Next Product
                    </Button>
                  </>
                )}
              </div>
            </>
          ) : (
            <div className="p-4 bg-gray-100 rounded-lg text-center">
              <p className="text-muted-foreground">
                This stocktake is {stocktake.status.toLowerCase()} and cannot be edited.
              </p>
              {line.countedQuantity !== null && (
                <div className="mt-4">
                  <p className="text-sm text-muted-foreground">Counted Quantity</p>
                  <p className="text-3xl font-bold">{line.countedQuantity}</p>
                </div>
              )}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Quick Navigation */}
      {stocktake.lines.length > 1 && (
        <div className="text-center text-sm text-muted-foreground">
          Product {stocktake.lines.findIndex((l) => l.id === lineId) + 1} of{" "}
          {stocktake.lines.length}
        </div>
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
