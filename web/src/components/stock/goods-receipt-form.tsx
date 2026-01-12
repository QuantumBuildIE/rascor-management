"use client";

import * as React from "react";
import { useForm, useFieldArray } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { PlusIcon, TrashIcon } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ProductSearchSelect } from "@/components/stock/product-search-select";
import { SupplierSearchSelect } from "@/components/stock/supplier-search-select";
import { LocationSearchSelect } from "@/components/stock/location-search-select";
import { PurchaseOrderSearchSelect } from "@/components/stock/purchase-order-search-select";
import { BayLocationSearchSelect } from "@/components/stock/bay-location-search-select";
import { useCreateGoodsReceipt } from "@/lib/api/stock/use-goods-receipts";
import { useAuth } from "@/lib/auth/use-auth";
import type { GoodsReceipt, Product, Supplier, StockLocation, PurchaseOrder } from "@/types/stock";
import { toast } from "sonner";

const REJECTION_REASONS = [
  "Damaged",
  "Wrong Item",
  "Expired",
  "Quality Issue",
  "Other",
] as const;

const goodsReceiptLineSchema = z.object({
  productId: z.string().min(1, "Product is required"),
  productCode: z.string(),
  productName: z.string(),
  quantityReceived: z.number().min(1, "Quantity must be at least 1"),
  notes: z.string().optional(),
  quantityRejected: z.number().min(0).optional(),
  rejectionReason: z.string().nullable().optional(),
  batchNumber: z.string().optional(),
  expiryDate: z.string().optional(),
  bayLocationId: z.string().optional(),
});

const goodsReceiptFormSchema = z.object({
  supplierId: z.string().min(1, "Supplier is required"),
  supplierName: z.string(),
  deliveryNoteRef: z.string().optional(),
  purchaseOrderId: z.string().optional(),
  locationId: z.string().min(1, "Location is required"),
  locationName: z.string(),
  receivedBy: z.string().min(1, "Received by is required"),
  notes: z.string().optional(),
  lines: z.array(goodsReceiptLineSchema).min(1, "At least one line item is required"),
});

type GoodsReceiptFormValues = z.infer<typeof goodsReceiptFormSchema>;

interface GoodsReceiptFormProps {
  initialPurchaseOrderId?: string;
  initialPurchaseOrder?: PurchaseOrder;
  onSuccess?: (receipt: GoodsReceipt) => void;
  onCancel?: () => void;
}

export function GoodsReceiptForm({
  initialPurchaseOrderId,
  initialPurchaseOrder,
  onSuccess,
  onCancel,
}: GoodsReceiptFormProps) {
  const { user } = useAuth();
  const createGoodsReceipt = useCreateGoodsReceipt();

  const defaultReceivedBy = user ? `${user.firstName} ${user.lastName}` : "";

  const form = useForm<GoodsReceiptFormValues>({
    resolver: zodResolver(goodsReceiptFormSchema),
    defaultValues: {
      supplierId: initialPurchaseOrder?.supplierId ?? "",
      supplierName: initialPurchaseOrder?.supplierName ?? "",
      deliveryNoteRef: "",
      purchaseOrderId: initialPurchaseOrderId ?? "",
      locationId: "",
      locationName: "",
      receivedBy: defaultReceivedBy,
      notes: "",
      lines: initialPurchaseOrder?.lines
        .filter((line) => line.quantityOrdered > line.quantityReceived)
        .map((line) => ({
          productId: line.productId,
          productCode: line.productCode,
          productName: line.productName,
          quantityReceived: line.quantityOrdered - line.quantityReceived,
          notes: "",
          quantityRejected: 0,
          rejectionReason: null,
          batchNumber: "",
          expiryDate: "",
          bayLocationId: "",
        })) ?? [],
    },
  });

  const { fields, append, remove, replace } = useFieldArray({
    control: form.control,
    name: "lines",
  });

  const watchLines = form.watch("lines");
  const watchSupplierId = form.watch("supplierId");
  const watchLocationId = form.watch("locationId");

  const selectedProductIds = React.useMemo(
    () => watchLines.map((line) => line.productId).filter(Boolean),
    [watchLines]
  );

  const isSubmitting = createGoodsReceipt.isPending;

  const handleAddLine = () => {
    append({
      productId: "",
      productCode: "",
      productName: "",
      quantityReceived: 1,
      notes: "",
      quantityRejected: 0,
      rejectionReason: null,
      batchNumber: "",
      expiryDate: "",
      bayLocationId: "",
    });
  };

  const handleSupplierChange = (supplierId: string, supplier: Supplier | undefined) => {
    if (supplier) {
      form.setValue("supplierId", supplier.id);
      form.setValue("supplierName", supplier.supplierName);
      // Clear PO selection when supplier changes
      form.setValue("purchaseOrderId", "");
      // Clear lines when supplier changes (unless we have an initial PO)
      if (!initialPurchaseOrderId) {
        replace([]);
      }
    }
  };

  const handleLocationChange = (locationId: string, location: StockLocation | undefined) => {
    if (location) {
      form.setValue("locationId", location.id);
      form.setValue("locationName", location.locationName);
    }
  };

  const handlePurchaseOrderChange = (
    purchaseOrderId: string | undefined,
    purchaseOrder: PurchaseOrder | undefined
  ) => {
    form.setValue("purchaseOrderId", purchaseOrderId ?? "");

    if (purchaseOrder) {
      // Auto-populate lines from PO outstanding quantities
      const poLines = purchaseOrder.lines
        .filter((line) => line.quantityOrdered > line.quantityReceived)
        .map((line) => ({
          productId: line.productId,
          productCode: line.productCode,
          productName: line.productName,
          quantityReceived: line.quantityOrdered - line.quantityReceived,
          notes: "",
          quantityRejected: 0,
          rejectionReason: null,
          batchNumber: "",
          expiryDate: "",
          bayLocationId: "",
        }));
      replace(poLines);
    } else {
      // Clear lines when PO is cleared
      replace([]);
    }
  };

  const handleProductChange = (
    index: number,
    productId: string,
    product: Product | undefined
  ) => {
    if (product) {
      form.setValue(`lines.${index}.productId`, product.id);
      form.setValue(`lines.${index}.productCode`, product.productCode);
      form.setValue(`lines.${index}.productName`, product.productName);
    }
  };

  const handleQuantityChange = (index: number, quantity: number) => {
    form.setValue(`lines.${index}.quantityReceived`, quantity);
  };

  async function onSubmit(values: GoodsReceiptFormValues) {
    try {
      const result = await createGoodsReceipt.mutateAsync({
        supplierId: values.supplierId,
        deliveryNoteRef: values.deliveryNoteRef || undefined,
        purchaseOrderId: values.purchaseOrderId || undefined,
        locationId: values.locationId,
        receivedBy: values.receivedBy,
        notes: values.notes,
        lines: values.lines.map((line) => ({
          productId: line.productId,
          quantityReceived: line.quantityReceived,
          notes: line.notes,
          quantityRejected: line.quantityRejected || 0,
          rejectionReason: line.rejectionReason || undefined,
          batchNumber: line.batchNumber || undefined,
          expiryDate: line.expiryDate || undefined,
          bayLocationId: line.bayLocationId || undefined,
        })),
      });
      toast.success("Goods receipt created successfully");
      onSuccess?.(result);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to create goods receipt", {
        description: message,
      });
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Receipt Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <FormField
                control={form.control}
                name="supplierId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Supplier *</FormLabel>
                    <FormControl>
                      <SupplierSearchSelect
                        value={field.value}
                        onValueChange={handleSupplierChange}
                        disabled={!!initialPurchaseOrderId}
                        placeholder="Select a supplier..."
                      />
                    </FormControl>
                    <FormDescription>
                      The supplier providing the goods
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="deliveryNoteRef"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Delivery Note Ref</FormLabel>
                    <FormControl>
                      <Input placeholder="Supplier's delivery note number" {...field} />
                    </FormControl>
                    <FormDescription>
                      Reference from supplier&apos;s delivery note
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <FormField
                control={form.control}
                name="purchaseOrderId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Purchase Order</FormLabel>
                    <FormControl>
                      <PurchaseOrderSearchSelect
                        value={field.value}
                        supplierId={watchSupplierId}
                        onValueChange={handlePurchaseOrderChange}
                        disabled={!!initialPurchaseOrderId}
                        placeholder="Link to a PO (optional)..."
                      />
                    </FormControl>
                    <FormDescription>
                      Optionally link to an existing purchase order
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="locationId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Location *</FormLabel>
                    <FormControl>
                      <LocationSearchSelect
                        value={field.value}
                        onValueChange={handleLocationChange}
                        placeholder="Select receiving location..."
                      />
                    </FormControl>
                    <FormDescription>
                      Where the goods will be received
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="receivedBy"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Received By *</FormLabel>
                    <FormControl>
                      <Input placeholder="Name of person receiving goods" {...field} />
                    </FormControl>
                    <FormDescription>
                      Person receiving the goods
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <FormField
              control={form.control}
              name="notes"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Notes</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Any additional notes about this delivery..."
                      className="min-h-[100px]"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle>Line Items</CardTitle>
            <Button type="button" variant="outline" size="sm" onClick={handleAddLine}>
              <PlusIcon className="mr-2 h-4 w-4" />
              Add Line
            </Button>
          </CardHeader>
          <CardContent>
            {fields.length === 0 ? (
              <div className="rounded-lg border border-dashed p-8 text-center">
                <p className="text-muted-foreground">No line items added yet.</p>
                <p className="text-sm text-muted-foreground mt-1">
                  {watchSupplierId
                    ? "Add items manually or select a Purchase Order to auto-populate."
                    : "Select a supplier first, then add items or link to a Purchase Order."}
                </p>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="mt-4"
                  onClick={handleAddLine}
                  disabled={!watchSupplierId}
                >
                  <PlusIcon className="mr-2 h-4 w-4" />
                  Add First Line
                </Button>
              </div>
            ) : (
              <div className="rounded-md border">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead className="w-[250px]">Product</TableHead>
                      <TableHead className="w-[100px] text-right">Qty Received</TableHead>
                      <TableHead className="w-[180px]">Bay Location</TableHead>
                      <TableHead className="w-[120px]">Notes</TableHead>
                      <TableHead className="w-[50px]"></TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {fields.map((field, index) => (
                      <TableRow key={field.id}>
                        <TableCell>
                          <FormField
                            control={form.control}
                            name={`lines.${index}.productId`}
                            render={({ field: productField }) => (
                              <FormItem className="space-y-0">
                                <FormControl>
                                  <ProductSearchSelect
                                    value={productField.value}
                                    onValueChange={(productId, product) =>
                                      handleProductChange(index, productId, product)
                                    }
                                    excludeProductIds={selectedProductIds.filter(
                                      (id) => id !== productField.value
                                    )}
                                    placeholder="Select product..."
                                  />
                                </FormControl>
                                <FormMessage />
                              </FormItem>
                            )}
                          />
                        </TableCell>
                        <TableCell className="text-right">
                          <FormField
                            control={form.control}
                            name={`lines.${index}.quantityReceived`}
                            render={({ field: qtyField }) => (
                              <FormItem className="space-y-0">
                                <FormControl>
                                  <Input
                                    type="number"
                                    min="1"
                                    className="w-full text-right"
                                    {...qtyField}
                                    value={qtyField.value ?? ""}
                                    onChange={(e) => {
                                      const val = e.target.valueAsNumber || 1;
                                      handleQuantityChange(index, val);
                                    }}
                                  />
                                </FormControl>
                                <FormMessage />
                              </FormItem>
                            )}
                          />
                        </TableCell>
                        <TableCell>
                          <FormField
                            control={form.control}
                            name={`lines.${index}.bayLocationId`}
                            render={({ field: bayField }) => (
                              <FormItem className="space-y-0">
                                <FormControl>
                                  <BayLocationSearchSelect
                                    value={bayField.value}
                                    stockLocationId={watchLocationId}
                                    onValueChange={(bayId) => bayField.onChange(bayId ?? "")}
                                    placeholder="Select bay..."
                                  />
                                </FormControl>
                              </FormItem>
                            )}
                          />
                        </TableCell>
                        <TableCell>
                          <FormField
                            control={form.control}
                            name={`lines.${index}.notes`}
                            render={({ field: notesField }) => (
                              <FormItem className="space-y-0">
                                <FormControl>
                                  <Input
                                    placeholder="Notes..."
                                    className="w-full"
                                    {...notesField}
                                  />
                                </FormControl>
                              </FormItem>
                            )}
                          />
                        </TableCell>
                        <TableCell>
                          <Button
                            type="button"
                            variant="ghost"
                            size="icon"
                            onClick={() => remove(index)}
                            className="h-8 w-8 text-destructive hover:text-destructive"
                          >
                            <TrashIcon className="h-4 w-4" />
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            )}
            {form.formState.errors.lines?.root && (
              <p className="mt-2 text-sm text-destructive">
                {form.formState.errors.lines.root.message}
              </p>
            )}
          </CardContent>
        </Card>

        <div className="flex justify-end gap-4">
          {onCancel && (
            <Button type="button" variant="outline" onClick={onCancel}>
              Cancel
            </Button>
          )}
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? (
              <>
                <LoadingSpinner className="mr-2 h-4 w-4" />
                Creating...
              </>
            ) : (
              "Create Receipt"
            )}
          </Button>
        </div>
      </form>
    </Form>
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
