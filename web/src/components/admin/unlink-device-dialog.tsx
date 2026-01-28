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
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { useUnlinkDevice } from "@/lib/api/admin/use-devices";
import { toast } from "sonner";
import type { AdminDevice } from "@/types/admin";

const unlinkDeviceSchema = z.object({
  reason: z.string().min(1, "Please provide a reason for unlinking"),
});

type UnlinkDeviceFormValues = z.infer<typeof unlinkDeviceSchema>;

interface UnlinkDeviceDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  device: AdminDevice;
}

export function UnlinkDeviceDialog({
  open,
  onOpenChange,
  device,
}: UnlinkDeviceDialogProps) {
  const unlinkDevice = useUnlinkDevice();

  const form = useForm<UnlinkDeviceFormValues>({
    resolver: zodResolver(unlinkDeviceSchema) as any,
    defaultValues: {
      reason: "",
    },
  });

  // Reset form when dialog closes
  React.useEffect(() => {
    if (!open) {
      form.reset();
    }
  }, [open, form]);

  async function onSubmit(values: UnlinkDeviceFormValues) {
    try {
      await unlinkDevice.mutateAsync({
        deviceId: device.id,
        data: { reason: values.reason },
      });
      toast.success("Device unlinked successfully", {
        description: `${device.deviceIdentifier} has been unlinked from ${device.employeeName}`,
      });
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to unlink device", {
        description: message,
      });
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Unlink Device</DialogTitle>
          <DialogDescription>
            Are you sure you want to unlink {device.deviceIdentifier} from{" "}
            {device.employeeName}?
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <div className="text-sm text-muted-foreground">
              The device will continue to function but attendance records
              won&apos;t be associated with any employee.
            </div>

            <FormField
              control={form.control}
              name="reason"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Reason (required)</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="e.g., Employee left company"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <DialogFooter className="pt-4">
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
                disabled={unlinkDevice.isPending}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                variant="destructive"
                disabled={unlinkDevice.isPending}
              >
                {unlinkDevice.isPending ? (
                  <>
                    <LoadingSpinner className="mr-2 h-4 w-4" />
                    Unlinking...
                  </>
                ) : (
                  "Unlink Device"
                )}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
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
