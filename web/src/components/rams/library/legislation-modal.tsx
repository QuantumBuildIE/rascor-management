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
  useCreateLegislationLibraryItem,
  useUpdateLegislationLibraryItem,
} from "@/lib/api/rams";
import { LegislationReferenceDto } from "@/types/rams";
import { toast } from "sonner";

const legislationSchema = z.object({
  code: z.string().min(1, "Code is required"),
  name: z.string().min(1, "Name is required"),
  shortName: z.string().optional(),
  description: z.string().optional(),
  jurisdiction: z.string().optional(),
  keywords: z.string().optional(),
  documentUrl: z.string().url("Please enter a valid URL").optional().or(z.literal("")),
  applicableCategories: z.string().optional(),
  sortOrder: z.coerce.number().min(0),
  isActive: z.boolean(),
});

type LegislationFormData = z.infer<typeof legislationSchema>;

interface LegislationModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  legislation?: LegislationReferenceDto | null;
}

export function LegislationModal({ open, onOpenChange, legislation }: LegislationModalProps) {
  const isEditing = !!legislation;

  const createLegislation = useCreateLegislationLibraryItem();
  const updateLegislation = useUpdateLegislationLibraryItem();

  const form = useForm<LegislationFormData>({
    resolver: zodResolver(legislationSchema) as any,
    defaultValues: {
      code: "",
      name: "",
      shortName: "",
      description: "",
      jurisdiction: "",
      keywords: "",
      documentUrl: "",
      applicableCategories: "",
      sortOrder: 0,
      isActive: true,
    },
  });

  // Reset form when dialog opens/closes or legislation changes
  React.useEffect(() => {
    if (open) {
      if (legislation) {
        form.reset({
          code: legislation.code,
          name: legislation.name,
          shortName: legislation.shortName ?? "",
          description: legislation.description ?? "",
          jurisdiction: legislation.jurisdiction ?? "",
          keywords: legislation.keywords ?? "",
          documentUrl: legislation.documentUrl ?? "",
          applicableCategories: legislation.applicableCategories ?? "",
          sortOrder: legislation.sortOrder,
          isActive: legislation.isActive,
        });
      } else {
        form.reset({
          code: "",
          name: "",
          shortName: "",
          description: "",
          jurisdiction: "",
          keywords: "",
          documentUrl: "",
          applicableCategories: "",
          sortOrder: 0,
          isActive: true,
        });
      }
    }
  }, [open, legislation, form]);

  const isLoading = createLegislation.isPending || updateLegislation.isPending;

  const onSubmit = async (data: LegislationFormData) => {
    try {
      const payload = {
        code: data.code,
        name: data.name,
        shortName: data.shortName || undefined,
        description: data.description || undefined,
        jurisdiction: data.jurisdiction || undefined,
        keywords: data.keywords || undefined,
        documentUrl: data.documentUrl || undefined,
        applicableCategories: data.applicableCategories || undefined,
        sortOrder: data.sortOrder,
        isActive: data.isActive,
      };

      if (isEditing && legislation) {
        await updateLegislation.mutateAsync({
          id: legislation.id,
          data: payload,
        });
        toast.success("Legislation updated successfully");
      } else {
        await createLegislation.mutateAsync(payload);
        toast.success("Legislation created successfully");
      }
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error(isEditing ? "Failed to update legislation" : "Failed to create legislation", {
        description: message,
      });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{isEditing ? "Edit" : "Add"} Legislation Reference</DialogTitle>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
            {/* Code and Name */}
            <div className="grid gap-4 sm:grid-cols-3">
              <FormField
                control={form.control}
                name="code"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Code *</FormLabel>
                    <FormControl>
                      <Input placeholder="e.g., LEG-001" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="name"
                render={({ field }) => (
                  <FormItem className="sm:col-span-2">
                    <FormLabel>Name *</FormLabel>
                    <FormControl>
                      <Input placeholder="Full legislation name" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* Short Name and Jurisdiction */}
            <div className="grid gap-4 sm:grid-cols-2">
              <FormField
                control={form.control}
                name="shortName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Short Name</FormLabel>
                    <FormControl>
                      <Input placeholder="e.g., COSHH Regs" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="jurisdiction"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Jurisdiction</FormLabel>
                    <FormControl>
                      <Input placeholder="e.g., Ireland, UK, EU" {...field} />
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
                      placeholder="Brief description of the legislation..."
                      className="min-h-[80px]"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <Separator />

            {/* Keywords and Document URL */}
            <div className="grid gap-4 sm:grid-cols-2">
              <FormField
                control={form.control}
                name="keywords"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Keywords</FormLabel>
                    <FormControl>
                      <Input placeholder="Comma-separated keywords" {...field} />
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

            {/* Applicable Categories */}
            <FormField
              control={form.control}
              name="applicableCategories"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Applicable Categories</FormLabel>
                  <FormControl>
                    <Input placeholder="e.g., Chemical, Electrical (comma-separated)" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

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
                {isEditing ? "Save Changes" : "Add Legislation"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}

