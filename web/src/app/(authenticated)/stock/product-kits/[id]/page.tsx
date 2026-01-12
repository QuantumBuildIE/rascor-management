"use client";

import * as React from "react";
import Link from "next/link";
import { useRouter, useParams } from "next/navigation";
import { Button } from "@/components/ui/button";
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
import { DeleteConfirmationDialog } from "@/components/shared/delete-confirmation-dialog";
import { KitItemDialog } from "@/components/stock/kit-item-dialog";
import {
  useProductKit,
  useDeleteProductKit,
  useDeleteProductKitItem,
  type ProductKitItem,
} from "@/lib/api/stock/use-product-kits";
import { toast } from "sonner";

function formatCurrency(value: number): string {
  return new Intl.NumberFormat("en-IE", {
    style: "currency",
    currency: "EUR",
  }).format(value);
}

export default function ProductKitDetailPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;

  const { data: kit, isLoading, error } = useProductKit(id);
  const deleteProductKit = useDeleteProductKit();
  const deleteKitItem = useDeleteProductKitItem();

  // Dialog states
  const [deleteKitDialogOpen, setDeleteKitDialogOpen] = React.useState(false);
  const [itemDialogOpen, setItemDialogOpen] = React.useState(false);
  const [editingItem, setEditingItem] = React.useState<ProductKitItem | undefined>();
  const [deleteItemDialogOpen, setDeleteItemDialogOpen] = React.useState(false);
  const [itemToDelete, setItemToDelete] = React.useState<ProductKitItem | null>(null);

  const handleDeleteKit = async () => {
    try {
      await deleteProductKit.mutateAsync(id);
      toast.success("Product kit deleted successfully");
      router.push("/stock/product-kits");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to delete product kit", { description: message });
    }
  };

  const handleAddItem = () => {
    setEditingItem(undefined);
    setItemDialogOpen(true);
  };

  const handleEditItem = (item: ProductKitItem) => {
    setEditingItem(item);
    setItemDialogOpen(true);
  };

  const handleDeleteItemClick = (item: ProductKitItem) => {
    setItemToDelete(item);
    setDeleteItemDialogOpen(true);
  };

  const handleDeleteItemConfirm = async () => {
    if (!itemToDelete) return;

    try {
      await deleteKitItem.mutateAsync({
        itemId: itemToDelete.id,
        kitId: id,
      });
      toast.success("Item removed from kit");
      setDeleteItemDialogOpen(false);
      setItemToDelete(null);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to remove item", { description: message });
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href="/stock/product-kits">
              <ChevronLeftIcon className="mr-1 h-4 w-4" />
              Back to Product Kits
            </Link>
          </Button>
        </div>
        <div className="flex items-center justify-center py-12">
          <LoadingSpinner className="h-8 w-8" />
        </div>
      </div>
    );
  }

  if (error || !kit) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href="/stock/product-kits">
              <ChevronLeftIcon className="mr-1 h-4 w-4" />
              Back to Product Kits
            </Link>
          </Button>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load product kit. Please try again.
          </p>
        </div>
      </div>
    );
  }

  const margin = kit.totalPrice - kit.totalCost;
  const marginPercent = kit.totalCost > 0 ? (margin / kit.totalCost) * 100 : 0;

  const existingProductIds = kit.items.map((item) => item.productId);
  const nextSortOrder = kit.items.length > 0
    ? Math.max(...kit.items.map((item) => item.sortOrder)) + 1
    : 0;

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" asChild>
          <Link href="/stock/product-kits">
            <ChevronLeftIcon className="mr-1 h-4 w-4" />
            Back to Product Kits
          </Link>
        </Button>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-semibold tracking-tight">
              {kit.kitCode}
            </h1>
            {kit.categoryName && (
              <Badge variant="secondary">{kit.categoryName}</Badge>
            )}
            <Badge variant={kit.isActive ? "default" : "secondary"}>
              {kit.isActive ? "Active" : "Inactive"}
            </Badge>
          </div>
          <p className="text-lg text-muted-foreground">{kit.kitName}</p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" asChild>
            <Link href={`/stock/product-kits/${id}/edit`}>Edit Kit</Link>
          </Button>
          <Button
            variant="destructive"
            onClick={() => setDeleteKitDialogOpen(true)}
          >
            Delete Kit
          </Button>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Kit Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {kit.description && (
              <div>
                <p className="text-sm text-muted-foreground">Description</p>
                <p className="text-sm mt-1">{kit.description}</p>
              </div>
            )}
            {kit.notes && (
              <div>
                <p className="text-sm text-muted-foreground">Notes</p>
                <p className="text-sm mt-1">{kit.notes}</p>
              </div>
            )}
            {!kit.description && !kit.notes && (
              <p className="text-sm text-muted-foreground">
                No description or notes provided.
              </p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Pricing Summary</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <p className="text-muted-foreground">Total Cost</p>
                <p className="text-lg font-semibold">
                  {formatCurrency(kit.totalCost)}
                </p>
              </div>
              <div>
                <p className="text-muted-foreground">Total Price</p>
                <p className="text-lg font-semibold">
                  {formatCurrency(kit.totalPrice)}
                </p>
              </div>
              <div>
                <p className="text-muted-foreground">Margin</p>
                <p className={`text-lg font-semibold ${margin >= 0 ? "text-green-600" : "text-red-600"}`}>
                  {formatCurrency(margin)} ({marginPercent.toFixed(1)}%)
                </p>
              </div>
              <div>
                <p className="text-muted-foreground">Items Count</p>
                <p className="text-lg font-semibold">{kit.itemCount}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle>Kit Items</CardTitle>
          <Button onClick={handleAddItem}>Add Item</Button>
        </CardHeader>
        <CardContent>
          {kit.items.length === 0 ? (
            <div className="text-center py-8">
              <p className="text-muted-foreground">
                No items in this kit yet. Click "Add Item" to add products.
              </p>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Product Code</TableHead>
                  <TableHead>Product Name</TableHead>
                  <TableHead className="text-right">Default Qty</TableHead>
                  <TableHead className="hidden sm:table-cell">Unit</TableHead>
                  <TableHead className="text-right hidden md:table-cell">Unit Cost</TableHead>
                  <TableHead className="text-right hidden md:table-cell">Unit Price</TableHead>
                  <TableHead className="text-right hidden lg:table-cell">Line Cost</TableHead>
                  <TableHead className="text-right">Line Price</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {kit.items
                  .sort((a, b) => a.sortOrder - b.sortOrder)
                  .map((item) => (
                    <TableRow key={item.id}>
                      <TableCell className="font-medium">
                        {item.productCode}
                      </TableCell>
                      <TableCell>
                        <div>
                          {item.productName}
                          {item.notes && (
                            <p className="text-xs text-muted-foreground">
                              {item.notes}
                            </p>
                          )}
                        </div>
                      </TableCell>
                      <TableCell className="text-right">
                        {item.defaultQuantity}
                      </TableCell>
                      <TableCell className="hidden sm:table-cell">
                        {item.unit}
                      </TableCell>
                      <TableCell className="text-right hidden md:table-cell">
                        {formatCurrency(item.unitCost)}
                      </TableCell>
                      <TableCell className="text-right hidden md:table-cell">
                        {formatCurrency(item.unitPrice)}
                      </TableCell>
                      <TableCell className="text-right hidden lg:table-cell">
                        {formatCurrency(item.lineCost)}
                      </TableCell>
                      <TableCell className="text-right font-medium">
                        {formatCurrency(item.linePrice)}
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex items-center justify-end gap-2">
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleEditItem(item)}
                          >
                            Edit
                          </Button>
                          <Button
                            variant="ghost"
                            size="sm"
                            className="text-destructive hover:text-destructive"
                            onClick={() => handleDeleteItemClick(item)}
                          >
                            Remove
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                <TableRow className="bg-muted/50">
                  <TableCell
                    colSpan={6}
                    className="text-right font-medium hidden lg:table-cell"
                  >
                    Totals
                  </TableCell>
                  <TableCell
                    colSpan={4}
                    className="text-right font-medium lg:hidden"
                  >
                    Total
                  </TableCell>
                  <TableCell className="text-right font-bold hidden lg:table-cell">
                    {formatCurrency(kit.totalCost)}
                  </TableCell>
                  <TableCell className="text-right font-bold">
                    {formatCurrency(kit.totalPrice)}
                  </TableCell>
                  <TableCell />
                </TableRow>
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <DeleteConfirmationDialog
        open={deleteKitDialogOpen}
        onOpenChange={setDeleteKitDialogOpen}
        title="Delete Product Kit"
        description={`Are you sure you want to delete "${kit.kitName}"? This action cannot be undone.`}
        onConfirm={handleDeleteKit}
        isLoading={deleteProductKit.isPending}
      />

      <DeleteConfirmationDialog
        open={deleteItemDialogOpen}
        onOpenChange={setDeleteItemDialogOpen}
        title="Remove Item from Kit"
        description={`Are you sure you want to remove "${itemToDelete?.productName}" from this kit?`}
        onConfirm={handleDeleteItemConfirm}
        isLoading={deleteKitItem.isPending}
      />

      <KitItemDialog
        open={itemDialogOpen}
        onOpenChange={setItemDialogOpen}
        kitId={id}
        item={editingItem}
        existingProductIds={existingProductIds}
        nextSortOrder={nextSortOrder}
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
