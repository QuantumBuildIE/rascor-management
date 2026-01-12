"use client";

import * as React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { format, parseISO } from "date-fns";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Calendar } from "@/components/ui/calendar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { CalendarIcon, Loader2 } from "lucide-react";
import { cn } from "@/lib/utils";
import { useAllSites } from "@/lib/api/admin/use-sites";
import { useAllEmployees } from "@/lib/api/admin/use-employees";
import type { RamsDocument, CreateRamsDocumentDto, UpdateRamsDocumentDto, ProjectType } from "@/types/rams";
import { ProjectTypeLabels } from "@/types/rams";

const PROJECT_TYPES: ProjectType[] = [
  "RemedialInjection",
  "RascotankNewBuild",
  "CarParkCoating",
  "GroundGasBarrier",
  "Other",
];

const ramsFormSchema = z.object({
  projectReference: z.string().min(1, "Project reference is required").max(50),
  projectName: z.string().min(1, "Project name is required").max(200),
  projectType: z.enum(["RemedialInjection", "RascotankNewBuild", "CarParkCoating", "GroundGasBarrier", "Other"]),
  clientName: z.string().optional(),
  siteAddress: z.string().optional(),
  areaOfActivity: z.string().optional(),
  proposedStartDate: z.string().optional(),
  proposedEndDate: z.string().optional(),
  safetyOfficerId: z.string().optional(),
  siteId: z.string().optional(),
  methodStatementBody: z.string().optional(),
});

type RamsFormValues = z.infer<typeof ramsFormSchema>;

interface RamsFormProps {
  rams?: RamsDocument;
  onSubmit: (data: CreateRamsDocumentDto | UpdateRamsDocumentDto) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

export function RamsForm({
  rams,
  onSubmit,
  onCancel,
  isLoading = false,
}: RamsFormProps) {
  const isEditing = !!rams;

  const { data: sites } = useAllSites();
  const { data: employees } = useAllEmployees();

  const form = useForm<RamsFormValues>({
    resolver: zodResolver(ramsFormSchema),
    defaultValues: {
      projectReference: rams?.projectReference ?? "",
      projectName: rams?.projectName ?? "",
      projectType: rams?.projectType ?? "Other",
      clientName: rams?.clientName ?? "",
      siteAddress: rams?.siteAddress ?? "",
      areaOfActivity: rams?.areaOfActivity ?? "",
      proposedStartDate: rams?.proposedStartDate ?? "",
      proposedEndDate: rams?.proposedEndDate ?? "",
      safetyOfficerId: rams?.safetyOfficerId ?? "",
      siteId: rams?.siteId ?? "",
      methodStatementBody: rams?.methodStatementBody ?? "",
    },
  });

  const handleFormSubmit = async (values: RamsFormValues) => {
    const data: CreateRamsDocumentDto | UpdateRamsDocumentDto = {
      projectReference: values.projectReference,
      projectName: values.projectName,
      projectType: values.projectType,
      clientName: values.clientName || undefined,
      siteAddress: values.siteAddress || undefined,
      areaOfActivity: values.areaOfActivity || undefined,
      proposedStartDate: values.proposedStartDate || undefined,
      proposedEndDate: values.proposedEndDate || undefined,
      safetyOfficerId: values.safetyOfficerId || undefined,
      siteId: values.siteId || undefined,
      methodStatementBody: values.methodStatementBody || undefined,
    };
    await onSubmit(data);
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleFormSubmit)} className="space-y-8">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {/* Left column: Project Info */}
          <div className="space-y-6">
            <div className="space-y-4">
              <h3 className="text-lg font-medium">Project Information</h3>

              <FormField
                control={form.control}
                name="projectReference"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Project Reference *</FormLabel>
                    <FormControl>
                      <Input placeholder="e.g., UK240001" {...field} />
                    </FormControl>
                    <FormDescription>
                      Unique reference for this RAMS document
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="projectType"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Project Type *</FormLabel>
                    <Select onValueChange={field.onChange} defaultValue={field.value}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select project type" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {PROJECT_TYPES.map((type) => (
                          <SelectItem key={type} value={type}>
                            {ProjectTypeLabels[type]}
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
                name="projectName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Project Name *</FormLabel>
                    <FormControl>
                      <Input placeholder="Enter project name" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="clientName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Client Name</FormLabel>
                    <FormControl>
                      <Input placeholder="Enter client name" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="siteId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Site</FormLabel>
                    <Select
                      onValueChange={field.onChange}
                      defaultValue={field.value}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select site (optional)" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {sites?.map((site) => (
                          <SelectItem key={site.id} value={site.id}>
                            {site.siteName}
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
                name="siteAddress"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Site Address</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder="Enter site address"
                        className="resize-none"
                        rows={2}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="areaOfActivity"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Area of Activity</FormLabel>
                    <FormControl>
                      <Input
                        placeholder="e.g., Basement Sub-structure and Ground Floor"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
          </div>

          {/* Right column: Dates, Safety Officer, Method Statement */}
          <div className="space-y-6">
            <div className="space-y-4">
              <h3 className="text-lg font-medium">Schedule & Safety</h3>

              <FormField
                control={form.control}
                name="proposedStartDate"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Proposed Start Date</FormLabel>
                    <Popover>
                      <PopoverTrigger asChild>
                        <FormControl>
                          <Button
                            variant="outline"
                            className={cn(
                              "w-full pl-3 text-left font-normal",
                              !field.value && "text-muted-foreground"
                            )}
                          >
                            {field.value ? (
                              format(parseISO(field.value), "dd MMM yyyy")
                            ) : (
                              <span>Pick a date</span>
                            )}
                            <CalendarIcon className="ml-auto h-4 w-4 opacity-50" />
                          </Button>
                        </FormControl>
                      </PopoverTrigger>
                      <PopoverContent className="w-auto p-0" align="start">
                        <Calendar
                          mode="single"
                          selected={field.value ? parseISO(field.value) : undefined}
                          onSelect={(date) => {
                            if (date) {
                              field.onChange(format(date, "yyyy-MM-dd"));
                            } else {
                              field.onChange("");
                            }
                          }}
                          initialFocus
                        />
                      </PopoverContent>
                    </Popover>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="proposedEndDate"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Proposed End Date</FormLabel>
                    <Popover>
                      <PopoverTrigger asChild>
                        <FormControl>
                          <Button
                            variant="outline"
                            className={cn(
                              "w-full pl-3 text-left font-normal",
                              !field.value && "text-muted-foreground"
                            )}
                          >
                            {field.value ? (
                              format(parseISO(field.value), "dd MMM yyyy")
                            ) : (
                              <span>Pick a date</span>
                            )}
                            <CalendarIcon className="ml-auto h-4 w-4 opacity-50" />
                          </Button>
                        </FormControl>
                      </PopoverTrigger>
                      <PopoverContent className="w-auto p-0" align="start">
                        <Calendar
                          mode="single"
                          selected={field.value ? parseISO(field.value) : undefined}
                          onSelect={(date) => {
                            if (date) {
                              field.onChange(format(date, "yyyy-MM-dd"));
                            } else {
                              field.onChange("");
                            }
                          }}
                          initialFocus
                        />
                      </PopoverContent>
                    </Popover>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="safetyOfficerId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Safety Officer</FormLabel>
                    <Select
                      onValueChange={field.onChange}
                      defaultValue={field.value}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select safety officer (optional)" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {employees?.map((employee) => (
                          <SelectItem key={employee.id} value={employee.id}>
                            {employee.fullName}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <div className="space-y-4">
              <h3 className="text-lg font-medium">Method Statement Overview</h3>

              <FormField
                control={form.control}
                name="methodStatementBody"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Overview</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder="Enter initial method statement overview (can be expanded later)..."
                        className="resize-none"
                        rows={8}
                        {...field}
                      />
                    </FormControl>
                    <FormDescription>
                      You can add detailed method steps after creating the document.
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
          </div>
        </div>

        {/* Actions */}
        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={onCancel} disabled={isLoading}>
            Cancel
          </Button>
          <Button type="submit" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {isEditing ? "Save Changes" : "Create Document"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
