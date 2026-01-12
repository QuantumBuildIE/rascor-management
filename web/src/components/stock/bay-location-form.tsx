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
import { LocationSearchSelect } from "./location-search-select";
import {
  useCreateBayLocation,
  useUpdateBayLocation,
} from "@/lib/api/stock/use-bay-locations";
import type { BayLocation } from "@/types/stock";
import { toast } from "sonner";

const bayLocationFormSchema = z.object({
  bayCode: z.string().min(1, "Bay code is required").max(50, "Bay code must be 50 characters or less"),
  bayName: z.string().max(200, "Bay name must be 200 characters or less").optional(),
  stockLocationId: z.string().min(1, "Stock location is required"),
  capacity: z.number().min(0, "Capacity must be 0 or greater").optional().nullable(),
  isActive: z.boolean(),
  notes: z.string().max(1000, "Notes must be 1000 characters or less").optional(),
});

type BayLocationFormValues = z.infer<typeof bayLocationFormSchema>;

interface BayLocationFormProps {
  bayLocation?: BayLocation;
  onSuccess?: () => void;
  onCancel?: () => void;
}

export function BayLocationForm({
  bayLocation,
  onSuccess,
  onCancel,
}: BayLocationFormProps) {
  const isEditing = !!bayLocation;

  const createBayLocation = useCreateBayLocation();
  const updateBayLocation = useUpdateBayLocation();

  const form = useForm<BayLocationFormValues>({
    resolver: zodResolver(bayLocationFormSchema),
    defaultValues: {
      bayCode: bayLocation?.bayCode ?? "",
      bayName: bayLocation?.bayName ?? "",
      stockLocationId: bayLocation?.stockLocationId ?? "",
      capacity: bayLocation?.capacity ?? undefined,
      isActive: bayLocation?.isActive ?? true,
      notes: bayLocation?.notes ?? "",
    },
  });

  const isSubmitting = createBayLocation.isPending || updateBayLocation.isPending;

  async function onSubmit(values: BayLocationFormValues) {
    try {
      if (isEditing) {
        await updateBayLocation.mutateAsync({
          id: bayLocation.id,
          data: {
            bayCode: values.bayCode,
            bayName: values.bayName || undefined,
            stockLocationId: values.stockLocationId,
            capacity: values.capacity ?? undefined,
            isActive: values.isActive,
            notes: values.notes || undefined,
          },
        });
        toast.success("Bay location updated successfully");
      } else {
        await createBayLocation.mutateAsync({
          bayCode: values.bayCode,
          bayName: values.bayName || undefined,
          stockLocationId: values.stockLocationId,
          capacity: values.capacity ?? undefined,
          isActive: values.isActive,
          notes: values.notes || undefined,
        });
        toast.success("Bay location created successfully");
      }
      onSuccess?.();
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "An error occurred";
      toast.error(
        isEditing ? "Failed to update bay location" : "Failed to create bay location",
        {
          description: message,
        }
      );
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <FormField
          control={form.control}
          name="bayCode"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Bay Code *</FormLabel>
              <FormControl>
                <Input placeholder="e.g., A-1" {...field} />
              </FormControl>
              <FormDescription>
                A unique code for this bay within the location (e.g., A-1, B-2)
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="bayName"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Bay Name</FormLabel>
              <FormControl>
                <Input placeholder="e.g., Aisle A, Bay 1 - Adhesives" {...field} />
              </FormControl>
              <FormDescription>
                A descriptive name for this bay location
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="stockLocationId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Stock Location *</FormLabel>
              <FormControl>
                <LocationSearchSelect
                  value={field.value}
                  onValueChange={(id) => field.onChange(id)}
                  placeholder="Select a stock location..."
                />
              </FormControl>
              <FormDescription>
                The warehouse or stock location this bay belongs to
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="capacity"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Capacity</FormLabel>
              <FormControl>
                <Input
                  type="number"
                  min="0"
                  placeholder="e.g., 100"
                  {...field}
                  value={field.value ?? ""}
                  onChange={(e) => field.onChange(e.target.value ? e.target.valueAsNumber : undefined)}
                />
              </FormControl>
              <FormDescription>
                Maximum number of items this bay can hold (optional)
              </FormDescription>
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
                  placeholder="Any additional notes about this bay location..."
                  rows={3}
                  {...field}
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
                  Inactive bay locations won&apos;t appear in selection dropdowns
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
              "Update Bay Location"
            ) : (
              "Create Bay Location"
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
