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
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useRejectStockOrder } from "@/lib/api/stock/use-stock-orders";
import type { StockOrder } from "@/types/stock";
import { toast } from "sonner";

const rejectSchema = z.object({
  rejectedBy: z.string().min(1, "Rejected by is required"),
  reason: z.string().min(1, "Reason is required"),
});

type RejectFormValues = z.infer<typeof rejectSchema>;

interface RejectOrderDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  order: StockOrder | null;
}

export function RejectOrderDialog({
  open,
  onOpenChange,
  order,
}: RejectOrderDialogProps) {
  const rejectOrder = useRejectStockOrder();

  const form = useForm<RejectFormValues>({
    resolver: zodResolver(rejectSchema) as any,
    defaultValues: {
      rejectedBy: "",
      reason: "",
    },
  });

  React.useEffect(() => {
    if (open) {
      form.reset({
        rejectedBy: "",
        reason: "",
      });
    }
  }, [open, form]);

  const onSubmit = async (values: RejectFormValues) => {
    if (!order) return;

    try {
      await rejectOrder.mutateAsync({
        id: order.id,
        rejectedBy: values.rejectedBy,
        reason: values.reason,
      });
      toast.success("Order rejected");
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to reject order", { description: message });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Reject Order</DialogTitle>
          <DialogDescription>
            Reject order {order?.orderNumber} for {order?.siteName}
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="rejectedBy"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Rejected By *</FormLabel>
                  <FormControl>
                    <Input placeholder="Your name" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="reason"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Reason *</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Enter reason for rejection"
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
              <Button
                type="submit"
                variant="destructive"
                disabled={rejectOrder.isPending}
              >
                {rejectOrder.isPending ? "Rejecting..." : "Reject Order"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}

