"use client";

import * as React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useCategories } from "@/lib/api/stock/use-categories";
import { useSuppliers } from "@/lib/api/stock/use-suppliers";
import { useCreateProduct, useUpdateProduct } from "@/lib/api/stock/use-products";
import { uploadProductImage, deleteProductImage } from "@/lib/api/stock/products";
import type { Product } from "@/types/stock";
import { toast } from "sonner";
import { Upload, X, ImageIcon } from "lucide-react";
import { useQueryClient } from "@tanstack/react-query";

const UNIT_TYPES = [
  "Each",
  "Box",
  "Pack",
  "Litre",
  "Bag",
  "Length",
  "Board",
  "Roll",
  "Pair",
  "Set",
  "Metre",
  "Kilogram",
] as const;

const PRODUCT_TYPES = [
  "Main Product",
  "Ancillary Product",
  "Tool",
  "Consumable",
] as const;

const productFormSchema = z.object({
  productCode: z.string().min(1, "Product code is required"),
  productName: z.string().min(1, "Product name is required"),
  categoryId: z.string().min(1, "Category is required"),
  supplierId: z.string().nullable(),
  unitType: z.string().min(1, "Unit type is required"),
  baseRate: z.number().min(0, "Base rate must be 0 or greater"),
  reorderLevel: z.number().min(0).optional(),
  reorderQuantity: z.number().min(0).optional(),
  leadTimeDays: z.number().min(0).optional(),
  isActive: z.boolean(),
  costPrice: z.number().min(0).nullable().optional(),
  sellPrice: z.number().min(0).nullable().optional(),
  productType: z.string().nullable().optional(),
});

type ProductFormValues = z.infer<typeof productFormSchema>;

interface ProductFormProps {
  product?: Product;
  onSuccess?: () => void;
  onCancel?: () => void;
}

export function ProductForm({ product, onSuccess, onCancel }: ProductFormProps) {
  const isEditing = !!product;
  const queryClient = useQueryClient();

  const { data: categories, isLoading: categoriesLoading } = useCategories();
  const { data: suppliers, isLoading: suppliersLoading } = useSuppliers();

  const createProduct = useCreateProduct();
  const updateProduct = useUpdateProduct();

  const [imageFile, setImageFile] = React.useState<File | null>(null);
  const [imagePreview, setImagePreview] = React.useState<string | null>(
    product?.imageUrl ? `http://localhost:5222${product.imageUrl}` : null
  );
  const [isUploadingImage, setIsUploadingImage] = React.useState(false);
  const fileInputRef = React.useRef<HTMLInputElement>(null);

  const form = useForm<ProductFormValues>({
    resolver: zodResolver(productFormSchema) as any,
    defaultValues: {
      productCode: product?.productCode ?? "",
      productName: product?.productName ?? "",
      categoryId: product?.categoryId ?? "",
      supplierId: product?.supplierId ?? null,
      unitType: product?.unitType ?? "Each",
      baseRate: product?.baseRate ?? 0,
      reorderLevel: product?.reorderLevel ?? 0,
      reorderQuantity: product?.reorderQuantity ?? 0,
      leadTimeDays: product?.leadTimeDays ?? 0,
      isActive: product?.isActive ?? true,
      costPrice: product?.costPrice ?? null,
      sellPrice: product?.sellPrice ?? null,
      productType: product?.productType ?? null,
    },
  });

  // Watch cost and sell prices to calculate margins
  const costPrice = form.watch("costPrice");
  const sellPrice = form.watch("sellPrice");

  const marginAmount = sellPrice && costPrice ? sellPrice - costPrice : null;
  const marginPercent = sellPrice && costPrice && sellPrice > 0
    ? ((sellPrice - costPrice) / sellPrice * 100).toFixed(2)
    : null;

  const isSubmitting = createProduct.isPending || updateProduct.isPending;

  const handleImageSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validate file type
    const allowedTypes = ["image/jpeg", "image/jpg", "image/png", "image/webp"];
    if (!allowedTypes.includes(file.type)) {
      toast.error("Invalid file type", {
        description: "Please upload a JPG, PNG, or WebP image",
      });
      return;
    }

    // Validate file size (5MB)
    if (file.size > 5 * 1024 * 1024) {
      toast.error("File too large", {
        description: "Maximum file size is 5MB",
      });
      return;
    }

    setImageFile(file);
    const reader = new FileReader();
    reader.onloadend = () => {
      setImagePreview(reader.result as string);
    };
    reader.readAsDataURL(file);
  };

  const handleImageUpload = async () => {
    if (!imageFile || !product?.id) return;

    setIsUploadingImage(true);
    try {
      await uploadProductImage(product.id, imageFile);
      toast.success("Image uploaded successfully");
      setImageFile(null);
      queryClient.invalidateQueries({ queryKey: ["product", product.id] });
      queryClient.invalidateQueries({ queryKey: ["products"] });
    } catch (error) {
      const message = error instanceof Error ? error.message : "Failed to upload image";
      toast.error("Upload failed", { description: message });
    } finally {
      setIsUploadingImage(false);
    }
  };

  const handleImageRemove = async () => {
    if (!product?.id) return;

    setIsUploadingImage(true);
    try {
      await deleteProductImage(product.id);
      toast.success("Image removed successfully");
      setImagePreview(null);
      setImageFile(null);
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
      queryClient.invalidateQueries({ queryKey: ["product", product.id] });
      queryClient.invalidateQueries({ queryKey: ["products"] });
    } catch (error) {
      const message = error instanceof Error ? error.message : "Failed to remove image";
      toast.error("Remove failed", { description: message });
    } finally {
      setIsUploadingImage(false);
    }
  };

  async function onSubmit(values: ProductFormValues) {
    try {
      if (isEditing) {
        await updateProduct.mutateAsync({
          id: product.id,
          data: {
            productCode: values.productCode,
            productName: values.productName,
            categoryId: values.categoryId,
            supplierId: values.supplierId || null,
            unitType: values.unitType,
            baseRate: values.baseRate,
            reorderLevel: values.reorderLevel,
            reorderQuantity: values.reorderQuantity,
            leadTimeDays: values.leadTimeDays,
            isActive: values.isActive,
            costPrice: values.costPrice,
            sellPrice: values.sellPrice,
            productType: values.productType,
          },
        });
        toast.success("Product updated successfully");
      } else {
        await createProduct.mutateAsync({
          productCode: values.productCode,
          productName: values.productName,
          categoryId: values.categoryId,
          supplierId: values.supplierId || undefined,
          unitType: values.unitType,
          baseRate: values.baseRate,
          reorderLevel: values.reorderLevel,
          reorderQuantity: values.reorderQuantity,
          leadTimeDays: values.leadTimeDays,
          isActive: values.isActive,
          costPrice: values.costPrice,
          sellPrice: values.sellPrice,
          productType: values.productType,
        });
        toast.success("Product created successfully");
      }
      onSuccess?.();
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error(isEditing ? "Failed to update product" : "Failed to create product", {
        description: message,
      });
    }
  }

  const activeCategories = categories?.filter((c) => c.isActive) ?? [];
  const activeSuppliers = suppliers?.filter((s) => s.isActive) ?? [];

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <div className="grid gap-6 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="productCode"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Product Code *</FormLabel>
                <FormControl>
                  <Input placeholder="e.g., PROD-001" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="productName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Product Name *</FormLabel>
                <FormControl>
                  <Input placeholder="e.g., Safety Helmet" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="grid gap-6 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="categoryId"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Category *</FormLabel>
                <Select
                  onValueChange={field.onChange}
                  defaultValue={field.value}
                  disabled={categoriesLoading}
                >
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Select a category" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {activeCategories.map((category) => (
                      <SelectItem key={category.id} value={category.id}>
                        {category.categoryName}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="supplierId"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Supplier</FormLabel>
                <Select
                  onValueChange={(value) => field.onChange(value === "none" ? null : value)}
                  defaultValue={field.value ?? "none"}
                  disabled={suppliersLoading}
                >
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Select a supplier (optional)" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    <SelectItem value="none">No supplier</SelectItem>
                    {activeSuppliers.map((supplier) => (
                      <SelectItem key={supplier.id} value={supplier.id}>
                        {supplier.supplierName}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormDescription>Optional - select if this product has a preferred supplier</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="grid gap-6 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="unitType"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Unit Type *</FormLabel>
                <Select onValueChange={field.onChange} defaultValue={field.value}>
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Select unit type" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {UNIT_TYPES.map((unit) => (
                      <SelectItem key={unit} value={unit}>
                        {unit}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="baseRate"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Base Rate (£) *</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    step="0.01"
                    min="0"
                    placeholder="0.00"
                    {...field}
                    value={field.value ?? ""}
                    onChange={(e) => field.onChange(e.target.valueAsNumber || 0)}
                  />
                </FormControl>
                <FormDescription>Cost price per unit</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="grid gap-6 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="productType"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Product Type</FormLabel>
                <Select
                  onValueChange={(value) => field.onChange(value === "none" ? null : value)}
                  defaultValue={field.value ?? "none"}
                >
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Select product type" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    <SelectItem value="none">Not specified</SelectItem>
                    {PRODUCT_TYPES.map((type) => (
                      <SelectItem key={type} value={type}>
                        {type}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="grid gap-6 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="costPrice"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Cost Price (£)</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    step="0.01"
                    min="0"
                    placeholder="0.00"
                    {...field}
                    value={field.value ?? ""}
                    onChange={(e) => field.onChange(e.target.value ? e.target.valueAsNumber : null)}
                  />
                </FormControl>
                <FormDescription>Purchase cost per unit</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="sellPrice"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Sell Price (£)</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    step="0.01"
                    min="0"
                    placeholder="0.00"
                    {...field}
                    value={field.value ?? ""}
                    onChange={(e) => field.onChange(e.target.value ? e.target.valueAsNumber : null)}
                  />
                </FormControl>
                <FormDescription>Selling price per unit</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        {sellPrice && (
          <div className="rounded-md border p-4 bg-muted/50">
            <div className="grid gap-4 sm:grid-cols-2">
              <div>
                <p className="text-sm text-muted-foreground">Margin Amount</p>
                <p className="text-lg font-semibold">
                  {marginAmount !== null ? `£${marginAmount.toFixed(2)}` : "-"}
                </p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Margin %</p>
                <p className="text-lg font-semibold">
                  {marginPercent !== null ? `${marginPercent}%` : "-"}
                </p>
              </div>
            </div>
          </div>
        )}

        <div className="grid gap-6 sm:grid-cols-3">
          <FormField
            control={form.control}
            name="reorderLevel"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Reorder Level</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    min="0"
                    placeholder="0"
                    {...field}
                    value={field.value ?? ""}
                    onChange={(e) => field.onChange(e.target.valueAsNumber || 0)}
                  />
                </FormControl>
                <FormDescription>Alert when stock falls below</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="reorderQuantity"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Reorder Quantity</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    min="0"
                    placeholder="0"
                    {...field}
                    value={field.value ?? ""}
                    onChange={(e) => field.onChange(e.target.valueAsNumber || 0)}
                  />
                </FormControl>
                <FormDescription>Default quantity to order</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="leadTimeDays"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Lead Time (Days)</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    min="0"
                    placeholder="0"
                    {...field}
                    value={field.value ?? ""}
                    onChange={(e) => field.onChange(e.target.valueAsNumber || 0)}
                  />
                </FormControl>
                <FormDescription>Typical delivery time</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        {isEditing && (
          <div className="rounded-md border p-4">
            <div className="space-y-4">
              <div>
                <h3 className="text-sm font-medium">Product Image</h3>
                <p className="text-sm text-muted-foreground">
                  Upload a product image (JPG, PNG, or WebP, max 5MB)
                </p>
              </div>

              <div className="flex flex-col gap-4 sm:flex-row sm:items-start">
                <div className="relative h-32 w-32 rounded-md border bg-muted flex items-center justify-center overflow-hidden mx-auto sm:mx-0">
                  {imagePreview ? (
                    <img
                      src={imagePreview}
                      alt="Product preview"
                      className="h-full w-full object-cover"
                    />
                  ) : (
                    <ImageIcon className="h-12 w-12 text-muted-foreground" />
                  )}
                </div>

                <div className="flex-1 space-y-2">
                  <input
                    ref={fileInputRef}
                    type="file"
                    accept="image/jpeg,image/jpg,image/png,image/webp"
                    onChange={handleImageSelect}
                    className="hidden"
                    id="product-image-upload"
                  />

                  <div className="flex flex-col gap-2 sm:flex-row">
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={() => fileInputRef.current?.click()}
                      disabled={isUploadingImage}
                      className="w-full sm:w-auto"
                    >
                      <Upload className="mr-2 h-4 w-4" />
                      Choose Image
                    </Button>

                    {imageFile && (
                      <Button
                        type="button"
                        size="sm"
                        onClick={handleImageUpload}
                        disabled={isUploadingImage}
                        className="w-full sm:w-auto"
                      >
                        {isUploadingImage ? "Uploading..." : "Upload"}
                      </Button>
                    )}

                    {imagePreview && !imageFile && (
                      <Button
                        type="button"
                        variant="destructive"
                        size="sm"
                        onClick={handleImageRemove}
                        disabled={isUploadingImage}
                        className="w-full sm:w-auto"
                      >
                        <X className="mr-2 h-4 w-4" />
                        Remove
                      </Button>
                    )}
                  </div>

                  {imageFile && (
                    <p className="text-sm text-muted-foreground">
                      {imageFile.name} ({(imageFile.size / 1024).toFixed(0)} KB)
                    </p>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}

        <FormField
          control={form.control}
          name="isActive"
          render={({ field }) => (
            <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
              <FormControl>
                <Checkbox
                  checked={field.value}
                  onCheckedChange={field.onChange}
                />
              </FormControl>
              <div className="space-y-1 leading-none">
                <FormLabel>Active</FormLabel>
                <FormDescription>
                  Inactive products won&apos;t appear in order forms
                </FormDescription>
              </div>
            </FormItem>
          )}
        />

        <div className="flex flex-col gap-3 sm:flex-row sm:justify-end sm:gap-4">
          {onCancel && (
            <Button type="button" variant="outline" onClick={onCancel} className="w-full sm:w-auto">
              Cancel
            </Button>
          )}
          <Button type="submit" disabled={isSubmitting} className="w-full sm:w-auto">
            {isSubmitting ? (
              <>
                <LoadingSpinner className="mr-2 h-4 w-4" />
                {isEditing ? "Updating..." : "Creating..."}
              </>
            ) : isEditing ? (
              "Update Product"
            ) : (
              "Create Product"
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

