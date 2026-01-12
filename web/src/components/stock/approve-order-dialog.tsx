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
import { Input } from "@/components/ui/input";
import { useApproveStockOrder } from "@/lib/api/stock/use-stock-orders";
import { useLocations } from "@/lib/api/stock/use-locations";
import type { StockOrder } from "@/types/stock";
import { toast } from "sonner";

const approveSchema = z.object({
  approvedBy: z.string().min(1, "Approved by is required"),
  warehouseLocationId: z.string().min(1, "Warehouse location is required"),
});

type ApproveFormValues = z.infer<typeof approveSchema>;

interface ApproveOrderDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  order: StockOrder | null;
}

export function ApproveOrderDialog({
  open,
  onOpenChange,
  order,
}: ApproveOrderDialogProps) {
  const approveOrder = useApproveStockOrder();
  const { data: locations } = useLocations();

  const warehouseLocations = React.useMemo(
    () => locations?.filter((l) => l.locationType === "Warehouse" && l.isActive) ?? [],
    [locations]
  );

  const form = useForm<ApproveFormValues>({
    resolver: zodResolver(approveSchema),
    defaultValues: {
      approvedBy: "",
      warehouseLocationId: "",
    },
  });

  React.useEffect(() => {
    if (open) {
      form.reset({
        approvedBy: "",
        warehouseLocationId: warehouseLocations[0]?.id ?? "",
      });
    }
  }, [open, form, warehouseLocations]);

  const onSubmit = async (values: ApproveFormValues) => {
    if (!order) return;

    try {
      await approveOrder.mutateAsync({
        id: order.id,
        approvedBy: values.approvedBy,
        warehouseLocationId: values.warehouseLocationId,
      });
      toast.success("Order approved successfully");
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to approve order", { description: message });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Approve Order</DialogTitle>
          <DialogDescription>
            Approve order {order?.orderNumber} for {order?.siteName}
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="approvedBy"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Approved By *</FormLabel>
                  <FormControl>
                    <Input placeholder="Your name" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

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
              <Button type="submit" disabled={approveOrder.isPending}>
                {approveOrder.isPending ? "Approving..." : "Approve Order"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
