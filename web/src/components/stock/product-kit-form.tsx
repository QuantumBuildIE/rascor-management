"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
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
import {
  useCreateProductKit,
  useUpdateProductKit,
  type ProductKit,
} from "@/lib/api/stock/use-product-kits";
import { useCategories } from "@/lib/api/stock/use-categories";
import { toast } from "sonner";

const productKitFormSchema = z.object({
  kitCode: z.string().min(1, "Kit code is required"),
  kitName: z.string().min(1, "Kit name is required"),
  description: z.string().nullable(),
  categoryId: z.string().nullable(),
  isActive: z.boolean(),
  notes: z.string().nullable(),
});

type ProductKitFormValues = z.infer<typeof productKitFormSchema>;

interface ProductKitFormProps {
  productKit?: ProductKit;
  onSuccess?: (id: string) => void;
  onCancel?: () => void;
}

export function ProductKitForm({
  productKit,
  onSuccess,
  onCancel,
}: ProductKitFormProps) {
  const isEditing = !!productKit;

  const createProductKit = useCreateProductKit();
  const updateProductKit = useUpdateProductKit();
  const { data: categories } = useCategories();

  const form = useForm<ProductKitFormValues>({
    resolver: zodResolver(productKitFormSchema) as any,
    defaultValues: {
      kitCode: productKit?.kitCode ?? "",
      kitName: productKit?.kitName ?? "",
      description: productKit?.description ?? "",
      categoryId: productKit?.categoryId ?? null,
      isActive: productKit?.isActive ?? true,
      notes: productKit?.notes ?? "",
    },
  });

  const isSubmitting = createProductKit.isPending || updateProductKit.isPending;

  async function onSubmit(values: ProductKitFormValues) {
    try {
      if (isEditing) {
        await updateProductKit.mutateAsync({
          id: productKit.id,
          data: {
            kitCode: values.kitCode,
            kitName: values.kitName,
            description: values.description || undefined,
            categoryId: values.categoryId || undefined,
            isActive: values.isActive,
            notes: values.notes || undefined,
          },
        });
        toast.success("Product kit updated successfully");
        onSuccess?.(productKit.id);
      } else {
        const result = await createProductKit.mutateAsync({
          kitCode: values.kitCode,
          kitName: values.kitName,
          description: values.description || undefined,
          categoryId: values.categoryId || undefined,
          isActive: values.isActive,
          notes: values.notes || undefined,
        });
        toast.success("Product kit created successfully");
        onSuccess?.(result.id);
      }
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "An error occurred";
      toast.error(
        isEditing ? "Failed to update product kit" : "Failed to create product kit",
        {
          description: message,
        }
      );
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <div className="grid gap-6 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="kitCode"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Kit Code *</FormLabel>
                <FormControl>
                  <Input placeholder="e.g., KIT-001" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="kitName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Kit Name *</FormLabel>
                <FormControl>
                  <Input placeholder="e.g., Bathroom Fitting Kit" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="categoryId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Category</FormLabel>
              <Select
                onValueChange={(value) => field.onChange(value === "none" ? null : value)}
                value={field.value ?? "none"}
              >
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder="Select a category" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  <SelectItem value="none">No category</SelectItem>
                  {categories?.map((category) => (
                    <SelectItem key={category.id} value={category.id}>
                      {category.categoryName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <FormDescription>
                Optionally assign this kit to a category
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Description</FormLabel>
              <FormControl>
                <Textarea
                  placeholder="Describe what this kit contains and when to use it..."
                  className="resize-none"
                  rows={3}
                  {...field}
                  value={field.value ?? ""}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="notes"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Notes</FormLabel>
              <FormControl>
                <Textarea
                  placeholder="Internal notes about this kit..."
                  className="resize-none"
                  rows={2}
                  {...field}
                  value={field.value ?? ""}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

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
                  Inactive kits won&apos;t appear in order forms
                </FormDescription>
              </div>
            </FormItem>
          )}
        />

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
              "Update Kit"
            ) : (
              "Create Kit"
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

