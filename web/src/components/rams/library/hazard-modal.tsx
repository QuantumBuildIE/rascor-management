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
  useCreateHazardLibraryItem,
  useUpdateHazardLibraryItem,
} from "@/lib/api/rams";
import {
  HazardLibraryDto,
  HazardCategory,
  HazardCategoryLabels,
} from "@/types/rams";
import { toast } from "sonner";

const hazardSchema = z.object({
  code: z.string().min(1, "Code is required"),
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
  category: z.string().min(1, "Category is required"),
  keywords: z.string().optional(),
  defaultLikelihood: z.coerce.number().min(1).max(5),
  defaultSeverity: z.coerce.number().min(1).max(5),
  typicalWhoAtRisk: z.string().optional(),
  sortOrder: z.coerce.number().min(0),
  isActive: z.boolean(),
});

type HazardFormData = z.infer<typeof hazardSchema>;

interface HazardModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  hazard?: HazardLibraryDto | null;
}

const likelihoodOptions = [
  { value: "1", label: "1 - Very Unlikely" },
  { value: "2", label: "2 - Unlikely" },
  { value: "3", label: "3 - Possible" },
  { value: "4", label: "4 - Likely" },
  { value: "5", label: "5 - Very Likely" },
];

const severityOptions = [
  { value: "1", label: "1 - Insignificant" },
  { value: "2", label: "2 - Minor" },
  { value: "3", label: "3 - Moderate" },
  { value: "4", label: "4 - Major" },
  { value: "5", label: "5 - Catastrophic" },
];

export function HazardModal({ open, onOpenChange, hazard }: HazardModalProps) {
  const isEditing = !!hazard;

  const createHazard = useCreateHazardLibraryItem();
  const updateHazard = useUpdateHazardLibraryItem();

  const form = useForm<HazardFormData>({
    resolver: zodResolver(hazardSchema),
    defaultValues: {
      code: "",
      name: "",
      description: "",
      category: "",
      keywords: "",
      defaultLikelihood: 3,
      defaultSeverity: 3,
      typicalWhoAtRisk: "",
      sortOrder: 0,
      isActive: true,
    },
  });

  // Reset form when dialog opens/closes or hazard changes
  React.useEffect(() => {
    if (open) {
      if (hazard) {
        form.reset({
          code: hazard.code,
          name: hazard.name,
          description: hazard.description ?? "",
          category: String(hazard.category),
          keywords: hazard.keywords ?? "",
          defaultLikelihood: hazard.defaultLikelihood,
          defaultSeverity: hazard.defaultSeverity,
          typicalWhoAtRisk: hazard.typicalWhoAtRisk ?? "",
          sortOrder: hazard.sortOrder,
          isActive: hazard.isActive,
        });
      } else {
        form.reset({
          code: "",
          name: "",
          description: "",
          category: "",
          keywords: "",
          defaultLikelihood: 3,
          defaultSeverity: 3,
          typicalWhoAtRisk: "",
          sortOrder: 0,
          isActive: true,
        });
      }
    }
  }, [open, hazard, form]);

  const isLoading = createHazard.isPending || updateHazard.isPending;

  const onSubmit = async (data: HazardFormData) => {
    try {
      const payload = {
        code: data.code,
        name: data.name,
        description: data.description || undefined,
        category: parseInt(data.category) as HazardCategory,
        keywords: data.keywords || undefined,
        defaultLikelihood: data.defaultLikelihood,
        defaultSeverity: data.defaultSeverity,
        typicalWhoAtRisk: data.typicalWhoAtRisk || undefined,
        sortOrder: data.sortOrder,
        isActive: data.isActive,
      };

      if (isEditing && hazard) {
        await updateHazard.mutateAsync({
          id: hazard.id,
          data: payload,
        });
        toast.success("Hazard updated successfully");
      } else {
        await createHazard.mutateAsync(payload);
        toast.success("Hazard created successfully");
      }
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error(isEditing ? "Failed to update hazard" : "Failed to create hazard", {
        description: message,
      });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{isEditing ? "Edit" : "Add"} Hazard</DialogTitle>
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
                      <Input placeholder="e.g., HAZ-001" {...field} />
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
                      <Input placeholder="Hazard name" {...field} />
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
                      placeholder="Describe the hazard..."
                      className="min-h-[80px]"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Category and Keywords */}
            <div className="grid gap-4 sm:grid-cols-2">
              <FormField
                control={form.control}
                name="category"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Category *</FormLabel>
                    <Select onValueChange={field.onChange} value={field.value}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select category..." />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
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
              <FormField
                control={form.control}
                name="keywords"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Keywords</FormLabel>
                    <FormControl>
                      <Input placeholder="Comma-separated keywords for search/AI" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <Separator />

            {/* Default Risk */}
            <div>
              <h4 className="text-sm font-medium mb-3">Default Risk Rating</h4>
              <div className="grid gap-4 sm:grid-cols-3">
                <FormField
                  control={form.control}
                  name="defaultLikelihood"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Likelihood (1-5)</FormLabel>
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
                          {likelihoodOptions.map((option) => (
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
                  name="defaultSeverity"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Severity (1-5)</FormLabel>
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
                          {severityOptions.map((option) => (
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
                  name="typicalWhoAtRisk"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Typical Who at Risk</FormLabel>
                      <FormControl>
                        <Input placeholder="e.g., Employees, Contractors" {...field} />
                      </FormControl>
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
                {isEditing ? "Save Changes" : "Add Hazard"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
