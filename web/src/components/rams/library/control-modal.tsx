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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
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
  useCreateControlLibraryItem,
  useUpdateControlLibraryItem,
} from "@/lib/api/rams";
import {
  ControlMeasureLibraryDto,
  ControlHierarchy,
  ControlHierarchyLabels,
  HazardCategory,
  HazardCategoryLabels,
} from "@/types/rams";
import { toast } from "sonner";

const controlSchema = z.object({
  code: z.string().min(1, "Code is required"),
  name: z.string().min(1, "Name is required"),
  description: z.string().min(1, "Description is required"),
  hierarchy: z.string().min(1, "Hierarchy is required"),
  applicableToCategory: z.string().optional(),
  keywords: z.string().optional(),
  typicalLikelihoodReduction: z.coerce.number().min(0).max(4),
  typicalSeverityReduction: z.coerce.number().min(0).max(4),
  sortOrder: z.coerce.number().min(0),
  isActive: z.boolean(),
});

interface ControlFormData {
  code: string;
  name: string;
  description: string;
  hierarchy: string;
  applicableToCategory?: string;
  keywords?: string;
  typicalLikelihoodReduction: number;
  typicalSeverityReduction: number;
  sortOrder: number;
  isActive: boolean;
}

interface ControlModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  control?: ControlMeasureLibraryDto | null;
}

const reductionOptions = [
  { value: "0", label: "0 - None" },
  { value: "1", label: "1 - Minor" },
  { value: "2", label: "2 - Moderate" },
  { value: "3", label: "3 - Significant" },
  { value: "4", label: "4 - Major" },
];

export function ControlModal({ open, onOpenChange, control }: ControlModalProps) {
  const isEditing = !!control;

  const createControl = useCreateControlLibraryItem();
  const updateControl = useUpdateControlLibraryItem();

  const form = useForm<ControlFormData>({
    resolver: zodResolver(controlSchema) as any,
    defaultValues: {
      code: "",
      name: "",
      description: "",
      hierarchy: "",
      applicableToCategory: "",
      keywords: "",
      typicalLikelihoodReduction: 1,
      typicalSeverityReduction: 0,
      sortOrder: 0,
      isActive: true,
    },
  });

  // Reset form when dialog opens/closes or control changes
  React.useEffect(() => {
    if (open) {
      if (control) {
        form.reset({
          code: control.code,
          name: control.name,
          description: control.description,
          hierarchy: String(control.hierarchy),
          applicableToCategory: control.applicableToCategory !== undefined ? String(control.applicableToCategory) : "",
          keywords: control.keywords ?? "",
          typicalLikelihoodReduction: control.typicalLikelihoodReduction,
          typicalSeverityReduction: control.typicalSeverityReduction,
          sortOrder: control.sortOrder,
          isActive: control.isActive,
        });
      } else {
        form.reset({
          code: "",
          name: "",
          description: "",
          hierarchy: "",
          applicableToCategory: "",
          keywords: "",
          typicalLikelihoodReduction: 1,
          typicalSeverityReduction: 0,
          sortOrder: 0,
          isActive: true,
        });
      }
    }
  }, [open, control, form]);

  const isLoading = createControl.isPending || updateControl.isPending;

  const onSubmit = async (data: ControlFormData) => {
    try {
      const payload = {
        code: data.code,
        name: data.name,
        description: data.description,
        hierarchy: parseInt(data.hierarchy) as ControlHierarchy,
        applicableToCategory: data.applicableToCategory ? parseInt(data.applicableToCategory) as HazardCategory : undefined,
        keywords: data.keywords || undefined,
        typicalLikelihoodReduction: data.typicalLikelihoodReduction,
        typicalSeverityReduction: data.typicalSeverityReduction,
        sortOrder: data.sortOrder,
        isActive: data.isActive,
      };

      if (isEditing && control) {
        await updateControl.mutateAsync({
          id: control.id,
          data: payload,
        });
        toast.success("Control measure updated successfully");
      } else {
        await createControl.mutateAsync(payload);
        toast.success("Control measure created successfully");
      }
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error(isEditing ? "Failed to update control measure" : "Failed to create control measure", {
        description: message,
      });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{isEditing ? "Edit" : "Add"} Control Measure</DialogTitle>
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
                      <Input placeholder="e.g., CTL-001" {...field} />
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
                      <Input placeholder="Control measure name" {...field} />
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
                  <FormLabel>Description *</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Describe the control measure..."
                      className="min-h-[100px]"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Hierarchy and Applicable Category */}
            <div className="grid gap-4 sm:grid-cols-2">
              <FormField
                control={form.control}
                name="hierarchy"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Hierarchy *</FormLabel>
                    <Select onValueChange={field.onChange} value={field.value}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select hierarchy..." />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {Object.entries(ControlHierarchyLabels).map(([value, label]) => (
                          <SelectItem key={value} value={value}>
                            {label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="applicableToCategory"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Applicable To Category</FormLabel>
                    <Select
                      onValueChange={(value) => field.onChange(value === "__all__" ? "" : value)}
                      value={field.value || "__all__"}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="All Categories" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        <SelectItem value="__all__">All Categories</SelectItem>
                        {Object.entries(HazardCategoryLabels).map(([value, label]) => (
                          <SelectItem key={value} value={value}>
                            {label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* Keywords */}
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

            <Separator />

            {/* Risk Reduction */}
            <div>
              <h4 className="text-sm font-medium mb-3">Typical Risk Reduction</h4>
              <div className="grid gap-4 sm:grid-cols-2">
                <FormField
                  control={form.control}
                  name="typicalLikelihoodReduction"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Likelihood Reduction</FormLabel>
                      <Select
                        onValueChange={(value) => field.onChange(parseInt(value))}
                        value={String(field.value)}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {reductionOptions.map((option) => (
                            <SelectItem key={option.value} value={option.value}>
                              {option.label}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="typicalSeverityReduction"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Severity Reduction</FormLabel>
                      <Select
                        onValueChange={(value) => field.onChange(parseInt(value))}
                        value={String(field.value)}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {reductionOptions.map((option) => (
                            <SelectItem key={option.value} value={option.value}>
                              {option.label}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>
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
                {isEditing ? "Save Changes" : "Add Control"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}


