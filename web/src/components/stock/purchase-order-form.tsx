"use client";

import * as React from "react";
import { useForm, useFieldArray, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { format } from "date-fns";
import { CalendarIcon, PlusIcon, TrashIcon } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Calendar } from "@/components/ui/calendar";
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
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
  TableFooter,
} from "@/components/ui/table";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ProductSearchSelect } from "@/components/stock/product-search-select";
import { SupplierSearchSelect } from "@/components/stock/supplier-search-select";
import { useCreatePurchaseOrder, useUpdatePurchaseOrder } from "@/lib/api/stock/use-purchase-orders";
import type { PurchaseOrder, Product, Supplier } from "@/types/stock";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

const purchaseOrderLineSchema = z.object({
  id: z.string().optional(),
  productId: z.string().min(1, "Product is required"),
  productCode: z.string(),
  productName: z.string(),
  quantityOrdered: z.number().min(1, "Quantity must be at least 1"),
  unitPrice: z.number().min(0, "Unit price must be positive"),
  lineTotal: z.number(),
});

const purchaseOrderFormSchema = z.object({
  supplierId: z.string().min(1, "Supplier is required"),
  supplierName: z.string(),
  expectedDate: z.date().optional().nullable(),
  notes: z.string().optional(),
  lines: z.array(purchaseOrderLineSchema).min(1, "At least one line item is required"),
});

type PurchaseOrderFormValues = z.infer<typeof purchaseOrderFormSchema>;

interface PurchaseOrderFormProps {
  order?: PurchaseOrder;
  onSuccess?: (order: PurchaseOrder) => void;
  onCancel?: () => void;
}

export function PurchaseOrderForm({ order, onSuccess, onCancel }: PurchaseOrderFormProps) {
  const isEditing = !!order;

  const createPurchaseOrder = useCreatePurchaseOrder();
  const updatePurchaseOrder = useUpdatePurchaseOrder();

  const form = useForm<PurchaseOrderFormValues>({
    resolver: zodResolver(purchaseOrderFormSchema) as any,
    defaultValues: {
      supplierId: order?.supplierId ?? "",
      supplierName: order?.supplierName ?? "",
      expectedDate: order?.expectedDate ? new Date(order.expectedDate) : null,
      notes: order?.notes ?? "",
      lines: order?.lines.map((line) => ({
        id: line.id,
        productId: line.productId,
        productCode: line.productCode,
        productName: line.productName,
        quantityOrdered: line.quantityOrdered,
        unitPrice: line.unitPrice,
        lineTotal: line.lineTotal,
      })) ?? [],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control: form.control,
    name: "lines",
  });

/*   
  const watchLines = form.watch("lines");
  const orderTotal = React.useMemo(
    () => watchLines.reduce((sum, line) => sum + (line.lineTotal || 0), 0),
    [watchLines]

  ); 
  const selectedProductIds = React.useMemo(
    () => watchLines.map((line) => line.productId).filter(Boolean),
    [watchLines]
  );
*/

  const watchLines = useWatch({
    control: form.control,
    name: "lines",
  }) ?? [];

  const orderTotal = React.useMemo(
    () => watchLines.reduce((sum, line) => sum + (line?.lineTotal || 0), 0),
    [watchLines]
  );

  const selectedProductIds = React.useMemo(
    () => watchLines.map((line) => line?.productId).filter(Boolean),
    [watchLines]
  );

  const isSubmitting = createPurchaseOrder.isPending || updatePurchaseOrder.isPending;

  const handleAddLine = () => {
    append({
      productId: "",
      productCode: "",
      productName: "",
      quantityOrdered: 1,
      unitPrice: 0,
      lineTotal: 0,
    });
  };

  const handleSupplierChange = (supplierId: string, supplier: Supplier | undefined) => {
    if (supplier) {
      form.setValue("supplierId", supplier.id);
      form.setValue("supplierName", supplier.supplierName);
    }
  };

  const handleProductChange = (
    index: number,
    productId: string,
    product: Product | undefined
  ) => {
    if (product) {
      const quantity = form.getValues(`lines.${index}.quantityOrdered`) || 1;
      form.setValue(`lines.${index}.productId`, product.id);
      form.setValue(`lines.${index}.productCode`, product.productCode);
      form.setValue(`lines.${index}.productName`, product.productName);
      form.setValue(`lines.${index}.unitPrice`, product.baseRate);
      form.setValue(`lines.${index}.lineTotal`, product.baseRate * quantity);
    }
  };

  const handleQuantityChange = (index: number, quantity: number) => {
    const unitPrice = form.getValues(`lines.${index}.unitPrice`) || 0;
    form.setValue(`lines.${index}.quantityOrdered`, quantity);
    form.setValue(`lines.${index}.lineTotal`, unitPrice * quantity);
  };

  const handleUnitPriceChange = (index: number, unitPrice: number) => {
    const quantity = form.getValues(`lines.${index}.quantityOrdered`) || 1;
    form.setValue(`lines.${index}.unitPrice`, unitPrice);
    form.setValue(`lines.${index}.lineTotal`, unitPrice * quantity);
  };

  async function onSubmit(values: PurchaseOrderFormValues) {
    try {
      if (isEditing && order) {
        const result = await updatePurchaseOrder.mutateAsync({
          id: order.id,
          data: {
            expectedDate: values.expectedDate?.toISOString(),
            notes: values.notes,
            lines: values.lines.map((line) => ({
              id: line.id,
              productId: line.productId,
              quantityOrdered: line.quantityOrdered,
              unitPrice: line.unitPrice,
            })),
          },
        });
        toast.success("Purchase order updated successfully");
        onSuccess?.(result);
      } else {
        const result = await createPurchaseOrder.mutateAsync({
          supplierId: values.supplierId,
          expectedDate: values.expectedDate?.toISOString(),
          notes: values.notes,
          lines: values.lines.map((line) => ({
            productId: line.productId,
            quantityOrdered: line.quantityOrdered,
            unitPrice: line.unitPrice,
          })),
        });
        toast.success("Purchase order created successfully");
        onSuccess?.(result);
      }
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error(isEditing ? "Failed to update purchase order" : "Failed to create purchase order", {
        description: message,
      });
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Order Details</CardTitle>
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
                        disabled={isEditing}
                        placeholder="Select a supplier..."
                      />
                    </FormControl>
                    <FormDescription>
                      The supplier for this order
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="expectedDate"
                render={({ field }) => (
                  <FormItem className="flex flex-col">
                    <FormLabel>Expected Date</FormLabel>
                    <Popover>
                      <PopoverTrigger asChild>
                        <FormControl>
                          <Button
                            variant="outline"
                            className={cn(
                              "w-full pl-3 text-left font-normal",
                              !field.value && "text-muted-foreground"
                            )}
                          >
                            {field.value ? (
                              format(field.value, "PPP")
                            ) : (
                              <span>Pick a date</span>
                            )}
                            <CalendarIcon className="ml-auto h-4 w-4 opacity-50" />
                          </Button>
                        </FormControl>
                      </PopoverTrigger>
                      <PopoverContent className="w-auto p-0" align="start">
                        <Calendar
                          mode="single"
                          selected={field.value ?? undefined}
                          onSelect={field.onChange}
                          disabled={(date) => date < new Date()}
                        />
                      </PopoverContent>
                    </Popover>
                    <FormDescription>
                      When the goods are expected
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
                      placeholder="Any additional notes for this order..."
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
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="mt-4"
                  onClick={handleAddLine}
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
                      <TableHead className="w-[300px]">Product</TableHead>
                      <TableHead className="w-[120px] text-right">Quantity</TableHead>
                      <TableHead className="w-[120px] text-right">Unit Price</TableHead>
                      <TableHead className="w-[120px] text-right">Line Total</TableHead>
                      <TableHead className="w-[60px]"></TableHead>
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
                            name={`lines.${index}.quantityOrdered`}
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
                        <TableCell className="text-right">
                          <FormField
                            control={form.control}
                            name={`lines.${index}.unitPrice`}
                            render={({ field: priceField }) => (
                              <FormItem className="space-y-0">
                                <FormControl>
                                  <Input
                                    type="number"
                                    min="0"
                                    step="0.01"
                                    className="w-full text-right"
                                    {...priceField}
                                    value={priceField.value ?? ""}
                                    onChange={(e) => {
                                      const val = e.target.valueAsNumber || 0;
                                      handleUnitPriceChange(index, val);
                                    }}
                                  />
                                </FormControl>
                                <FormMessage />
                              </FormItem>
                            )}
                          />
                        </TableCell>
                        <TableCell className="text-right font-medium">
                          {watchLines[index]?.lineTotal?.toFixed(2) ?? "0.00"}
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
                  <TableFooter>
                    <TableRow>
                      <TableCell colSpan={3} className="text-right font-medium">
                        Order Total
                      </TableCell>
                      <TableCell className="text-right font-bold text-lg">
                        {orderTotal.toFixed(2)}
                      </TableCell>
                      <TableCell></TableCell>
                    </TableRow>
                  </TableFooter>
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
                {isEditing ? "Updating..." : "Creating..."}
              </>
            ) : isEditing ? (
              "Update Order"
            ) : (
              "Create Order"
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

