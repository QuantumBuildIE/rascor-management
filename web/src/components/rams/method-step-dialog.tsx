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
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import {
  useCreateMethodStep,
  useUpdateMethodStep,
  type MethodStepDto,
  type RiskAssessmentDto,
} from "@/lib/api/rams";
import { toast } from "sonner";

const methodStepSchema = z.object({
  stepTitle: z.string().min(1, "Step title is required"),
  detailedProcedure: z.string().optional(),
  linkedRiskAssessmentId: z.string().optional(),
  requiredPermits: z.string().optional(),
  requiresSignoff: z.boolean(),
});

type MethodStepFormData = z.infer<typeof methodStepSchema>;

interface MethodStepDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  ramsDocumentId: string;
  methodStep?: MethodStepDto | null;
  riskAssessments?: RiskAssessmentDto[];
  nextStepNumber?: number;
}

export function MethodStepDialog({
  open,
  onOpenChange,
  ramsDocumentId,
  methodStep,
  riskAssessments = [],
  nextStepNumber = 1,
}: MethodStepDialogProps) {
  const isEditing = !!methodStep;

  const createMethodStep = useCreateMethodStep();
  const updateMethodStep = useUpdateMethodStep();

  const form = useForm<MethodStepFormData>({
    resolver: zodResolver(methodStepSchema) as any,
    defaultValues: {
      stepTitle: "",
      detailedProcedure: "",
      linkedRiskAssessmentId: "",
      requiredPermits: "",
      requiresSignoff: false,
    },
  });

  // Reset form when dialog opens/closes or method step changes
  React.useEffect(() => {
    if (open) {
      if (methodStep) {
        form.reset({
          stepTitle: methodStep.stepTitle,
          detailedProcedure: methodStep.detailedProcedure ?? "",
          linkedRiskAssessmentId: methodStep.linkedRiskAssessmentId ?? "",
          requiredPermits: methodStep.requiredPermits ?? "",
          requiresSignoff: methodStep.requiresSignoff,
        });
      } else {
        form.reset({
          stepTitle: "",
          detailedProcedure: "",
          linkedRiskAssessmentId: "",
          requiredPermits: "",
          requiresSignoff: false,
        });
      }
    }
  }, [open, methodStep, form]);

  const isLoading = createMethodStep.isPending || updateMethodStep.isPending;

  const onSubmit = async (data: MethodStepFormData) => {
    try {
      if (isEditing && methodStep) {
        await updateMethodStep.mutateAsync({
          ramsDocumentId,
          id: methodStep.id,
          data: {
            stepNumber: methodStep.stepNumber,
            stepTitle: data.stepTitle,
            detailedProcedure: data.detailedProcedure || undefined,
            linkedRiskAssessmentId: data.linkedRiskAssessmentId || undefined,
            requiredPermits: data.requiredPermits || undefined,
            requiresSignoff: data.requiresSignoff,
          },
        });
        toast.success("Method step updated");
      } else {
        await createMethodStep.mutateAsync({
          ramsDocumentId,
          data: {
            stepNumber: nextStepNumber,
            stepTitle: data.stepTitle,
            detailedProcedure: data.detailedProcedure || undefined,
            linkedRiskAssessmentId: data.linkedRiskAssessmentId || undefined,
            requiredPermits: data.requiredPermits || undefined,
            requiresSignoff: data.requiresSignoff,
          },
        });
        toast.success("Method step created");
      }
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error(isEditing ? "Failed to update method step" : "Failed to create method step", {
        description: message,
      });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle>
            {isEditing ? "Edit" : "Add"} Method Step
            {!isEditing && ` (Step ${nextStepNumber})`}
          </DialogTitle>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
            <FormField
              control={form.control}
              name="stepTitle"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Step Title *</FormLabel>
                  <FormControl>
                    <Input placeholder="e.g., Site Setup and Preparation" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="detailedProcedure"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Detailed Procedure</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Describe the detailed procedure for this step..."
                      className="min-h-[150px]"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="grid gap-4 sm:grid-cols-2">
              <FormField
                control={form.control}
                name="linkedRiskAssessmentId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Link to Risk Assessment</FormLabel>
                    <Select
                      onValueChange={(value) => field.onChange(value === "__none__" ? "" : value)}
                      value={field.value || "__none__"}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select risk assessment..." />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        <SelectItem value="__none__">No linked risk assessment</SelectItem>
                        {riskAssessments.map((risk) => (
                          <SelectItem key={risk.id} value={risk.id}>
                            {risk.taskActivity}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormDescription>
                      Optionally link this step to a risk assessment
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="requiredPermits"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Required Permits</FormLabel>
                    <FormControl>
                      <Input placeholder="e.g., Hot Work Permit, Confined Space" {...field} />
                    </FormControl>
                    <FormDescription>
                      Any permits required for this step
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <FormField
              control={form.control}
              name="requiresSignoff"
              render={({ field }) => (
                <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                  <FormControl>
                    <Checkbox
                      checked={field.value}
                      onCheckedChange={field.onChange}
                    />
                  </FormControl>
                  <div className="space-y-1 leading-none">
                    <FormLabel>Requires Sign-off</FormLabel>
                    <FormDescription>
                      Check this if the step requires supervisor sign-off before proceeding
                    </FormDescription>
                  </div>
                </FormItem>
              )}
            />

            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
                Cancel
              </Button>
              <Button type="submit" disabled={isLoading}>
                {isLoading && (
                  <span className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-primary-foreground border-r-transparent" />
                )}
                {isEditing ? "Save Changes" : "Add Method Step"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}

