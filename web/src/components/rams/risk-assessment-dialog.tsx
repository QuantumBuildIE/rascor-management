"use client";

import * as React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Sparkles, Loader2, Check, X, AlertCircle } from "lucide-react";
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
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
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
  FormDescription,
} from "@/components/ui/form";
import { Separator } from "@/components/ui/separator";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Combobox, type ComboboxOption } from "@/components/ui/combobox";
import { MultiSelectCombobox, type MultiSelectOption } from "@/components/ui/multi-select-combobox";
import {
  useCreateRiskAssessment,
  useUpdateRiskAssessment,
  useHazardLibrary,
  useControlLibrary,
  useLegislationLibrary,
  useSopLibrary,
  useSuggestControls,
  useAcceptSuggestion,
  type RiskAssessmentDto,
  type ControlMeasureLibraryDto,
  type HazardLibraryDto,
  type LegislationReferenceDto,
  type SopReferenceDto,
} from "@/lib/api/rams";
import type { ControlMeasureSuggestionResponse } from "@/types/rams";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

const riskAssessmentSchema = z.object({
  taskActivity: z.string().min(1, "Task/Activity is required"),
  locationArea: z.string().optional(),
  hazardIdentified: z.string().min(1, "Hazard is required"),
  whoAtRisk: z.string().optional(),
  initialLikelihood: z.coerce.number().min(1).max(5),
  initialSeverity: z.coerce.number().min(1).max(5),
  controlMeasures: z.string().optional(),
  relevantLegislation: z.string().optional(),
  referenceSops: z.string().optional(),
  residualLikelihood: z.coerce.number().min(1).max(5),
  residualSeverity: z.coerce.number().min(1).max(5),
});

type RiskAssessmentFormData = z.infer<typeof riskAssessmentSchema>;

interface RiskAssessmentDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  ramsDocumentId: string;
  riskAssessment?: RiskAssessmentDto | null;
  nextSortOrder?: number;
  projectType?: string;
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

function getRiskLevel(rating: number): { level: string; className: string } {
  if (rating <= 4) return { level: "Low", className: "bg-green-100 text-green-800" };
  if (rating <= 12) return { level: "Medium", className: "bg-yellow-100 text-yellow-800" };
  return { level: "High", className: "bg-red-100 text-red-800" };
}

export function RiskAssessmentDialog({
  open,
  onOpenChange,
  ramsDocumentId,
  riskAssessment,
  nextSortOrder = 0,
  projectType,
}: RiskAssessmentDialogProps) {
  const isEditing = !!riskAssessment;

  // Mutations
  const createRiskAssessment = useCreateRiskAssessment();
  const updateRiskAssessment = useUpdateRiskAssessment();
  const suggestControls = useSuggestControls();
  const acceptSuggestion = useAcceptSuggestion();

  // Library queries
  const { data: hazards = [], isLoading: hazardsLoading } = useHazardLibrary({ includeInactive: false });
  const { data: controls = [], isLoading: controlsLoading } = useControlLibrary({ includeInactive: false });
  const { data: legislation = [], isLoading: legislationLoading } = useLegislationLibrary({ includeInactive: false });
  const { data: sops = [], isLoading: sopsLoading } = useSopLibrary({ includeInactive: false });

  // Local state for selected library items
  const [selectedControlIds, setSelectedControlIds] = React.useState<string[]>([]);
  const [selectedLegislationIds, setSelectedLegislationIds] = React.useState<string[]>([]);
  const [selectedSopIds, setSelectedSopIds] = React.useState<string[]>([]);

  // AI Suggestions state
  const [suggestions, setSuggestions] = React.useState<ControlMeasureSuggestionResponse | null>(null);
  const [showSuggestions, setShowSuggestions] = React.useState(false);

  const form = useForm<RiskAssessmentFormData>({
    resolver: zodResolver(riskAssessmentSchema) as any,
    defaultValues: {
      taskActivity: "",
      locationArea: "",
      hazardIdentified: "",
      whoAtRisk: "",
      initialLikelihood: 3,
      initialSeverity: 3,
      controlMeasures: "",
      relevantLegislation: "",
      referenceSops: "",
      residualLikelihood: 2,
      residualSeverity: 3,
    },
  });

  // Convert library items to combobox options
  const hazardOptions: ComboboxOption[] = React.useMemo(
    () =>
      hazards.map((h: HazardLibraryDto) => ({
        value: h.id,
        label: h.name,
        description: `[${h.categoryDisplay}] ${h.description || ""}`.trim(),
        metadata: {
          code: h.code,
          category: h.category,
          defaultLikelihood: h.defaultLikelihood,
          defaultSeverity: h.defaultSeverity,
          typicalWhoAtRisk: h.typicalWhoAtRisk,
        },
      })),
    [hazards]
  );

  const controlOptions: MultiSelectOption[] = React.useMemo(
    () =>
      controls.map((c: ControlMeasureLibraryDto) => ({
        value: c.id,
        label: c.name,
        description: `[${c.hierarchyDisplay}] ${c.description}`,
        metadata: {
          code: c.code,
          hierarchy: c.hierarchy,
          likelihoodReduction: c.typicalLikelihoodReduction,
          severityReduction: c.typicalSeverityReduction,
        },
      })),
    [controls]
  );

  const legislationOptions: MultiSelectOption[] = React.useMemo(
    () =>
      legislation.map((l: LegislationReferenceDto) => ({
        value: l.id,
        label: l.shortName || l.code,
        description: l.name,
      })),
    [legislation]
  );

  const sopOptions: MultiSelectOption[] = React.useMemo(
    () =>
      sops.map((s: SopReferenceDto) => ({
        value: s.id,
        label: s.sopId,
        description: s.topic,
      })),
    [sops]
  );

  // Reset form when dialog opens/closes or risk assessment changes
  React.useEffect(() => {
    if (open) {
      if (riskAssessment) {
        form.reset({
          taskActivity: riskAssessment.taskActivity,
          locationArea: riskAssessment.locationArea ?? "",
          hazardIdentified: riskAssessment.hazardIdentified,
          whoAtRisk: riskAssessment.whoAtRisk ?? "",
          initialLikelihood: riskAssessment.initialLikelihood,
          initialSeverity: riskAssessment.initialSeverity,
          controlMeasures: riskAssessment.controlMeasures ?? "",
          relevantLegislation: riskAssessment.relevantLegislation ?? "",
          referenceSops: riskAssessment.referenceSops ?? "",
          residualLikelihood: riskAssessment.residualLikelihood,
          residualSeverity: riskAssessment.residualSeverity,
        });
      } else {
        form.reset({
          taskActivity: "",
          locationArea: "",
          hazardIdentified: "",
          whoAtRisk: "",
          initialLikelihood: 3,
          initialSeverity: 3,
          controlMeasures: "",
          relevantLegislation: "",
          referenceSops: "",
          residualLikelihood: 2,
          residualSeverity: 3,
        });
      }
      setSelectedControlIds([]);
      setSelectedLegislationIds([]);
      setSelectedSopIds([]);
      setSuggestions(null);
      setShowSuggestions(false);
    }
  }, [open, riskAssessment, form]);

  // Watch form values for live risk calculation
  const initialLikelihood = form.watch("initialLikelihood");
  const initialSeverity = form.watch("initialSeverity");
  const residualLikelihood = form.watch("residualLikelihood");
  const residualSeverity = form.watch("residualSeverity");
  const taskActivity = form.watch("taskActivity");
  const hazardIdentified = form.watch("hazardIdentified");

  const initialRating = initialLikelihood * initialSeverity;
  const residualRating = residualLikelihood * residualSeverity;
  const initialRiskLevel = getRiskLevel(initialRating);
  const residualRiskLevel = getRiskLevel(residualRating);

  const isLoading = createRiskAssessment.isPending || updateRiskAssessment.isPending;

  // Handle hazard selection from library
  const handleHazardSelect = (value: string, option?: ComboboxOption) => {
    if (option) {
      form.setValue("hazardIdentified", option.label);
      if (option.metadata) {
        const metadata = option.metadata as {
          defaultLikelihood?: number;
          defaultSeverity?: number;
          typicalWhoAtRisk?: string;
        };
        if (metadata.defaultLikelihood) {
          form.setValue("initialLikelihood", metadata.defaultLikelihood);
        }
        if (metadata.defaultSeverity) {
          form.setValue("initialSeverity", metadata.defaultSeverity);
        }
        if (metadata.typicalWhoAtRisk && !form.getValues("whoAtRisk")) {
          form.setValue("whoAtRisk", metadata.typicalWhoAtRisk);
        }
      }
    } else {
      form.setValue("hazardIdentified", value);
    }
  };

  // Handle control measures selection
  const handleControlsChange = (values: string[], selectedOptions: MultiSelectOption[]) => {
    setSelectedControlIds(values);

    if (selectedOptions.length > 0) {
      // Build control measures text from selected controls
      const selectedControls = controls.filter((c) => values.includes(c.id));
      const controlText = selectedControls
        .map((c) => `• [${c.hierarchyDisplay}] ${c.name}: ${c.description}`)
        .join("\n");
      form.setValue("controlMeasures", controlText);

      // Calculate risk reduction based on selected controls
      const totalLikelihoodReduction = selectedControls.reduce(
        (sum, c) => sum + c.typicalLikelihoodReduction,
        0
      );
      const totalSeverityReduction = selectedControls.reduce(
        (sum, c) => sum + c.typicalSeverityReduction,
        0
      );

      const newResidualL = Math.max(1, initialLikelihood - totalLikelihoodReduction);
      const newResidualS = Math.max(1, initialSeverity - totalSeverityReduction);

      form.setValue("residualLikelihood", newResidualL);
      form.setValue("residualSeverity", newResidualS);
    }
  };

  // Handle legislation selection
  const handleLegislationChange = (values: string[], selectedOptions: MultiSelectOption[]) => {
    setSelectedLegislationIds(values);
    if (selectedOptions.length > 0) {
      const legText = selectedOptions.map((o) => o.label).join(", ");
      form.setValue("relevantLegislation", legText);
    }
  };

  // Handle SOPs selection
  const handleSopsChange = (values: string[], selectedOptions: MultiSelectOption[]) => {
    setSelectedSopIds(values);
    if (selectedOptions.length > 0) {
      const sopText = selectedOptions.map((o) => o.label).join(", ");
      form.setValue("referenceSops", sopText);
    }
  };

  // Get AI suggestions
  const handleGetSuggestions = async () => {
    if (!taskActivity || !hazardIdentified) {
      toast.error("Please enter Task/Activity and Hazard before requesting suggestions");
      return;
    }

    try {
      const result = await suggestControls.mutateAsync({
        request: {
          taskActivity,
          hazardIdentified,
          locationArea: form.getValues("locationArea") || undefined,
          whoAtRisk: form.getValues("whoAtRisk") || undefined,
          initialLikelihood: Number(form.getValues("initialLikelihood")),
          initialSeverity: Number(form.getValues("initialSeverity")),
          projectType,
        },
        ramsDocumentId,
        riskAssessmentId: riskAssessment?.id,
      });

      setSuggestions(result);
      setShowSuggestions(true);

      if (!result.success) {
        toast.error(result.errorMessage || "Failed to get suggestions");
      }
    } catch (error) {
      toast.error("Failed to get AI suggestions");
      console.error(error);
    }
  };

  // Apply AI suggestions
  const handleApplySuggestions = async () => {
    if (!suggestions) return;

    // Apply AI-generated control measures or use library matches
    if (suggestions.aiGeneratedControlMeasures) {
      form.setValue("controlMeasures", suggestions.aiGeneratedControlMeasures);
      setSelectedControlIds([]);
    } else if (suggestions.suggestedControls.length > 0) {
      // Select matching controls from library
      const matchingIds = suggestions.suggestedControls
        .map((sc) => controls.find((c) => c.code === sc.code || c.name === sc.name)?.id)
        .filter(Boolean) as string[];

      if (matchingIds.length > 0) {
        setSelectedControlIds(matchingIds);
        const selectedControls = controls.filter((c) => matchingIds.includes(c.id));
        const controlText = selectedControls
          .map((c) => `• [${c.hierarchyDisplay}] ${c.name}: ${c.description}`)
          .join("\n");
        form.setValue("controlMeasures", controlText);
      }
    }

    // Apply legislation
    if (suggestions.aiGeneratedLegislation) {
      form.setValue("relevantLegislation", suggestions.aiGeneratedLegislation);
    } else if (suggestions.relevantLegislation.length > 0) {
      const matchingIds = suggestions.relevantLegislation
        .map((sl) => legislation.find((l) => l.code === sl.code)?.id)
        .filter(Boolean) as string[];

      if (matchingIds.length > 0) {
        setSelectedLegislationIds(matchingIds);
        const selectedLeg = legislation.filter((l) => matchingIds.includes(l.id));
        const legText = selectedLeg.map((l) => l.shortName || l.code).join(", ");
        form.setValue("relevantLegislation", legText);
      }
    }

    // Apply SOPs
    if (suggestions.relevantSops.length > 0) {
      const matchingIds = suggestions.relevantSops
        .map((ss) => sops.find((s) => s.sopId === ss.sopId)?.id)
        .filter(Boolean) as string[];

      if (matchingIds.length > 0) {
        setSelectedSopIds(matchingIds);
        const selectedS = sops.filter((s) => matchingIds.includes(s.id));
        const sopText = selectedS.map((s) => s.sopId).join(", ");
        form.setValue("referenceSops", sopText);
      }
    }

    // Apply suggested residual risk
    if (suggestions.suggestedResidualLikelihood) {
      form.setValue("residualLikelihood", suggestions.suggestedResidualLikelihood);
    }
    if (suggestions.suggestedResidualSeverity) {
      form.setValue("residualSeverity", suggestions.suggestedResidualSeverity);
    }

    // Mark as accepted
    if (suggestions.auditLogId) {
      await acceptSuggestion.mutateAsync({ auditLogId: suggestions.auditLogId, accepted: true });
    }

    setShowSuggestions(false);
    toast.success("Suggestions applied");
  };

  // Dismiss suggestions
  const handleDismissSuggestions = async () => {
    if (suggestions?.auditLogId) {
      await acceptSuggestion.mutateAsync({ auditLogId: suggestions.auditLogId, accepted: false });
    }
    setShowSuggestions(false);
  };

  const onSubmit = async (data: RiskAssessmentFormData) => {
    try {
      if (isEditing && riskAssessment) {
        await updateRiskAssessment.mutateAsync({
          ramsDocumentId,
          id: riskAssessment.id,
          data: {
            taskActivity: data.taskActivity,
            locationArea: data.locationArea || undefined,
            hazardIdentified: data.hazardIdentified,
            whoAtRisk: data.whoAtRisk || undefined,
            initialLikelihood: data.initialLikelihood,
            initialSeverity: data.initialSeverity,
            controlMeasures: data.controlMeasures || undefined,
            relevantLegislation: data.relevantLegislation || undefined,
            referenceSops: data.referenceSops || undefined,
            residualLikelihood: data.residualLikelihood,
            residualSeverity: data.residualSeverity,
            sortOrder: riskAssessment.sortOrder,
          },
        });
        toast.success("Risk assessment updated");
      } else {
        await createRiskAssessment.mutateAsync({
          ramsDocumentId,
          data: {
            taskActivity: data.taskActivity,
            locationArea: data.locationArea || undefined,
            hazardIdentified: data.hazardIdentified,
            whoAtRisk: data.whoAtRisk || undefined,
            initialLikelihood: data.initialLikelihood,
            initialSeverity: data.initialSeverity,
            controlMeasures: data.controlMeasures || undefined,
            relevantLegislation: data.relevantLegislation || undefined,
            referenceSops: data.referenceSops || undefined,
            residualLikelihood: data.residualLikelihood,
            residualSeverity: data.residualSeverity,
            sortOrder: nextSortOrder,
          },
        });
        toast.success("Risk assessment created");
      }
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error(isEditing ? "Failed to update risk assessment" : "Failed to create risk assessment", {
        description: message,
      });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{isEditing ? "Edit" : "Add"} Risk Assessment</DialogTitle>
        </DialogHeader>

        <div>
          {/* Main Form */}
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
              {/* Task and Location */}
              <div className="grid gap-4 sm:grid-cols-2">
                <FormField
                  control={form.control}
                  name="taskActivity"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Task/Activity *</FormLabel>
                      <FormControl>
                        <Input placeholder="e.g., Drilling concrete walls" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="locationArea"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Location/Area</FormLabel>
                      <FormControl>
                        <Input placeholder="e.g., Basement Level 1" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              {/* Hazard and Who at Risk */}
              <div className="grid gap-4 sm:grid-cols-2">
                <FormField
                  control={form.control}
                  name="hazardIdentified"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Hazard Identified *</FormLabel>
                      <FormDescription className="text-xs">
                        Select from library or type custom
                      </FormDescription>
                      <FormControl>
                        <Combobox
                          options={hazardOptions}
                          value={field.value}
                          onValueChange={handleHazardSelect}
                          placeholder="Search hazards..."
                          searchPlaceholder="Search hazards..."
                          isLoading={hazardsLoading}
                          allowCustomValue={true}
                          renderOption={(option) => (
                            <div>
                              <div className="font-medium">{option.label}</div>
                              <div className="text-xs text-muted-foreground">
                                {option.description}
                              </div>
                              {option.metadata && (
                                <div className="flex gap-1 mt-1">
                                  <Badge variant="outline" className="text-[10px] px-1 py-0">
                                    L{(option.metadata as { defaultLikelihood?: number }).defaultLikelihood}
                                  </Badge>
                                  <Badge variant="outline" className="text-[10px] px-1 py-0">
                                    S{(option.metadata as { defaultSeverity?: number }).defaultSeverity}
                                  </Badge>
                                </div>
                              )}
                            </div>
                          )}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="whoAtRisk"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Who is at Risk?</FormLabel>
                      <FormControl>
                        <Input placeholder="e.g., Employees, Contractors" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <Separator />

              {/* Initial Risk */}
              <div>
                <h4 className="text-sm font-medium mb-3">Initial Risk (Before Controls)</h4>
                <div className="grid gap-4 sm:grid-cols-3">
                  <FormField
                    control={form.control}
                    name="initialLikelihood"
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
                    name="initialSeverity"
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
                  <div>
                    <Label className="text-sm font-medium">Initial Risk Rating</Label>
                    <div className="flex items-center gap-2 mt-2">
                      <Badge className={cn("text-lg px-3 py-1", initialRiskLevel.className)}>
                        {initialRating}
                      </Badge>
                      <span className="text-sm text-muted-foreground">{initialRiskLevel.level}</span>
                    </div>
                  </div>
                </div>
              </div>

              <Separator />

              {/* Control Measures */}
              <div>
                <div className="flex items-center justify-between mb-3">
                  <h4 className="text-sm font-medium">Control Measures</h4>
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={handleGetSuggestions}
                    disabled={suggestControls.isPending || !taskActivity || !hazardIdentified}
                  >
                    {suggestControls.isPending ? (
                      <>
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        Getting Suggestions...
                      </>
                    ) : (
                      <>
                        <Sparkles className="mr-2 h-4 w-4" />
                        AI Suggest
                      </>
                    )}
                  </Button>
                </div>

                {/* AI Suggestions Panel - Inline */}
                {showSuggestions && suggestions && (
                  <Card className="mb-4 border-primary/20 bg-primary/5">
                    <CardHeader className="pb-2 pt-3">
                      <div className="flex items-center justify-between">
                        <CardTitle className="text-sm flex items-center gap-2">
                          <Sparkles className="h-4 w-4 text-primary" />
                          AI Suggestions
                          {suggestions.usedAi && (
                            <Badge variant="secondary" className="text-[10px]">AI Enhanced</Badge>
                          )}
                        </CardTitle>
                        <Button
                          type="button"
                          variant="ghost"
                          size="icon"
                          className="h-6 w-6"
                          onClick={handleDismissSuggestions}
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      </div>
                    </CardHeader>
                    <CardContent className="pb-3 pt-0">
                      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3 mb-3">
                        {/* Matched Hazards */}
                        {suggestions.matchedHazards.length > 0 && (
                          <div>
                            <h6 className="text-xs font-medium text-muted-foreground mb-1">Matched Hazards</h6>
                            <div className="space-y-1">
                              {suggestions.matchedHazards.slice(0, 3).map((h) => (
                                <div key={h.id} className="text-xs">
                                  <Badge variant="outline" className="mr-1 text-[10px]">{h.category}</Badge>
                                  {h.name}
                                </div>
                              ))}
                            </div>
                          </div>
                        )}

                        {/* Legislation */}
                        {suggestions.relevantLegislation.length > 0 && (
                          <div>
                            <h6 className="text-xs font-medium text-muted-foreground mb-1">Relevant Legislation</h6>
                            <div className="flex flex-wrap gap-1">
                              {suggestions.relevantLegislation.map((l) => (
                                <Badge key={l.id} variant="outline" className="text-[10px]">
                                  {l.shortName || l.code}
                                </Badge>
                              ))}
                            </div>
                          </div>
                        )}

                        {/* SOPs */}
                        {suggestions.relevantSops.length > 0 && (
                          <div>
                            <h6 className="text-xs font-medium text-muted-foreground mb-1">Reference SOPs</h6>
                            <div className="flex flex-wrap gap-1">
                              {suggestions.relevantSops.map((s) => (
                                <Badge key={s.id} variant="outline" className="text-[10px]">
                                  {s.sopId}
                                </Badge>
                              ))}
                            </div>
                          </div>
                        )}

                        {/* Suggested Residual Risk */}
                        {(suggestions.suggestedResidualLikelihood || suggestions.suggestedResidualSeverity) && (
                          <div>
                            <h6 className="text-xs font-medium text-muted-foreground mb-1">Suggested Residual Risk</h6>
                            <div className="text-xs flex items-center gap-2">
                              L{suggestions.suggestedResidualLikelihood} × S{suggestions.suggestedResidualSeverity} ={" "}
                              <Badge className={cn(
                                "text-xs",
                                getRiskLevel((suggestions.suggestedResidualLikelihood || 1) * (suggestions.suggestedResidualSeverity || 1)).className
                              )}>
                                {(suggestions.suggestedResidualLikelihood || 1) * (suggestions.suggestedResidualSeverity || 1)}
                              </Badge>
                            </div>
                          </div>
                        )}
                      </div>

                      {/* AI Generated Controls or Library Controls */}
                      {suggestions.aiGeneratedControlMeasures ? (
                        <div className="mb-3">
                          <h6 className="text-xs font-medium text-muted-foreground mb-1">Suggested Control Measures</h6>
                          <div className="text-xs bg-background p-2 rounded border whitespace-pre-wrap max-h-[120px] overflow-y-auto">
                            {suggestions.aiGeneratedControlMeasures}
                          </div>
                        </div>
                      ) : suggestions.suggestedControls.length > 0 && (
                        <div className="mb-3">
                          <h6 className="text-xs font-medium text-muted-foreground mb-1">Library Control Measures</h6>
                          <div className="grid gap-2 sm:grid-cols-2">
                            {suggestions.suggestedControls.slice(0, 4).map((c) => (
                              <div key={c.id} className="text-xs bg-background p-2 rounded border">
                                <div className="flex justify-between items-start gap-1">
                                  <strong className="line-clamp-1">{c.name}</strong>
                                  <Badge variant="secondary" className="text-[10px] shrink-0">{c.hierarchy}</Badge>
                                </div>
                                <div className="text-muted-foreground mt-1 line-clamp-2">{c.description}</div>
                              </div>
                            ))}
                          </div>
                        </div>
                      )}

                      {/* Error message */}
                      {!suggestions.success && suggestions.errorMessage && (
                        <div className="flex items-start gap-2 text-xs text-destructive mb-3">
                          <AlertCircle className="h-4 w-4 mt-0.5 shrink-0" />
                          {suggestions.errorMessage}
                        </div>
                      )}

                      <Button
                        type="button"
                        className="w-full"
                        size="sm"
                        onClick={handleApplySuggestions}
                        disabled={!suggestions.success}
                      >
                        <Check className="mr-2 h-4 w-4" />
                        Apply Suggestions
                      </Button>
                    </CardContent>
                  </Card>
                )}

                <div className="space-y-4">
                  <div>
                    <Label className="text-sm">Select from Library</Label>
                    <FormDescription className="text-xs mb-2">
                      Auto-calculates residual risk based on selections
                    </FormDescription>
                    <MultiSelectCombobox
                      options={controlOptions}
                      selectedValues={selectedControlIds}
                      onValuesChange={handleControlsChange}
                      placeholder="Select control measures..."
                      searchPlaceholder="Search controls..."
                      isLoading={controlsLoading}
                      renderOption={(option) => (
                        <div>
                          <div className="font-medium">{option.label}</div>
                          <div className="text-xs text-muted-foreground line-clamp-2">
                            {option.description}
                          </div>
                        </div>
                      )}
                    />
                  </div>

                  <FormField
                    control={form.control}
                    name="controlMeasures"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Control Measures Description</FormLabel>
                        <FormDescription className="text-xs">
                          Auto-filled from selection or enter custom
                        </FormDescription>
                        <FormControl>
                          <Textarea
                            placeholder="Describe the control measures to reduce the risk..."
                            className="min-h-[100px]"
                            {...field}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <div className="grid gap-4 sm:grid-cols-2">
                    <div>
                      <Label className="text-sm">Relevant Legislation</Label>
                      <MultiSelectCombobox
                        options={legislationOptions}
                        selectedValues={selectedLegislationIds}
                        onValuesChange={handleLegislationChange}
                        placeholder="Select legislation..."
                        searchPlaceholder="Search legislation..."
                        isLoading={legislationLoading}
                        className="mt-2"
                      />
                      <FormField
                        control={form.control}
                        name="relevantLegislation"
                        render={({ field }) => (
                          <FormItem className="mt-2">
                            <FormControl>
                              <Input placeholder="Or type custom references..." {...field} />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                    </div>
                    <div>
                      <Label className="text-sm">Reference SOPs</Label>
                      <MultiSelectCombobox
                        options={sopOptions}
                        selectedValues={selectedSopIds}
                        onValuesChange={handleSopsChange}
                        placeholder="Select SOPs..."
                        searchPlaceholder="Search SOPs..."
                        isLoading={sopsLoading}
                        className="mt-2"
                      />
                      <FormField
                        control={form.control}
                        name="referenceSops"
                        render={({ field }) => (
                          <FormItem className="mt-2">
                            <FormControl>
                              <Input placeholder="Or type custom references..." {...field} />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                    </div>
                  </div>
                </div>
              </div>

              <Separator />

              {/* Residual Risk */}
              <div>
                <h4 className="text-sm font-medium mb-3">Residual Risk (After Controls)</h4>
                <div className="grid gap-4 sm:grid-cols-3">
                  <FormField
                    control={form.control}
                    name="residualLikelihood"
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
                    name="residualSeverity"
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
                  <div>
                    <Label className="text-sm font-medium">Residual Risk Rating</Label>
                    <div className="flex items-center gap-2 mt-2">
                      <Badge className={cn("text-lg px-3 py-1", residualRiskLevel.className)}>
                        {residualRating}
                      </Badge>
                      <span className="text-sm text-muted-foreground">{residualRiskLevel.level}</span>
                    </div>
                  </div>
                </div>
              </div>

              <DialogFooter>
                <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
                  Cancel
                </Button>
                <Button type="submit" disabled={isLoading}>
                  {isLoading && (
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  )}
                  {isEditing ? "Save Changes" : "Add Risk Assessment"}
                </Button>
              </DialogFooter>
            </form>
          </Form>
        </div>
      </DialogContent>
    </Dialog>
  );
}

