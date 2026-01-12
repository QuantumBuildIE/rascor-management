"use client";

import * as React from "react";
import Link from "next/link";
import { useSearchParams, useRouter, usePathname } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  DataTable,
  type Column,
  type SortDirection,
} from "@/components/shared/data-table";
import { useGoodsReceipts } from "@/lib/api/stock/use-goods-receipts";
import type { GoodsReceipt } from "@/types/stock";

function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = React.useState(value);

  React.useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(timer);
    };
  }, [value, delay]);

  return debouncedValue;
}

export default function GoodsReceiptsPage() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  // Parse URL params
  const sortColumn = searchParams.get("sortColumn") || undefined;
  const sortDirection =
    (searchParams.get("sortDirection") as SortDirection) || undefined;
  const searchParam = searchParams.get("search") || "";

  const [searchInput, setSearchInput] = React.useState(searchParam);
  const debouncedSearch = useDebounce(searchInput, 300);

  // Update URL when search changes (debounced)
  React.useEffect(() => {
    if (debouncedSearch !== searchParam) {
      updateUrlParams({ search: debouncedSearch || null });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedSearch]);

  const { data: receipts, isLoading, error } = useGoodsReceipts();

  const updateUrlParams = (
    updates: Record<string, string | number | null | undefined>
  ) => {
    const params = new URLSearchParams(searchParams.toString());

    Object.entries(updates).forEach(([key, value]) => {
      if (value === null || value === undefined || value === "") {
        params.delete(key);
      } else {
        params.set(key, String(value));
      }
    });

    const queryString = params.toString();
    router.push(queryString ? `${pathname}?${queryString}` : pathname);
  };

  const handleSort = (column: string, direction: SortDirection) => {
    updateUrlParams({ sortColumn: column, sortDirection: direction });
  };

  // Client-side filtering and sorting
  const filteredAndSortedData = React.useMemo(() => {
    let data = receipts ?? [];

    // Filter by search (GRN number, supplier name, or PO number)
    if (searchParam) {
      const searchLower = searchParam.toLowerCase();
      data = data.filter(
        (item) =>
          item.grnNumber.toLowerCase().includes(searchLower) ||
          item.supplierName.toLowerCase().includes(searchLower) ||
          (item.poNumber && item.poNumber.toLowerCase().includes(searchLower))
      );
    }

    // Sort
    if (sortColumn) {
      const sortDir = sortDirection === "desc" ? -1 : 1;
      data = [...data].sort((a, b) => {
        const aValue = a[sortColumn as keyof GoodsReceipt];
        const bValue = b[sortColumn as keyof GoodsReceipt];

        if (aValue === null || aValue === undefined) return 1;
        if (bValue === null || bValue === undefined) return -1;

        if (typeof aValue === "string" && typeof bValue === "string") {
          return aValue.localeCompare(bValue) * sortDir;
        }
        if (typeof aValue === "number" && typeof bValue === "number") {
          return (aValue - bValue) * sortDir;
        }
        return String(aValue).localeCompare(String(bValue)) * sortDir;
      });
    }

    return data;
  }, [receipts, searchParam, sortColumn, sortDirection]);

  const renderActions = (receipt: GoodsReceipt) => {
    return (
      <div className="flex items-center justify-end gap-1">
        <Button variant="ghost" size="sm" asChild>
          <Link href={`/stock/goods-receipts/${receipt.id}`}>View</Link>
        </Button>
      </div>
    );
  };

  const columns: Column<GoodsReceipt>[] = [
    {
      key: "grnNumber",
      header: "GRN Number",
      sortable: true,
      className: "font-medium",
    },
    {
      key: "supplierName",
      header: "Supplier",
      sortable: true,
    },
    {
      key: "poNumber",
      header: "PO Number",
      sortable: true,
      render: (receipt) =>
        receipt.poNumber ? (
          <Link
            href={`/stock/purchase-orders/${receipt.purchaseOrderId}`}
            className="text-primary hover:underline"
            onClick={(e) => e.stopPropagation()}
          >
            {receipt.poNumber}
          </Link>
        ) : (
          <span className="text-muted-foreground">-</span>
        ),
    },
    {
      key: "locationName",
      header: "Location",
      sortable: true,
    },
    {
      key: "receiptDate",
      header: "Receipt Date",
      sortable: true,
      render: (receipt) => new Date(receipt.receiptDate).toLocaleDateString(),
    },
    {
      key: "receivedBy",
      header: "Received By",
      sortable: true,
    },
    {
      key: "actions",
      header: "Actions",
      headerClassName: "text-right",
      className: "text-right",
      render: renderActions,
    },
  ];

  if (error) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-semibold tracking-tight">Goods Receipts</h1>
            <p className="text-muted-foreground">Record incoming goods and deliveries</p>
          </div>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load goods receipts. Please try again.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Goods Receipts</h1>
          <p className="text-muted-foreground">Record incoming goods and deliveries</p>
        </div>
        <Button asChild>
          <Link href="/stock/goods-receipts/new">Receive Goods</Link>
        </Button>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-sm">
          <SearchIcon className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search by GRN #, supplier, or PO #..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="pl-9"
          />
        </div>
      </div>

      <DataTable
        columns={columns}
        data={filteredAndSortedData}
        isLoading={isLoading}
        emptyMessage="No goods receipts found"
        keyExtractor={(receipt) => receipt.id}
        sortColumn={sortColumn}
        sortDirection={sortDirection}
        onSort={handleSort}
        onRowClick={(receipt) => router.push(`/stock/goods-receipts/${receipt.id}`)}
      />
    </div>
  );
}

function SearchIcon({ className }: { className?: string }) {
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
        d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
      />
    </svg>
  );
}
