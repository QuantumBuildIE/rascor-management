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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useCollectStockOrder } from "@/lib/api/stock/use-stock-orders";
import { useLocations } from "@/lib/api/stock/use-locations";
import type { StockOrder } from "@/types/stock";
import { toast } from "sonner";

const collectSchema = z.object({
  warehouseLocationId: z.string().min(1, "Warehouse location is required"),
});

type CollectFormValues = z.infer<typeof collectSchema>;

interface CollectOrderDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  order: StockOrder | null;
}

export function CollectOrderDialog({
  open,
  onOpenChange,
  order,
}: CollectOrderDialogProps) {
  const collectOrder = useCollectStockOrder();
  const { data: locations } = useLocations();

  const warehouseLocations = React.useMemo(
    () => locations?.filter((l) => l.locationType === "Warehouse" && l.isActive) ?? [],
    [locations]
  );

  const form = useForm<CollectFormValues>({
    resolver: zodResolver(collectSchema),
    defaultValues: {
      warehouseLocationId: "",
    },
  });

  React.useEffect(() => {
    if (open) {
      form.reset({
        warehouseLocationId: warehouseLocations[0]?.id ?? "",
      });
    }
  }, [open, form, warehouseLocations]);

  const onSubmit = async (values: CollectFormValues) => {
    if (!order) return;

    try {
      await collectOrder.mutateAsync({
        id: order.id,
        warehouseLocationId: values.warehouseLocationId,
      });
      toast.success("Order marked as collected");
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to collect order", { description: message });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Collect Order</DialogTitle>
          <DialogDescription>
            Mark order {order?.orderNumber} as collected by {order?.siteName}
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="warehouseLocationId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Warehouse Location *</FormLabel>
                  <Select
                    onValueChange={field.onChange}
                    value={field.value}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Select warehouse" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {warehouseLocations.map((location) => (
                        <SelectItem key={location.id} value={location.id}>
                          {location.locationName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
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
              <Button type="submit" disabled={collectOrder.isPending}>
                {collectOrder.isPending ? "Processing..." : "Mark as Collected"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
