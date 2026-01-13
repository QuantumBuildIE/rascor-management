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
import {
  useAddSection,
  useUpdateSection,
  type ProposalSection,
} from "@/lib/api/proposals";
import { toast } from "sonner";

const sectionSchema = z.object({
  sectionName: z.string().min(1, "Section name is required"),
  description: z.string().optional(),
  sortOrder: z.number().min(0, "Sort order must be 0 or greater"),
});

type SectionFormValues = z.infer<typeof sectionSchema>;

interface SectionDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  proposalId: string;
  section?: ProposalSection | null;
  nextSortOrder?: number;
}

export function SectionDialog({
  open,
  onOpenChange,
  proposalId,
  section,
  nextSortOrder = 0,
}: SectionDialogProps) {
  const addSection = useAddSection();
  const updateSection = useUpdateSection();
  const isEditing = !!section;

  const form = useForm<SectionFormValues>({
    resolver: zodResolver(sectionSchema) as any,
    defaultValues: {
      sectionName: "",
      description: "",
      sortOrder: nextSortOrder,
    },
  });

  React.useEffect(() => {
    if (open) {
      if (section) {
        form.reset({
          sectionName: section.sectionName,
          description: section.description ?? "",
          sortOrder: section.sortOrder,
        });
      } else {
        form.reset({
          sectionName: "",
          description: "",
          sortOrder: nextSortOrder,
        });
      }
    }
  }, [open, section, nextSortOrder, form]);

  const onSubmit = async (values: SectionFormValues) => {
    try {
      if (isEditing && section) {
        await updateSection.mutateAsync({
          sectionId: section.id,
          data: {
            sectionName: values.sectionName,
            description: values.description,
            sortOrder: values.sortOrder,
          },
        });
        toast.success("Section updated successfully");
      } else {
        await addSection.mutateAsync({
          proposalId,
          sectionName: values.sectionName,
          description: values.description,
          sortOrder: values.sortOrder,
        });
        toast.success("Section added successfully");
      }
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error(isEditing ? "Failed to update section" : "Failed to add section", {
        description: message,
      });
    }
  };

  const isPending = addSection.isPending || updateSection.isPending;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{isEditing ? "Edit Section" : "Add Section"}</DialogTitle>
          <DialogDescription>
            {isEditing
              ? "Update the section details"
              : "Add a new section to the proposal"}
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="sectionName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Section Name *</FormLabel>
                  <FormControl>
                    <Input placeholder="e.g., Labour, Materials, Equipment" {...field} />
                  </FormControl>
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
                      placeholder="Optional description for this section..."
                      className="resize-none"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="sortOrder"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Sort Order</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      min={0}
                      value={field.value}
                      onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
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
              <Button type="submit" disabled={isPending}>
                {isPending ? "Saving..." : isEditing ? "Update Section" : "Add Section"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}

