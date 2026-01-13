"use client";

import * as React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
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
import { cn } from "@/lib/utils";
import {
  useAddProductKitItem,
  useUpdateProductKitItem,
  type ProductKitItem,
} from "@/lib/api/stock/use-product-kits";
import { useAllProducts } from "@/lib/api/stock/use-products";
import { toast } from "sonner";

const kitItemFormSchema = z.object({
  productId: z.string().min(1, "Product is required"),
  defaultQuantity: z.number().min(0.01, "Quantity must be greater than 0"),
  sortOrder: z.number().min(0, "Sort order must be 0 or greater"),
  notes: z.string().nullable(),
});

type KitItemFormValues = z.infer<typeof kitItemFormSchema>;

interface KitItemDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  kitId: string;
  item?: ProductKitItem;
  existingProductIds?: string[];
  nextSortOrder?: number;
}

export function KitItemDialog({
  open,
  onOpenChange,
  kitId,
  item,
  existingProductIds = [],
  nextSortOrder = 0,
}: KitItemDialogProps) {
  const isEditing = !!item;
  const [productSearchOpen, setProductSearchOpen] = React.useState(false);

  const addItem = useAddProductKitItem();
  const updateItem = useUpdateProductKitItem();
  const { data: products } = useAllProducts();

  const form = useForm<KitItemFormValues>({
    resolver: zodResolver(kitItemFormSchema) as any,
    defaultValues: {
      productId: item?.productId ?? "",
      defaultQuantity: item?.defaultQuantity ?? 1,
      sortOrder: item?.sortOrder ?? nextSortOrder,
      notes: item?.notes ?? "",
    },
  });

  // Reset form when dialog opens/closes or item changes
  React.useEffect(() => {
    if (open) {
      form.reset({
        productId: item?.productId ?? "",
        defaultQuantity: item?.defaultQuantity ?? 1,
        sortOrder: item?.sortOrder ?? nextSortOrder,
        notes: item?.notes ?? "",
      });
    }
  }, [open, item, nextSortOrder, form]);

  const selectedProductId = form.watch("productId");
  const selectedProduct = products?.find((p) => p.id === selectedProductId);

  // Filter out products already in the kit (except current one if editing)
  const availableProducts = React.useMemo(() => {
    if (!products) return [];
    return products.filter(
      (p) => p.isActive && (!existingProductIds.includes(p.id) || p.id === item?.productId)
    );
  }, [products, existingProductIds, item?.productId]);

  const isSubmitting = addItem.isPending || updateItem.isPending;

  async function onSubmit(values: KitItemFormValues) {
    try {
      if (isEditing) {
        await updateItem.mutateAsync({
          itemId: item.id,
          kitId,
          data: {
            productId: values.productId,
            defaultQuantity: values.defaultQuantity,
            sortOrder: values.sortOrder,
            notes: values.notes || undefined,
          },
        });
        toast.success("Kit item updated successfully");
      } else {
        await addItem.mutateAsync({
          kitId,
          data: {
            productId: values.productId,
            defaultQuantity: values.defaultQuantity,
            sortOrder: values.sortOrder,
            notes: values.notes || undefined,
          },
        });
        toast.success("Item added to kit successfully");
      }
      onOpenChange(false);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "An error occurred";
      toast.error(
        isEditing ? "Failed to update kit item" : "Failed to add kit item",
        {
          description: message,
        }
      );
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>{isEditing ? "Edit Kit Item" : "Add Kit Item"}</DialogTitle>
          <DialogDescription>
            {isEditing
              ? "Update the item details in this kit."
              : "Add a product to this kit with a default quantity."}
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="productId"
              render={({ field }) => (
                <FormItem className="flex flex-col">
                  <FormLabel>Product *</FormLabel>
                  <Popover open={productSearchOpen} onOpenChange={setProductSearchOpen}>
                    <PopoverTrigger asChild>
                      <FormControl>
                        <Button
                          variant="outline"
                          role="combobox"
                          aria-expanded={productSearchOpen}
                          className={cn(
                            "w-full justify-between",
                            !field.value && "text-muted-foreground"
                          )}
                        >
                          {selectedProduct
                            ? `${selectedProduct.productCode} - ${selectedProduct.productName}`
                            : "Select a product..."}
                          <ChevronsUpDownIcon className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                        </Button>
                      </FormControl>
                    </PopoverTrigger>
                    <PopoverContent className="w-[450px] p-0" align="start">
                      <Command>
                        <CommandInput placeholder="Search products..." />
                        <CommandList>
                          <CommandEmpty>No products found.</CommandEmpty>
                          <CommandGroup>
                            {availableProducts.map((product) => (
                              <CommandItem
                                key={product.id}
                                value={`${product.productCode} ${product.productName}`}
                                onSelect={() => {
                                  field.onChange(product.id);
                                  setProductSearchOpen(false);
                                }}
                              >
                                <CheckIcon
                                  className={cn(
                                    "mr-2 h-4 w-4",
                                    field.value === product.id
                                      ? "opacity-100"
                                      : "opacity-0"
                                  )}
                                />
                                <div className="flex flex-col">
                                  <span className="font-medium">
                                    {product.productCode} - {product.productName}
                                  </span>
                                  <span className="text-xs text-muted-foreground">
                                    {product.unitType} | Cost: {formatCurrency(product.costPrice ?? 0)} | Price: {formatCurrency(product.sellPrice ?? product.baseRate)}
                                  </span>
                                </div>
                              </CommandItem>
                            ))}
                          </CommandGroup>
                        </CommandList>
                      </Command>
                    </PopoverContent>
                  </Popover>
                  {selectedProduct && (
                    <div className="rounded-md bg-muted p-3 text-sm">
                      <div className="grid grid-cols-2 gap-2">
                        <div>
                          <span className="text-muted-foreground">Unit:</span>{" "}
                          <span className="font-medium">{selectedProduct.unitType}</span>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Cost:</span>{" "}
                          <span className="font-medium">
                            {formatCurrency(selectedProduct.costPrice ?? 0)}
                          </span>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Price:</span>{" "}
                          <span className="font-medium">
                            {formatCurrency(selectedProduct.sellPrice ?? selectedProduct.baseRate)}
                          </span>
                        </div>
                      </div>
                    </div>
                  )}
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="grid gap-4 sm:grid-cols-2">
              <FormField
                control={form.control}
                name="defaultQuantity"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Default Quantity *</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        step="0.01"
                        min="0.01"
                        placeholder="1"
                        {...field}
                        value={field.value}
                        onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                      />
                    </FormControl>
                    <FormDescription>
                      Quantity when added to an order
                    </FormDescription>
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
                        step="1"
                        min="0"
                        placeholder="0"
                        {...field}
                        value={field.value}
                        onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                      />
                    </FormControl>
                    <FormDescription>
                      Display order in the kit
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
                    <Input
                      placeholder="Optional notes about this item..."
                      {...field}
                      value={field.value ?? ""}
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
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? (
                  <>
                    <LoadingSpinner className="mr-2 h-4 w-4" />
                    {isEditing ? "Updating..." : "Adding..."}
                  </>
                ) : isEditing ? (
                  "Update Item"
                ) : (
                  "Add Item"
                )}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}

function formatCurrency(value: number): string {
  return new Intl.NumberFormat("en-IE", {
    style: "currency",
    currency: "EUR",
  }).format(value);
}

function ChevronsUpDownIcon({ className }: { className?: string }) {
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
        d="M7 15l5 5 5-5M7 9l5-5 5 5"
      />
    </svg>
  );
}

function CheckIcon({ className }: { className?: string }) {
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
        d="M5 13l4 4L19 7"
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

