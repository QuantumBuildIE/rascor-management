"use client";

import * as React from "react";
import { useForm, useFieldArray } from "react-hook-form";
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
import { SiteSearchSelect } from "@/components/stock/site-search-select";
import { LocationSearchSelect } from "@/components/stock/location-search-select";
import { useCreateStockOrder, useUpdateStockOrder } from "@/lib/api/stock/use-stock-orders";
import type { Site } from "@/types/admin";
import { useAuth } from "@/lib/auth/use-auth";
import type { StockOrder, Product, StockLocation } from "@/types/stock";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

const stockOrderLineSchema = z.object({
  id: z.string().optional(),
  productId: z.string().min(1, "Product is required"),
  productCode: z.string(),
  productName: z.string(),
  quantityRequested: z.number().min(1, "Quantity must be at least 1"),
  unitPrice: z.number().min(0),
  lineTotal: z.number(),
});

const stockOrderFormSchema = z.object({
  siteId: z.string().min(1, "Site ID is required"),
  siteName: z.string().min(1, "Site name is required"),
  sourceLocationId: z.string().min(1, "Source location is required"),
  requiredDate: z.date().optional().nullable(),
  notes: z.string().optional(),
  lines: z.array(stockOrderLineSchema).min(1, "At least one line item is required"),
});

type StockOrderFormValues = z.infer<typeof stockOrderFormSchema>;

interface StockOrderFormProps {
  order?: StockOrder;
  onSuccess?: (order: StockOrder) => void;
  onCancel?: () => void;
}

export function StockOrderForm({ order, onSuccess, onCancel }: StockOrderFormProps) {
  const isEditing = !!order;
  const { user } = useAuth();

  const createStockOrder = useCreateStockOrder();
  const updateStockOrder = useUpdateStockOrder();

  const form = useForm<StockOrderFormValues>({
    resolver: zodResolver(stockOrderFormSchema) as any,
    defaultValues: {
      siteId: order?.siteId ?? "",
      siteName: order?.siteName ?? "",
      sourceLocationId: order?.sourceLocationId ?? "",
      requiredDate: order?.requiredDate ? new Date(order.requiredDate) : null,
      notes: order?.notes ?? "",
      lines: order?.lines.map((line) => ({
        id: line.id,
        productId: line.productId,
        productCode: line.productCode,
        productName: line.productName,
        quantityRequested: line.quantityRequested,
        unitPrice: line.unitPrice,
        lineTotal: line.lineTotal,
      })) ?? [],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control: form.control,
    name: "lines",
  });

  const watchLines = form.watch("lines");
  const orderTotal = React.useMemo(
    () => watchLines.reduce((sum, line) => sum + (line.lineTotal || 0), 0),
    [watchLines]
  );

  const selectedProductIds = React.useMemo(
    () => watchLines.map((line) => line.productId).filter(Boolean),
    [watchLines]
  );

  const isSubmitting = createStockOrder.isPending || updateStockOrder.isPending;

  const handleAddLine = () => {
    append({
      productId: "",
      productCode: "",
      productName: "",
      quantityRequested: 1,
      unitPrice: 0,
      lineTotal: 0,
    });
  };

  const handleProductChange = (
    index: number,
    productId: string,
    product: Product | undefined
  ) => {
    if (product) {
      const quantity = form.getValues(`lines.${index}.quantityRequested`) || 1;
      form.setValue(`lines.${index}.productId`, product.id);
      form.setValue(`lines.${index}.productCode`, product.productCode);
      form.setValue(`lines.${index}.productName`, product.productName);
      form.setValue(`lines.${index}.unitPrice`, product.baseRate);
      form.setValue(`lines.${index}.lineTotal`, product.baseRate * quantity);
    }
  };

  const handleQuantityChange = (index: number, quantity: number) => {
    const unitPrice = form.getValues(`lines.${index}.unitPrice`) || 0;
    form.setValue(`lines.${index}.quantityRequested`, quantity);
    form.setValue(`lines.${index}.lineTotal`, unitPrice * quantity);
  };

  const handleSiteChange = (siteId: string, site: Site | undefined) => {
    if (site) {
      form.setValue("siteId", site.id);
      form.setValue("siteName", site.siteName);
    }
  };

  const handleLocationChange = (locationId: string, location: StockLocation | undefined) => {
    if (location) {
      form.setValue("sourceLocationId", location.id);
    }
  };

  async function onSubmit(values: StockOrderFormValues) {
    try {
      const requestedBy = user ? `${user.firstName} ${user.lastName}` : "Unknown";

      if (isEditing && order) {
        const result = await updateStockOrder.mutateAsync({
          id: order.id,
          data: {
            siteId: values.siteId,
            siteName: values.siteName,
            sourceLocationId: values.sourceLocationId,
            requiredDate: values.requiredDate?.toISOString(),
            notes: values.notes,
            lines: values.lines.map((line) => ({
              productId: line.productId,
              quantityRequested: line.quantityRequested,
            })),
          },
        });
        toast.success("Stock order updated successfully");
        onSuccess?.(result);
      } else {
        const result = await createStockOrder.mutateAsync({
          siteId: values.siteId,
          siteName: values.siteName,
          orderDate: new Date().toISOString(),
          sourceLocationId: values.sourceLocationId,
          requiredDate: values.requiredDate?.toISOString(),
          requestedBy,
          notes: values.notes,
          lines: values.lines.map((line) => ({
            productId: line.productId,
            quantityRequested: line.quantityRequested,
          })),
        });
        toast.success("Stock order created successfully");
        onSuccess?.(result);
      }
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error(isEditing ? "Failed to update stock order" : "Failed to create stock order", {
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
                name="siteId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Site *</FormLabel>
                    <FormControl>
                      <SiteSearchSelect
                        value={field.value}
                        onValueChange={handleSiteChange}
                        placeholder="Select a site..."
                      />
                    </FormControl>
                    <FormDescription>
                      The site requesting the stock order
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="sourceLocationId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Source Location *</FormLabel>
                    <FormControl>
                      <LocationSearchSelect
                        value={field.value}
                        onValueChange={handleLocationChange}
                        placeholder="Select source location..."
                      />
                    </FormControl>
                    <FormDescription>
                      Warehouse location to pick stock from
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <FormField
                control={form.control}
                name="requiredDate"
                render={({ field }) => (
                  <FormItem className="flex flex-col">
                    <FormLabel>Required Date</FormLabel>
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
                      When the stock is needed
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
                            name={`lines.${index}.quantityRequested`}
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
                          <span className="text-muted-foreground">
                            {watchLines[index]?.unitPrice?.toFixed(2) ?? "0.00"}
                          </span>
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

