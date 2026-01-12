"use client";

import * as React from "react";
import Link from "next/link";
import { useSearchParams, useRouter, usePathname } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  DataTable,
  type Column,
  type SortDirection,
} from "@/components/shared/data-table";
import { DeleteConfirmationDialog } from "@/components/shared/delete-confirmation-dialog";
import {
  useProductKits,
  useDeleteProductKit,
  type ProductKitListItem,
} from "@/lib/api/stock/use-product-kits";
import { useCategories } from "@/lib/api/stock/use-categories";
import { toast } from "sonner";

function formatCurrency(value: number): string {
  return new Intl.NumberFormat("en-IE", {
    style: "currency",
    currency: "EUR",
  }).format(value);
}

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

export default function ProductKitsPage() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  // Parse URL params
  const pageNumber = Number(searchParams.get("page")) || 1;
  const pageSize = Number(searchParams.get("size")) || 20;
  const sortColumn = searchParams.get("sortColumn") || undefined;
  const sortDirection = (searchParams.get("sortDirection") as SortDirection) || undefined;
  const searchParam = searchParams.get("search") || "";
  const categoryParam = searchParams.get("categoryId") || "";
  const activeParam = searchParams.get("isActive") || "";

  const [searchInput, setSearchInput] = React.useState(searchParam);
  const debouncedSearch = useDebounce(searchInput, 300);

  // Delete dialog state
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false);
  const [kitToDelete, setKitToDelete] = React.useState<ProductKitListItem | null>(null);

  // Update URL when search changes (debounced)
  React.useEffect(() => {
    if (debouncedSearch !== searchParam) {
      updateUrlParams({ search: debouncedSearch || null, page: 1 });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedSearch]);

  const { data: categories } = useCategories();

  const { data, isLoading, error } = useProductKits({
    pageNumber,
    pageSize,
    sortColumn,
    sortDirection,
    search: searchParam || undefined,
    categoryId: categoryParam || undefined,
    isActive: activeParam === "" ? undefined : activeParam === "true",
  });

  const deleteProductKit = useDeleteProductKit();

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

    // Remove page param if it's 1
    if (params.get("page") === "1") {
      params.delete("page");
    }

    const queryString = params.toString();
    router.push(queryString ? `${pathname}?${queryString}` : pathname);
  };

  const handlePageChange = (page: number) => {
    updateUrlParams({ page });
  };

  const handlePageSizeChange = (size: number) => {
    updateUrlParams({ size, page: 1 });
  };

  const handleSort = (column: string, direction: SortDirection) => {
    updateUrlParams({ sortColumn: column, sortDirection: direction, page: 1 });
  };

  const handleCategoryChange = (value: string) => {
    updateUrlParams({ categoryId: value === "all" ? null : value, page: 1 });
  };

  const handleActiveChange = (value: string) => {
    updateUrlParams({ isActive: value === "all" ? null : value, page: 1 });
  };

  const handleDeleteClick = (kit: ProductKitListItem) => {
    setKitToDelete(kit);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!kitToDelete) return;

    try {
      await deleteProductKit.mutateAsync(kitToDelete.id);
      toast.success("Product kit deleted successfully");
      setDeleteDialogOpen(false);
      setKitToDelete(null);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to delete product kit", {
        description: message,
      });
    }
  };

  const handleRowClick = (kit: ProductKitListItem) => {
    router.push(`/stock/product-kits/${kit.id}`);
  };

  const columns: Column<ProductKitListItem>[] = [
    {
      key: "kitCode",
      header: "Kit Code",
      sortable: true,
      className: "font-medium",
    },
    {
      key: "kitName",
      header: "Kit Name",
      sortable: true,
    },
    {
      key: "categoryName",
      header: "Category",
      sortable: true,
      className: "hidden md:table-cell",
      headerClassName: "hidden md:table-cell",
      render: (kit) =>
        kit.categoryName ? (
          <Badge variant="secondary">{kit.categoryName}</Badge>
        ) : (
          <span className="text-muted-foreground">-</span>
        ),
    },
    {
      key: "itemCount",
      header: "Items",
      sortable: true,
      className: "text-right",
      headerClassName: "text-right",
    },
    {
      key: "totalCost",
      header: "Total Cost",
      sortable: true,
      className: "text-right hidden lg:table-cell",
      headerClassName: "text-right hidden lg:table-cell",
      render: (kit) => formatCurrency(kit.totalCost),
    },
    {
      key: "totalPrice",
      header: "Total Price",
      sortable: true,
      className: "text-right",
      headerClassName: "text-right",
      render: (kit) => (
        <span className="font-medium">{formatCurrency(kit.totalPrice)}</span>
      ),
    },
    {
      key: "isActive",
      header: "Status",
      sortable: true,
      className: "hidden sm:table-cell",
      headerClassName: "hidden sm:table-cell",
      render: (kit) =>
        kit.isActive ? (
          <Badge variant="default">Active</Badge>
        ) : (
          <Badge variant="secondary">Inactive</Badge>
        ),
    },
    {
      key: "actions",
      header: "Actions",
      headerClassName: "text-right",
      className: "text-right",
      render: (kit) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={(e) => {
              e.stopPropagation();
              router.push(`/stock/product-kits/${kit.id}`);
            }}
          >
            View
          </Button>
          <Button
            variant="ghost"
            size="sm"
            asChild
            onClick={(e) => e.stopPropagation()}
          >
            <Link href={`/stock/product-kits/${kit.id}/edit`}>Edit</Link>
          </Button>
          <Button
            variant="ghost"
            size="sm"
            className="text-destructive hover:text-destructive hidden sm:inline-flex"
            onClick={(e) => {
              e.stopPropagation();
              handleDeleteClick(kit);
            }}
          >
            Delete
          </Button>
        </div>
      ),
    },
  ];

  if (error) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-semibold tracking-tight">Product Kits</h1>
            <p className="text-muted-foreground">
              Manage pre-configured product bundles
            </p>
          </div>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load product kits. Please try again.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Product Kits</h1>
          <p className="text-muted-foreground">
            Manage pre-configured product bundles
          </p>
        </div>
        <Button asChild className="w-full sm:w-auto">
          <Link href="/stock/product-kits/new">Add Kit</Link>
        </Button>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 sm:max-w-sm">
          <SearchIcon className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search by code or name..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="pl-9"
          />
        </div>
        <Select value={categoryParam || "all"} onValueChange={handleCategoryChange}>
          <SelectTrigger className="w-full sm:w-[180px]">
            <SelectValue placeholder="All Categories" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Categories</SelectItem>
            {categories?.map((category) => (
              <SelectItem key={category.id} value={category.id}>
                {category.categoryName}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Select value={activeParam || "all"} onValueChange={handleActiveChange}>
          <SelectTrigger className="w-full sm:w-[140px]">
            <SelectValue placeholder="All Status" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Status</SelectItem>
            <SelectItem value="true">Active</SelectItem>
            <SelectItem value="false">Inactive</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <DataTable
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        emptyMessage="No product kits found"
        keyExtractor={(kit) => kit.id}
        skeletonRows={pageSize}
        onRowClick={handleRowClick}
        pagination={
          data
            ? {
                pageNumber: data.pageNumber,
                pageSize: data.pageSize,
                totalCount: data.totalCount,
                totalPages: data.totalPages,
              }
            : undefined
        }
        onPageChange={handlePageChange}
        onPageSizeChange={handlePageSizeChange}
        sortColumn={sortColumn}
        sortDirection={sortDirection}
        onSort={handleSort}
      />

      <DeleteConfirmationDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        title="Delete Product Kit"
        description={`Are you sure you want to delete "${kitToDelete?.kitName}"? This action cannot be undone.`}
        onConfirm={handleDeleteConfirm}
        isLoading={deleteProductKit.isPending}
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
