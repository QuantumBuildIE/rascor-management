"use client";

import * as React from "react";
import { useSearchParams, useRouter, usePathname } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import { useStockValuation } from "@/lib/api/stock/use-stock-reports";
import { useLocations } from "@/lib/api/stock/use-locations";
import { useCategories } from "@/lib/api/stock/use-categories";
import { usePermission } from "@/lib/auth/use-auth";
import type { StockValuationItem } from "@/types/stock";

type SortColumn = keyof StockValuationItem;
type SortDirection = "asc" | "desc";

export default function StockValuationPage() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const hasViewCostingsPermission = usePermission("StockManagement.ViewCostings");

  // Parse URL params
  const locationId = searchParams.get("locationId") || undefined;
  const categoryId = searchParams.get("categoryId") || undefined;
  const sortColumn = (searchParams.get("sortColumn") as SortColumn) || "locationName";
  const sortDirection = (searchParams.get("sortDirection") as SortDirection) || "asc";

  // Fetch data
  const { data: report, isLoading, error } = useStockValuation({ locationId, categoryId });
  const { data: locations } = useLocations();
  const { data: categories } = useCategories();

  const updateUrlParams = (
    updates: Record<string, string | null | undefined>
  ) => {
    const params = new URLSearchParams(searchParams.toString());

    Object.entries(updates).forEach(([key, value]) => {
      if (value === null || value === undefined || value === "" || value === "all") {
        params.delete(key);
      } else {
        params.set(key, String(value));
      }
    });

    const queryString = params.toString();
    router.push(queryString ? `${pathname}?${queryString}` : pathname);
  };

  const handleLocationChange = (value: string) => {
    updateUrlParams({ locationId: value === "all" ? null : value });
  };

  const handleCategoryChange = (value: string) => {
    updateUrlParams({ categoryId: value === "all" ? null : value });
  };

  const handleSort = (column: SortColumn) => {
    const newDirection = sortColumn === column && sortDirection === "asc" ? "desc" : "asc";
    updateUrlParams({ sortColumn: column, sortDirection: newDirection });
  };

  const handlePrint = () => {
    window.print();
  };

  // Sort items client-side
  const sortedItems = React.useMemo(() => {
    if (!report?.items) return [];

    return [...report.items].sort((a, b) => {
      const aValue = a[sortColumn];
      const bValue = b[sortColumn];
      const direction = sortDirection === "asc" ? 1 : -1;

      if (aValue === null || aValue === undefined) return 1;
      if (bValue === null || bValue === undefined) return -1;

      if (typeof aValue === "number" && typeof bValue === "number") {
        return (aValue - bValue) * direction;
      }

      return String(aValue).localeCompare(String(bValue)) * direction;
    });
  }, [report?.items, sortColumn, sortDirection]);

  const formatCurrency = (value: number | null) => {
    if (value === null || value === undefined) return "-";
    return new Intl.NumberFormat("en-IE", {
      style: "currency",
      currency: "EUR",
    }).format(value);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString("en-IE", {
      dateStyle: "medium",
      timeStyle: "short",
    });
  };

  // Check permission
  if (!hasViewCostingsPermission) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Stock Valuation Report</h1>
          <p className="text-muted-foreground">Current stock value by product and location</p>
        </div>
        <Card>
          <CardContent className="py-12 text-center">
            <p className="text-muted-foreground">
              You do not have permission to view stock costings.
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Stock Valuation Report</h1>
          <p className="text-muted-foreground">Current stock value by product and location</p>
        </div>
        <Card>
          <CardContent className="py-12 text-center">
            <p className="text-destructive">Failed to load report. Please try again.</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header - hidden when printing */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between print:hidden">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Stock Valuation Report</h1>
          <p className="text-muted-foreground">Current stock value by product and location</p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={handlePrint} className="w-full sm:w-auto">
            <PrinterIcon className="mr-2 h-4 w-4" />
            Print
          </Button>
        </div>
      </div>

      {/* Print Header - only visible when printing */}
      <div className="hidden print:block print:mb-6">
        <h1 className="text-2xl font-bold">Stock Valuation Report</h1>
        <p className="text-sm text-muted-foreground">
          Generated: {report?.generatedAt ? formatDate(report.generatedAt) : "-"}
        </p>
        {locationId && locations && (
          <p className="text-sm">
            Location: {locations.find((l) => l.id === locationId)?.locationName || "All"}
          </p>
        )}
        {categoryId && categories && (
          <p className="text-sm">
            Category: {categories.find((c) => c.id === categoryId)?.categoryName || "All"}
          </p>
        )}
      </div>

      {/* Filters - hidden when printing */}
      <div className="flex flex-col gap-4 sm:flex-row sm:flex-wrap sm:items-center print:hidden">
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
          <span className="text-sm text-muted-foreground">Location:</span>
          <Select value={locationId || "all"} onValueChange={handleLocationChange}>
            <SelectTrigger className="w-full sm:w-[200px]">
              <SelectValue placeholder="All locations" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All locations</SelectItem>
              {locations?.map((location) => (
                <SelectItem key={location.id} value={location.id}>
                  {location.locationName}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
          <span className="text-sm text-muted-foreground">Category:</span>
          <Select value={categoryId || "all"} onValueChange={handleCategoryChange}>
            <SelectTrigger className="w-full sm:w-[200px]">
              <SelectValue placeholder="All categories" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All categories</SelectItem>
              {categories?.map((category) => (
                <SelectItem key={category.id} value={category.id}>
                  {category.categoryName}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        {report?.generatedAt && (
          <div className="w-full text-sm text-muted-foreground sm:ml-auto sm:w-auto">
            Generated: {formatDate(report.generatedAt)}
          </div>
        )}
      </div>

      {/* Summary Cards */}
      <div className="grid gap-4 md:grid-cols-3 print:grid-cols-3">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Products
            </CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-20" />
            ) : (
              <p className="text-2xl font-bold">{report?.totalProducts ?? 0}</p>
            )}
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Quantity
            </CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-24" />
            ) : (
              <p className="text-2xl font-bold">
                {report?.totalQuantity?.toLocaleString() ?? 0}
              </p>
            )}
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Value
            </CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-32" />
            ) : (
              <p className="text-2xl font-bold text-green-600">
                {formatCurrency(report?.totalValue ?? 0)}
              </p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Data Table */}
      <Card>
        <CardHeader className="print:py-2">
          <CardTitle>Stock Items</CardTitle>
        </CardHeader>
        <CardContent className="print:p-0">
          {isLoading ? (
            <div className="space-y-2">
              {[...Array(5)].map((_, i) => (
                <Skeleton key={i} className="h-12 w-full" />
              ))}
            </div>
          ) : sortedItems.length === 0 ? (
            <div className="py-8 text-center text-muted-foreground">
              No stock items found with current filters.
            </div>
          ) : (
            <div className="overflow-x-auto rounded-md border print:border-0">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead
                      className="cursor-pointer hover:bg-muted/50 print:cursor-default"
                      onClick={() => handleSort("productCode")}
                    >
                      Product Code
                      {sortColumn === "productCode" && (
                        <SortIndicator direction={sortDirection} />
                      )}
                    </TableHead>
                    <TableHead
                      className="cursor-pointer hover:bg-muted/50 print:cursor-default"
                      onClick={() => handleSort("productName")}
                    >
                      Product Name
                      {sortColumn === "productName" && (
                        <SortIndicator direction={sortDirection} />
                      )}
                    </TableHead>
                    <TableHead
                      className="cursor-pointer hover:bg-muted/50 print:cursor-default"
                      onClick={() => handleSort("categoryName")}
                    >
                      Category
                      {sortColumn === "categoryName" && (
                        <SortIndicator direction={sortDirection} />
                      )}
                    </TableHead>
                    <TableHead
                      className="cursor-pointer hover:bg-muted/50 print:cursor-default"
                      onClick={() => handleSort("locationName")}
                    >
                      Location
                      {sortColumn === "locationName" && (
                        <SortIndicator direction={sortDirection} />
                      )}
                    </TableHead>
                    <TableHead
                      className="cursor-pointer hover:bg-muted/50 print:cursor-default"
                      onClick={() => handleSort("bayCode")}
                    >
                      Bay
                      {sortColumn === "bayCode" && (
                        <SortIndicator direction={sortDirection} />
                      )}
                    </TableHead>
                    <TableHead
                      className="cursor-pointer hover:bg-muted/50 text-right print:cursor-default"
                      onClick={() => handleSort("quantityOnHand")}
                    >
                      Qty On Hand
                      {sortColumn === "quantityOnHand" && (
                        <SortIndicator direction={sortDirection} />
                      )}
                    </TableHead>
                    <TableHead
                      className="cursor-pointer hover:bg-muted/50 text-right print:cursor-default"
                      onClick={() => handleSort("costPrice")}
                    >
                      Cost Price
                      {sortColumn === "costPrice" && (
                        <SortIndicator direction={sortDirection} />
                      )}
                    </TableHead>
                    <TableHead
                      className="cursor-pointer hover:bg-muted/50 text-right print:cursor-default"
                      onClick={() => handleSort("totalValue")}
                    >
                      Total Value
                      {sortColumn === "totalValue" && (
                        <SortIndicator direction={sortDirection} />
                      )}
                    </TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {sortedItems.map((item) => (
                    <TableRow 
                      key={`${item.productId}-${item.locationId}`}
                      className={item.costPrice === null ? "bg-yellow-50" : ""}
                    >
                      <TableCell className="font-medium">{item.productCode}</TableCell>
                      <TableCell>{item.productName}</TableCell>
                      <TableCell>{item.categoryName}</TableCell>
                      <TableCell>{item.locationName}</TableCell>
                      <TableCell>{item.bayCode || "-"}</TableCell>
                      <TableCell className="text-right">{item.quantityOnHand}</TableCell>
                      <TableCell className="text-right">
                        {formatCurrency(item.costPrice)}
                      </TableCell>
                      <TableCell className="text-right font-medium">
                        {formatCurrency(item.totalValue)}
                      </TableCell>
                    </TableRow>
                  ))}
                  {/* Totals row */}
                  <TableRow className="bg-muted/50 font-medium">
                    <TableCell colSpan={5} className="text-right">
                      Totals:
                    </TableCell>
                    <TableCell className="text-right">
                      {report?.totalQuantity?.toLocaleString()}
                    </TableCell>
                    <TableCell className="text-right">-</TableCell>
                    <TableCell className="text-right text-green-600">
                      {formatCurrency(report?.totalValue ?? 0)}
                    </TableCell>
                  </TableRow>
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Print styles */}
      <style jsx global>{`
        @media print {
          body * {
            visibility: hidden;
          }
          .space-y-6,
          .space-y-6 * {
            visibility: visible;
          }
          .space-y-6 {
            position: absolute;
            left: 0;
            top: 0;
            width: 100%;
          }
          .print\\:hidden {
            display: none !important;
          }
          .print\\:block {
            display: block !important;
          }
          nav,
          header,
          [role="navigation"] {
            display: none !important;
          }
        }
      `}</style>
    </div>
  );
}

function SortIndicator({ direction }: { direction: SortDirection }) {
  return (
    <span className="ml-1 inline-block print:hidden">
      {direction === "asc" ? "▲" : "▼"}
    </span>
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
