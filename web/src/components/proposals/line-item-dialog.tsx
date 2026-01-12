"use client";

import * as React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { cn } from "@/lib/utils";
import { Check, ChevronsUpDown } from "lucide-react";
import { useProducts } from "@/lib/api/stock/use-products";
import {
  useAddLineItem,
  useUpdateLineItem,
  type ProposalLineItem,
} from "@/lib/api/proposals";
import { toast } from "sonner";
import { useHasAnyPermission } from "@/lib/auth/use-auth";

const lineItemSchema = z.object({
  productId: z.string().optional(),
  description: z.string().min(1, "Description is required"),
  quantity: z.number().min(0.01, "Quantity must be greater than 0"),
  unit: z.string().min(1, "Unit is required"),
  unitCost: z.number().min(0, "Unit cost must be 0 or greater"),
  unitPrice: z.number().min(0, "Unit price must be 0 or greater"),
  sortOrder: z.number().min(0, "Sort order must be 0 or greater"),
  notes: z.string().optional(),
});

type LineItemFormValues = z.infer<typeof lineItemSchema>;

interface LineItemDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  sectionId: string;
  lineItem?: ProposalLineItem | null;
  nextSortOrder?: number;
}

export function LineItemDialog({
  open,
  onOpenChange,
  sectionId,
  lineItem,
  nextSortOrder = 0,
}: LineItemDialogProps) {
  const [productPopoverOpen, setProductPopoverOpen] = React.useState(false);
  const canViewCostings = useHasAnyPermission([
    "Proposals.ViewCostings",
    "Proposals.Admin",
  ]);

  const { data: productsData } = useProducts({ pageSize: 200, isActive: true });
  const addLineItem = useAddLineItem();
  const updateLineItem = useUpdateLineItem();

  const products = productsData?.items ?? [];
  const isEditing = !!lineItem;

  const form = useForm<LineItemFormValues>({
    resolver: zodResolver(lineItemSchema),
    defaultValues: {
      productId: "",
      description: "",
      quantity: 1,
      unit: "Each",
      unitCost: 0,
      unitPrice: 0,
      sortOrder: nextSortOrder,
      notes: "",
    },
  });

  const watchQuantity = form.watch("quantity");
  const watchUnitCost = form.watch("unitCost");
  const watchUnitPrice = form.watch("unitPrice");

  const lineTotal = watchQuantity * watchUnitPrice;
  const lineCost = watchQuantity * watchUnitCost;
  const lineMargin = lineTotal - lineCost;
  const marginPercent = lineTotal > 0 ? (lineMargin / lineTotal) * 100 : 0;

  React.useEffect(() => {
    if (open) {
      if (lineItem) {
        form.reset({
          productId: lineItem.productId ?? "",
          description: lineItem.description,
          quantity: lineItem.quantity,
          unit: lineItem.unit,
          unitCost: lineItem.unitCost,
          unitPrice: lineItem.unitPrice,
          sortOrder: lineItem.sortOrder,
          notes: lineItem.notes ?? "",
        });
      } else {
        form.reset({
          productId: "",
          description: "",
          quantity: 1,
          unit: "Each",
          unitCost: 0,
          unitPrice: 0,
          sortOrder: nextSortOrder,
          notes: "",
        });
      }
    }
  }, [open, lineItem, nextSortOrder, form]);

  const handleProductSelect = (productId: string) => {
    const product = products.find((p) => p.id === productId);
    if (product) {
      form.setValue("productId", productId);
      form.setValue("description", product.productName);
      form.setValue("unit", product.unitType);
      form.setValue("unitCost", product.costPrice ?? 0);
      form.setValue("unitPrice", product.sellPrice ?? product.baseRate);
    }
    setProductPopoverOpen(false);
  };

  const onSubmit = async (values: LineItemFormValues) => {
    try {
      if (isEditing && lineItem) {
        await updateLineItem.mutateAsync({
          itemId: lineItem.id,
          data: {
            productId: values.productId || undefined,
            description: values.description,
            quantity: values.quantity,
            unit: values.unit,
            unitCost: values.unitCost,
            unitPrice: values.unitPrice,
            sortOrder: values.sortOrder,
            notes: values.notes,
          },
        });
        toast.success("Line item updated successfully");
      } else {
        await addLineItem.mutateAsync({
          sectionId,
          data: {
            productId: values.productId || undefined,
            description: values.description,
            quantity: values.quantity,
            unit: values.unit,
            unitCost: values.unitCost,
            unitPrice: values.unitPrice,
            sortOrder: values.sortOrder,
            notes: values.notes,
          },
        });
        toast.success("Line item added successfully");
      }
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error(
        isEditing ? "Failed to update line item" : "Failed to add line item",
        { description: message }
      );
    }
  };

  const isPending = addLineItem.isPending || updateLineItem.isPending;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[550px]">
        <DialogHeader>
          <DialogTitle>
            {isEditing ? "Edit Line Item" : "Add Line Item"}
          </DialogTitle>
          <DialogDescription>
            {isEditing
              ? "Update the line item details"
              : "Add a new item to this section"}
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="productId"
              render={({ field }) => (
                <FormItem className="flex flex-col">
                  <FormLabel>Product (optional)</FormLabel>
                  <Popover
                    open={productPopoverOpen}
                    onOpenChange={setProductPopoverOpen}
                  >
                    <PopoverTrigger asChild>
                      <FormControl>
                        <Button
                          variant="outline"
                          role="combobox"
                          className={cn(
                            "w-full justify-between",
                            !field.value && "text-muted-foreground"
                          )}
                        >
                          {field.value
                            ? products.find((p) => p.id === field.value)?.productName
                            : "Select a product or enter manually..."}
                          <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                        </Button>
                      </FormControl>
                    </PopoverTrigger>
                    <PopoverContent className="w-[450px] p-0" align="start">
                      <Command>
                        <CommandInput placeholder="Search products..." />
                        <CommandList>
                          <CommandEmpty>No products found.</CommandEmpty>
                          <CommandGroup>
                            <CommandItem
                              value=""
                              onSelect={() => {
                                field.onChange("");
                                setProductPopoverOpen(false);
                              }}
                            >
                              <Check
                                className={cn(
                                  "mr-2 h-4 w-4",
                                  !field.value ? "opacity-100" : "opacity-0"
                                )}
                              />
                              <span className="text-muted-foreground">
                                Ad-hoc item (no product)
                              </span>
                            </CommandItem>
                            {products.map((product) => (
                              <CommandItem
                                key={product.id}
                                value={`${product.productCode} ${product.productName}`}
                                onSelect={() => handleProductSelect(product.id)}
                              >
                                <Check
                                  className={cn(
                                    "mr-2 h-4 w-4",
                                    product.id === field.value
                                      ? "opacity-100"
                                      : "opacity-0"
                                  )}
                                />
                                <div className="flex flex-col">
                                  <span>{product.productName}</span>
                                  <span className="text-xs text-muted-foreground">
                                    {product.productCode} - {product.unitType}
                                  </span>
                                </div>
                              </CommandItem>
                            ))}
                          </CommandGroup>
                        </CommandList>
                      </Command>
                    </PopoverContent>
                  </Popover>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="description"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Description *</FormLabel>
                  <FormControl>
                    <Input placeholder="Item description" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="grid grid-cols-3 gap-4">
              <FormField
                control={form.control}
                name="quantity"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Quantity *</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        step="0.01"
                        min="0.01"
                        value={field.value}
                        onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="unit"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Unit *</FormLabel>
                    <FormControl>
                      <Input placeholder="Each, m², hours" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="sortOrder"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Sort Order</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        min={0}
                        value={field.value}
                        onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              {canViewCostings && (
                <FormField
                  control={form.control}
                  name="unitCost"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Unit Cost</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          step="0.01"
                          min="0"
                          value={field.value}
                          onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              )}

              <FormField
                control={form.control}
                name="unitPrice"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Unit Price *</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        step="0.01"
                        min="0"
                        value={field.value}
                        onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* Calculated fields */}
            <div className="rounded-md border p-4 space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Line Total:</span>
                <span className="font-medium">
                  {lineTotal.toLocaleString("en-IE", {
                    style: "currency",
                    currency: "EUR",
                  })}
                </span>
              </div>
              {canViewCostings && (
                <>
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Line Cost:</span>
                    <span className="font-medium">
                      {lineCost.toLocaleString("en-IE", {
                        style: "currency",
                        currency: "EUR",
                      })}
                    </span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Margin:</span>
                    <span
                      className={cn(
                        "font-medium",
                        lineMargin >= 0 ? "text-green-600" : "text-red-600"
                      )}
                    >
                      {lineMargin.toLocaleString("en-IE", {
                        style: "currency",
                        currency: "EUR",
                      })}{" "}
                      ({marginPercent.toFixed(1)}%)
                    </span>
                  </div>
                </>
              )}
            </div>

            <FormField
              control={form.control}
              name="notes"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Notes</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Optional notes for this item..."
                      className="resize-none"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={isPending}>
                {isPending ? "Saving..." : isEditing ? "Update Item" : "Add Item"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
