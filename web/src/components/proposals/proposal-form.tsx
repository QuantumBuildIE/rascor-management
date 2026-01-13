"use client";

import * as React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { format, addDays, parseISO } from "date-fns";
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
import { CompanySelect } from "./company-select";
import { ContactSelect } from "./contact-select";
import type { Proposal, CreateProposalDto, UpdateProposalDto } from "@/lib/api/proposals";

const CURRENCIES = ["EUR", "GBP", "USD"] as const;

const proposalFormSchema = z.object({
  companyId: z.string().min(1, "Company is required"),
  primaryContactId: z.string().optional(),
  projectName: z.string().min(1, "Project name is required").max(200),
  projectAddress: z.string().optional(),
  projectDescription: z.string().optional(),
  proposalDate: z.string().min(1, "Proposal date is required"),
  validUntilDate: z.string().optional(),
  currency: z.string().min(1),
  vatRate: z.number().min(0).max(100),
  discountPercent: z.number().min(0).max(100),
  paymentTerms: z.string().optional(),
  termsAndConditions: z.string().optional(),
  notes: z.string().optional(),
});

type ProposalFormValues = z.infer<typeof proposalFormSchema>;

interface ProposalFormProps {
  proposal?: Proposal;
  onSubmit: (data: CreateProposalDto | UpdateProposalDto) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

export function ProposalForm({
  proposal,
  onSubmit,
  onCancel,
  isLoading = false,
}: ProposalFormProps) {
  const isEditing = !!proposal;

  const form = useForm<ProposalFormValues>({
    resolver: zodResolver(proposalFormSchema) as any,
    defaultValues: {
      companyId: proposal?.companyId ?? "",
      primaryContactId: proposal?.primaryContactId ?? "",
      projectName: proposal?.projectName ?? "",
      projectAddress: proposal?.projectAddress ?? "",
      projectDescription: proposal?.projectDescription ?? "",
      proposalDate: proposal?.proposalDate ?? format(new Date(), "yyyy-MM-dd"),
      validUntilDate: proposal?.validUntilDate ?? "",
      currency: proposal?.currency ?? "EUR",
      vatRate: proposal?.vatRate ?? 23,
      discountPercent: proposal?.discountPercent ?? 0,
      paymentTerms: proposal?.paymentTerms ?? "",
      termsAndConditions: proposal?.termsAndConditions ?? "",
      notes: proposal?.notes ?? "",
    },
  });

  const selectedCompanyId = form.watch("companyId");
  const proposalDate = form.watch("proposalDate");

  const handleFormSubmit = async (values: ProposalFormValues) => {
    const data: CreateProposalDto | UpdateProposalDto = {
      companyId: values.companyId,
      primaryContactId: values.primaryContactId || undefined,
      projectName: values.projectName,
      projectAddress: values.projectAddress || undefined,
      projectDescription: values.projectDescription || undefined,
      proposalDate: values.proposalDate,
      validUntilDate: values.validUntilDate || undefined,
      currency: values.currency,
      vatRate: values.vatRate,
      discountPercent: values.discountPercent,
      paymentTerms: values.paymentTerms || undefined,
      termsAndConditions: values.termsAndConditions || undefined,
      notes: values.notes || undefined,
    };
    await onSubmit(data);
  };

  const setValidityDate = (days: number) => {
    if (proposalDate) {
      const baseDate = parseISO(proposalDate);
      const newDate = addDays(baseDate, days);
      form.setValue("validUntilDate", format(newDate, "yyyy-MM-dd"));
    }
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleFormSubmit)} className="space-y-8">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {/* Left column: Client, Project Details */}
          <div className="space-y-6">
            {/* Client Section */}
            <div className="space-y-4">
              <h3 className="text-lg font-medium">Client</h3>

              <FormField
                control={form.control}
                name="companyId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Company *</FormLabel>
                    <FormControl>
                      <CompanySelect
                        value={field.value}
                        onChange={(value) => {
                          field.onChange(value);
                          // Clear contact when company changes
                          form.setValue("primaryContactId", "");
                        }}
                        disabled={isLoading}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="primaryContactId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Primary Contact</FormLabel>
                    <FormControl>
                      <ContactSelect
                        companyId={selectedCompanyId}
                        value={field.value || ""}
                        onChange={field.onChange}
                        disabled={isLoading || !selectedCompanyId}
                      />
                    </FormControl>
                    <FormDescription>
                      {!selectedCompanyId && "Select a company first"}
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* Project Details Section */}
            <div className="space-y-4">
              <h3 className="text-lg font-medium">Project Details</h3>

              <FormField
                control={form.control}
                name="projectName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Project Name *</FormLabel>
                    <FormControl>
                      <Input placeholder="e.g., Office Renovation Phase 1" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="projectAddress"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Project Address</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder="Enter the project location address"
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
                name="projectDescription"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Project Description</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder="Brief description of the project scope"
                        className="resize-none"
                        rows={3}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
          </div>

          {/* Right column: Dates, Pricing, Terms */}
          <div className="space-y-6">
            {/* Dates Section */}
            <div className="space-y-4">
              <h3 className="text-lg font-medium">Dates</h3>

              <FormField
                control={form.control}
                name="proposalDate"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Proposal Date *</FormLabel>
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
                name="validUntilDate"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Valid Until</FormLabel>
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
                    <div className="flex gap-2 mt-2">
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() => setValidityDate(30)}
                        disabled={!proposalDate}
                      >
                        +30 days
                      </Button>
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() => setValidityDate(60)}
                        disabled={!proposalDate}
                      >
                        +60 days
                      </Button>
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() => setValidityDate(90)}
                        disabled={!proposalDate}
                      >
                        +90 days
                      </Button>
                    </div>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* Pricing Settings Section */}
            <div className="space-y-4">
              <h3 className="text-lg font-medium">Pricing Settings</h3>

              <FormField
                control={form.control}
                name="currency"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Currency</FormLabel>
                    <Select onValueChange={field.onChange} defaultValue={field.value}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select currency" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {CURRENCIES.map((currency) => (
                          <SelectItem key={currency} value={currency}>
                            {currency}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <div className="grid grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="vatRate"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>VAT Rate (%)</FormLabel>
                      <FormControl>
                        <div className="relative">
                          <Input
                            type="number"
                            step="0.01"
                            min="0"
                            max="100"
                            placeholder="23"
                            value={field.value}
                            onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                            onBlur={field.onBlur}
                            name={field.name}
                            ref={field.ref}
                          />
                          <span className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground">
                            %
                          </span>
                        </div>
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="discountPercent"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Discount (%)</FormLabel>
                      <FormControl>
                        <div className="relative">
                          <Input
                            type="number"
                            step="0.01"
                            min="0"
                            max="100"
                            placeholder="0"
                            value={field.value}
                            onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                            onBlur={field.onBlur}
                            name={field.name}
                            ref={field.ref}
                          />
                          <span className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground">
                            %
                          </span>
                        </div>
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>
            </div>

            {/* Terms Section */}
            <div className="space-y-4">
              <h3 className="text-lg font-medium">Terms</h3>

              <FormField
                control={form.control}
                name="paymentTerms"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Payment Terms</FormLabel>
                    <FormControl>
                      <Input placeholder="30 days from invoice" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="termsAndConditions"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Terms & Conditions</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder="Enter terms and conditions"
                        className="resize-none"
                        rows={3}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
          </div>
        </div>

        {/* Full width: Notes */}
        <div className="space-y-4">
          <h3 className="text-lg font-medium">Notes</h3>

          <FormField
            control={form.control}
            name="notes"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Internal Notes</FormLabel>
                <FormControl>
                  <Textarea
                    placeholder="Internal notes (not shown on proposal)"
                    className="resize-none"
                    rows={3}
                    {...field}
                  />
                </FormControl>
                <FormDescription>
                  These notes are for internal use only and will not appear on the proposal.
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        {/* Actions */}
        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={onCancel} disabled={isLoading}>
            Cancel
          </Button>
          <Button type="submit" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {isEditing ? "Save Changes" : "Save as Draft"}
          </Button>
        </div>
      </form>
    </Form>
  );
}

