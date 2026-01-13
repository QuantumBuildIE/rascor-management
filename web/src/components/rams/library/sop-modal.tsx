"use client";

import * as React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Separator } from "@/components/ui/separator";
import {
  useCreateSopLibraryItem,
  useUpdateSopLibraryItem,
} from "@/lib/api/rams";
import { SopReferenceDto } from "@/types/rams";
import { toast } from "sonner";

const sopSchema = z.object({
  sopId: z.string().min(1, "SOP ID is required"),
  topic: z.string().min(1, "Topic is required"),
  description: z.string().optional(),
  taskKeywords: z.string().optional(),
  policySnippet: z.string().optional(),
  procedureDetails: z.string().optional(),
  applicableLegislation: z.string().optional(),
  documentUrl: z.string().url("Please enter a valid URL").optional().or(z.literal("")),
  sortOrder: z.coerce.number().min(0),
  isActive: z.boolean(),
});

type SopFormData = z.infer<typeof sopSchema>;

interface SopModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  sop?: SopReferenceDto | null;
}

export function SopModal({ open, onOpenChange, sop }: SopModalProps) {
  const isEditing = !!sop;

  const createSop = useCreateSopLibraryItem();
  const updateSop = useUpdateSopLibraryItem();

  const form = useForm<SopFormData>({
    resolver: zodResolver(sopSchema) as any,
    defaultValues: {
      sopId: "",
      topic: "",
      description: "",
      taskKeywords: "",
      policySnippet: "",
      procedureDetails: "",
      applicableLegislation: "",
      documentUrl: "",
      sortOrder: 0,
      isActive: true,
    },
  });

  // Reset form when dialog opens/closes or sop changes
  React.useEffect(() => {
    if (open) {
      if (sop) {
        form.reset({
          sopId: sop.sopId,
          topic: sop.topic,
          description: sop.description ?? "",
          taskKeywords: sop.taskKeywords ?? "",
          policySnippet: sop.policySnippet ?? "",
          procedureDetails: sop.procedureDetails ?? "",
          applicableLegislation: sop.applicableLegislation ?? "",
          documentUrl: sop.documentUrl ?? "",
          sortOrder: sop.sortOrder,
          isActive: sop.isActive,
        });
      } else {
        form.reset({
          sopId: "",
          topic: "",
          description: "",
          taskKeywords: "",
          policySnippet: "",
          procedureDetails: "",
          applicableLegislation: "",
          documentUrl: "",
          sortOrder: 0,
          isActive: true,
        });
      }
    }
  }, [open, sop, form]);

  const isLoading = createSop.isPending || updateSop.isPending;

  const onSubmit = async (data: SopFormData) => {
    try {
      const payload = {
        sopId: data.sopId,
        topic: data.topic,
        description: data.description || undefined,
        taskKeywords: data.taskKeywords || undefined,
        policySnippet: data.policySnippet || undefined,
        procedureDetails: data.procedureDetails || undefined,
        applicableLegislation: data.applicableLegislation || undefined,
        documentUrl: data.documentUrl || undefined,
        sortOrder: data.sortOrder,
        isActive: data.isActive,
      };

      if (isEditing && sop) {
        await updateSop.mutateAsync({
          id: sop.id,
          data: payload,
        });
        toast.success("SOP updated successfully");
      } else {
        await createSop.mutateAsync(payload);
        toast.success("SOP created successfully");
      }
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error(isEditing ? "Failed to update SOP" : "Failed to create SOP", {
        description: message,
      });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{isEditing ? "Edit" : "Add"} Standard Operating Procedure</DialogTitle>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
            {/* SOP ID and Topic */}
            <div className="grid gap-4 sm:grid-cols-3">
              <FormField
                control={form.control}
                name="sopId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>SOP ID *</FormLabel>
                    <FormControl>
                      <Input placeholder="e.g., SOP-001" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="topic"
                render={({ field }) => (
                  <FormItem className="sm:col-span-2">
                    <FormLabel>Topic *</FormLabel>
                    <FormControl>
                      <Input placeholder="SOP topic" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* Description */}
            <FormField
              control={form.control}
              name="description"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Description</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Brief description of the SOP..."
                      className="min-h-[80px]"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Task Keywords */}
            <FormField
              control={form.control}
              name="taskKeywords"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Task Keywords</FormLabel>
                  <FormControl>
                    <Input placeholder="Comma-separated keywords for matching tasks" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <Separator />

            {/* Policy Snippet */}
            <FormField
              control={form.control}
              name="policySnippet"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Policy Snippet</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Key policy points..."
                      className="min-h-[80px]"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Procedure Details */}
            <FormField
              control={form.control}
              name="procedureDetails"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Procedure Details</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Detailed procedure steps..."
                      className="min-h-[100px]"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <Separator />

            {/* Applicable Legislation and Document URL */}
            <div className="grid gap-4 sm:grid-cols-2">
              <FormField
                control={form.control}
                name="applicableLegislation"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Applicable Legislation</FormLabel>
                    <FormControl>
                      <Input placeholder="Related legislation references" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="documentUrl"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Document URL</FormLabel>
                    <FormControl>
                      <Input placeholder="https://..." {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <Separator />

            {/* Sort Order and Active */}
            <div className="grid gap-4 sm:grid-cols-2">
              <FormField
                control={form.control}
                name="sortOrder"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Sort Order</FormLabel>
                    <FormControl>
                      <Input type="number" min={0} {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              {isEditing && (
                <FormField
                  control={form.control}
                  name="isActive"
                  render={({ field }) => (
                    <FormItem className="flex flex-row items-end space-x-2 space-y-0 pb-2">
                      <FormControl>
                        <Checkbox
                          checked={field.value}
                          onCheckedChange={field.onChange}
                        />
                      </FormControl>
                      <FormLabel className="font-normal cursor-pointer">
                        Active
                      </FormLabel>
                    </FormItem>
                  )}
                />
              )}
            </div>

            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
                Cancel
              </Button>
              <Button type="submit" disabled={isLoading}>
                {isLoading && (
                  <span className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-primary-foreground border-r-transparent" />
                )}
                {isEditing ? "Save Changes" : "Add SOP"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}

