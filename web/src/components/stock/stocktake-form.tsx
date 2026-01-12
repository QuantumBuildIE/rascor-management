"use client";

import * as React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
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
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { LocationSearchSelect } from "@/components/stock/location-search-select";
import { useCreateStocktake } from "@/lib/api/stock/use-stocktakes";
import { useAuth } from "@/lib/auth/use-auth";
import type { Stocktake, StockLocation } from "@/types/stock";
import { toast } from "sonner";

const stocktakeFormSchema = z.object({
  locationId: z.string().min(1, "Location is required"),
  locationName: z.string(),
  countedBy: z.string().min(1, "Counted by is required"),
  notes: z.string().optional(),
});

type StocktakeFormValues = z.infer<typeof stocktakeFormSchema>;

interface StocktakeFormProps {
  onSuccess?: (stocktake: Stocktake) => void;
  onCancel?: () => void;
}

export function StocktakeForm({ onSuccess, onCancel }: StocktakeFormProps) {
  const { user } = useAuth();
  const createStocktake = useCreateStocktake();

  const defaultCountedBy = user ? `${user.firstName} ${user.lastName}` : "";

  const form = useForm<StocktakeFormValues>({
    resolver: zodResolver(stocktakeFormSchema),
    defaultValues: {
      locationId: "",
      locationName: "",
      countedBy: defaultCountedBy,
      notes: "",
    },
  });

  const isSubmitting = createStocktake.isPending;

  const handleLocationChange = (
    locationId: string,
    location: StockLocation | undefined
  ) => {
    if (location) {
      form.setValue("locationId", location.id);
      form.setValue("locationName", location.locationName);
    }
  };

  async function onSubmit(values: StocktakeFormValues) {
    try {
      const result = await createStocktake.mutateAsync({
        locationId: values.locationId,
        countedBy: values.countedBy,
        notes: values.notes,
      });
      toast.success("Stocktake created successfully", {
        description: "Lines have been auto-populated with products at this location.",
      });
      onSuccess?.(result);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to create stocktake", {
        description: message,
      });
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Stocktake Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
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
                      placeholder="Select location to count..."
                    />
                  </FormControl>
                  <FormDescription>
                    The stock location to perform the count. Lines will be
                    auto-populated with all products at this location.
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="countedBy"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Counted By *</FormLabel>
                  <FormControl>
                    <Input placeholder="Name of person counting" {...field} />
                  </FormControl>
                  <FormDescription>
                    Person responsible for this stock count
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
                      placeholder="Any additional notes about this stocktake..."
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
              "Create Stocktake"
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
