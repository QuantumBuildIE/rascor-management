"use client";

import * as React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { format } from "date-fns";
import { CalendarIcon } from "lucide-react";
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
import { Textarea } from "@/components/ui/textarea";
import { Calendar } from "@/components/ui/calendar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { cn } from "@/lib/utils";
import {
  useSubmitProposal,
  useApproveProposal,
  useRejectProposal,
  useWinProposal,
  useLoseProposal,
  useCancelProposal,
  useCreateRevision,
  type Proposal,
} from "@/lib/api/proposals";
import { toast } from "sonner";

// Submit Dialog
const submitSchema = z.object({
  notes: z.string().optional(),
});

type SubmitFormValues = z.infer<typeof submitSchema>;

interface SubmitDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  proposal: Proposal | null;
}

export function SubmitDialog({ open, onOpenChange, proposal }: SubmitDialogProps) {
  const submitProposal = useSubmitProposal();

  const form = useForm<SubmitFormValues>({
    resolver: zodResolver(submitSchema) as any,
    defaultValues: {
      notes: "",
    },
  });

  React.useEffect(() => {
    if (open) {
      form.reset({ notes: "" });
    }
  }, [open, form]);

  const onSubmit = async (values: SubmitFormValues) => {
    if (!proposal) return;

    try {
      await submitProposal.mutateAsync({
        id: proposal.id,
        notes: values.notes,
      });
      toast.success("Proposal submitted successfully");
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to submit proposal", { description: message });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Submit Proposal</DialogTitle>
          <DialogDescription>
            Submit proposal {proposal?.proposalNumber} for approval
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="notes"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Notes (optional)</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Add any notes for the reviewer..."
                      className="resize-none"
                      {...field}
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
              <Button type="submit" disabled={submitProposal.isPending}>
                {submitProposal.isPending ? "Submitting..." : "Submit Proposal"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}

// Approve Dialog
const approveSchema = z.object({
  notes: z.string().optional(),
});

type ApproveFormValues = z.infer<typeof approveSchema>;

interface ApproveDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  proposal: Proposal | null;
}

export function ApproveDialog({ open, onOpenChange, proposal }: ApproveDialogProps) {
  const approveProposal = useApproveProposal();

  const form = useForm<ApproveFormValues>({
    resolver: zodResolver(approveSchema) as any,
    defaultValues: {
      notes: "",
    },
  });

  React.useEffect(() => {
    if (open) {
      form.reset({ notes: "" });
    }
  }, [open, form]);

  const onSubmit = async (values: ApproveFormValues) => {
    if (!proposal) return;

    try {
      await approveProposal.mutateAsync({
        id: proposal.id,
        notes: values.notes,
      });
      toast.success("Proposal approved successfully");
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to approve proposal", { description: message });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Approve Proposal</DialogTitle>
          <DialogDescription>
            Approve proposal {proposal?.proposalNumber}
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="notes"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Notes (optional)</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Add approval notes..."
                      className="resize-none"
                      {...field}
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
              <Button type="submit" disabled={approveProposal.isPending}>
                {approveProposal.isPending ? "Approving..." : "Approve Proposal"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}

// Reject Dialog
const rejectSchema = z.object({
  reason: z.string().min(1, "Reason is required"),
});

type RejectFormValues = z.infer<typeof rejectSchema>;

interface RejectDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  proposal: Proposal | null;
}

export function RejectDialog({ open, onOpenChange, proposal }: RejectDialogProps) {
  const rejectProposal = useRejectProposal();

  const form = useForm<RejectFormValues>({
    resolver: zodResolver(rejectSchema) as any,
    defaultValues: {
      reason: "",
    },
  });

  React.useEffect(() => {
    if (open) {
      form.reset({ reason: "" });
    }
  }, [open, form]);

  const onSubmit = async (values: RejectFormValues) => {
    if (!proposal) return;

    try {
      await rejectProposal.mutateAsync({
        id: proposal.id,
        reason: values.reason,
      });
      toast.success("Proposal rejected");
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to reject proposal", { description: message });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Reject Proposal</DialogTitle>
          <DialogDescription>
            Reject proposal {proposal?.proposalNumber}
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="reason"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Reason *</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Enter the reason for rejection..."
                      className="resize-none"
                      {...field}
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
              <Button
                type="submit"
                variant="destructive"
                disabled={rejectProposal.isPending}
              >
                {rejectProposal.isPending ? "Rejecting..." : "Reject Proposal"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}

// Win Dialog
const winSchema = z.object({
  reason: z.string().optional(),
  wonDate: z.date().optional(),
});

type WinFormValues = z.infer<typeof winSchema>;

interface WinDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  proposal: Proposal | null;
}

export function WinDialog({ open, onOpenChange, proposal }: WinDialogProps) {
  const winProposal = useWinProposal();

  const form = useForm<WinFormValues>({
    resolver: zodResolver(winSchema) as any,
    defaultValues: {
      reason: "",
      wonDate: new Date(),
    },
  });

  React.useEffect(() => {
    if (open) {
      form.reset({ reason: "", wonDate: new Date() });
    }
  }, [open, form]);

  const onSubmit = async (values: WinFormValues) => {
    if (!proposal) return;

    try {
      await winProposal.mutateAsync({
        id: proposal.id,
        reason: values.reason,
        wonDate: values.wonDate ? format(values.wonDate, "yyyy-MM-dd") : undefined,
      });
      toast.success("Proposal marked as won");
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to update proposal", { description: message });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Mark as Won</DialogTitle>
          <DialogDescription>
            Mark proposal {proposal?.proposalNumber} as won
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="wonDate"
              render={({ field }) => (
                <FormItem className="flex flex-col">
                  <FormLabel>Won Date</FormLabel>
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
                            format(field.value, "PPP")
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
                        selected={field.value}
                        onSelect={field.onChange}
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
              name="reason"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Notes (optional)</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Add any notes about the win..."
                      className="resize-none"
                      {...field}
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
              <Button type="submit" disabled={winProposal.isPending}>
                {winProposal.isPending ? "Saving..." : "Mark as Won"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}

// Lose Dialog
const loseSchema = z.object({
  reason: z.string().min(1, "Reason is required"),
  lostDate: z.date().optional(),
});

type LoseFormValues = z.infer<typeof loseSchema>;

interface LoseDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  proposal: Proposal | null;
}

export function LoseDialog({ open, onOpenChange, proposal }: LoseDialogProps) {
  const loseProposal = useLoseProposal();

  const form = useForm<LoseFormValues>({
    resolver: zodResolver(loseSchema) as any,
    defaultValues: {
      reason: "",
      lostDate: new Date(),
    },
  });

  React.useEffect(() => {
    if (open) {
      form.reset({ reason: "", lostDate: new Date() });
    }
  }, [open, form]);

  const onSubmit = async (values: LoseFormValues) => {
    if (!proposal) return;

    try {
      await loseProposal.mutateAsync({
        id: proposal.id,
        reason: values.reason,
        lostDate: values.lostDate ? format(values.lostDate, "yyyy-MM-dd") : undefined,
      });
      toast.success("Proposal marked as lost");
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to update proposal", { description: message });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Mark as Lost</DialogTitle>
          <DialogDescription>
            Mark proposal {proposal?.proposalNumber} as lost
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="lostDate"
              render={({ field }) => (
                <FormItem className="flex flex-col">
                  <FormLabel>Lost Date</FormLabel>
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
                            format(field.value, "PPP")
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
                        selected={field.value}
                        onSelect={field.onChange}
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
              name="reason"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Reason *</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Enter the reason for losing this proposal..."
                      className="resize-none"
                      {...field}
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
              <Button
                type="submit"
                variant="destructive"
                disabled={loseProposal.isPending}
              >
                {loseProposal.isPending ? "Saving..." : "Mark as Lost"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}

// Cancel Dialog
interface CancelDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  proposal: Proposal | null;
}

export function CancelDialog({ open, onOpenChange, proposal }: CancelDialogProps) {
  const cancelProposal = useCancelProposal();

  const handleCancel = async () => {
    if (!proposal) return;

    try {
      await cancelProposal.mutateAsync(proposal.id);
      toast.success("Proposal cancelled");
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to cancel proposal", { description: message });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Cancel Proposal</DialogTitle>
          <DialogDescription>
            Are you sure you want to cancel proposal {proposal?.proposalNumber}? This
            action cannot be undone.
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            No, Keep Proposal
          </Button>
          <Button
            variant="destructive"
            onClick={handleCancel}
            disabled={cancelProposal.isPending}
          >
            {cancelProposal.isPending ? "Cancelling..." : "Yes, Cancel Proposal"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// Create Revision Dialog
const revisionSchema = z.object({
  notes: z.string().optional(),
});

type RevisionFormValues = z.infer<typeof revisionSchema>;

interface CreateRevisionDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  proposal: Proposal | null;
  onRevisionCreated?: (newProposal: Proposal) => void;
}

export function CreateRevisionDialog({
  open,
  onOpenChange,
  proposal,
  onRevisionCreated,
}: CreateRevisionDialogProps) {
  const createRevision = useCreateRevision();

  const form = useForm<RevisionFormValues>({
    resolver: zodResolver(revisionSchema) as any,
    defaultValues: {
      notes: "",
    },
  });

  React.useEffect(() => {
    if (open) {
      form.reset({ notes: "" });
    }
  }, [open, form]);

  const onSubmit = async (values: RevisionFormValues) => {
    if (!proposal) return;

    try {
      const newProposal = await createRevision.mutateAsync({
        id: proposal.id,
        notes: values.notes,
      });
      toast.success("Revision created successfully");
      onOpenChange(false);
      if (onRevisionCreated) {
        onRevisionCreated(newProposal);
      }
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to create revision", { description: message });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Create Revision</DialogTitle>
          <DialogDescription>
            Create a new revision of proposal {proposal?.proposalNumber}. This will
            create a new draft proposal based on this one.
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <div className="rounded-md bg-muted p-3 text-sm">
              A new proposal will be created with version{" "}
              {proposal ? proposal.version + 1 : ""}. The current proposal will remain
              unchanged.
            </div>

            <FormField
              control={form.control}
              name="notes"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Notes (optional)</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Add any notes about this revision..."
                      className="resize-none"
                      {...field}
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
              <Button type="submit" disabled={createRevision.isPending}>
                {createRevision.isPending ? "Creating..." : "Create Revision"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}

