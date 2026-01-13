"use client";

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
  useCreateSupplier,
  useUpdateSupplier,
} from "@/lib/api/stock/use-suppliers";
import type { Supplier } from "@/types/stock";
import { toast } from "sonner";

const supplierFormSchema = z.object({
  supplierCode: z.string().min(1, "Supplier code is required"),
  supplierName: z.string().min(1, "Supplier name is required"),
  contactName: z.string().nullable(),
  email: z.string().email("Invalid email address").nullable().or(z.literal("")),
  phone: z.string().nullable(),
  address: z.string().nullable(),
  paymentTerms: z.string().nullable(),
  isActive: z.boolean(),
});

type SupplierFormValues = z.infer<typeof supplierFormSchema>;

interface SupplierFormProps {
  supplier?: Supplier;
  onSuccess?: () => void;
  onCancel?: () => void;
}

export function SupplierForm({
  supplier,
  onSuccess,
  onCancel,
}: SupplierFormProps) {
  const isEditing = !!supplier;

  const createSupplier = useCreateSupplier();
  const updateSupplier = useUpdateSupplier();

  const form = useForm<SupplierFormValues>({
    resolver: zodResolver(supplierFormSchema) as any,
    defaultValues: {
      supplierCode: supplier?.supplierCode ?? "",
      supplierName: supplier?.supplierName ?? "",
      contactName: supplier?.contactName ?? "",
      email: supplier?.email ?? "",
      phone: supplier?.phone ?? "",
      address: supplier?.address ?? "",
      paymentTerms: supplier?.paymentTerms ?? "",
      isActive: supplier?.isActive ?? true,
    },
  });

  const isSubmitting = createSupplier.isPending || updateSupplier.isPending;

  async function onSubmit(values: SupplierFormValues) {
    try {
      if (isEditing) {
        // For update, convert empty strings to null
        await updateSupplier.mutateAsync({
          id: supplier.id,
          data: {
            supplierCode: values.supplierCode,
            supplierName: values.supplierName,
            contactName: values.contactName || null,
            email: values.email || null,
            phone: values.phone || null,
            address: values.address || null,
            paymentTerms: values.paymentTerms || null,
            isActive: values.isActive,
          },
        });
        toast.success("Supplier updated successfully");
      } else {
        // For create, convert empty strings to undefined
        await createSupplier.mutateAsync({
          supplierCode: values.supplierCode,
          supplierName: values.supplierName,
          contactName: values.contactName || undefined,
          email: values.email || undefined,
          phone: values.phone || undefined,
          address: values.address || undefined,
          paymentTerms: values.paymentTerms || undefined,
          isActive: values.isActive,
        });
        toast.success("Supplier created successfully");
      }
      onSuccess?.();
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "An error occurred";
      toast.error(
        isEditing ? "Failed to update supplier" : "Failed to create supplier",
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
            name="supplierCode"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Supplier Code *</FormLabel>
                <FormControl>
                  <Input placeholder="e.g., SUP-001" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="supplierName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Supplier Name *</FormLabel>
                <FormControl>
                  <Input placeholder="e.g., ABC Supplies Ltd" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="grid gap-6 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="contactName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Contact Name</FormLabel>
                <FormControl>
                  <Input
                    placeholder="e.g., John Smith"
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
            name="email"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Email</FormLabel>
                <FormControl>
                  <Input
                    type="email"
                    placeholder="e.g., contact@supplier.com"
                    {...field}
                    value={field.value ?? ""}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="grid gap-6 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="phone"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Phone</FormLabel>
                <FormControl>
                  <Input
                    placeholder="e.g., +353 1 234 5678"
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
            name="paymentTerms"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Payment Terms</FormLabel>
                <FormControl>
                  <Input
                    placeholder="e.g., Net 30"
                    {...field}
                    value={field.value ?? ""}
                  />
                </FormControl>
                <FormDescription>
                  Payment terms agreed with supplier
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="address"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Address</FormLabel>
              <FormControl>
                <Input
                  placeholder="e.g., 123 Business Park, Dublin"
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
                  Inactive suppliers won&apos;t appear in product forms
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
              "Update Supplier"
            ) : (
              "Create Supplier"
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

