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
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import { cn } from "@/lib/utils";
import { Check, ChevronsUpDown } from "lucide-react";
import { useCompanyContacts } from "@/lib/api/admin/use-contacts";
import {
  useAddContact,
  useUpdateContact,
  type ProposalContact,
} from "@/lib/api/proposals";
import { toast } from "sonner";

const CONTACT_ROLES = [
  "Quantity Surveyor",
  "Site Manager",
  "Contracts Manager",
  "Safety Officer",
  "Project Manager",
  "Other",
];

const contactSchema = z.object({
  existingContactId: z.string().optional(),
  contactName: z.string().min(1, "Contact name is required"),
  email: z.string().email("Invalid email address").optional().or(z.literal("")),
  phone: z.string().optional(),
  role: z.string().min(1, "Role is required"),
  isPrimary: z.boolean(),
});

type ContactFormValues = z.infer<typeof contactSchema>;

interface ContactDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  proposalId: string;
  companyId: string;
  contact?: ProposalContact | null;
}

export function ContactDialog({
  open,
  onOpenChange,
  proposalId,
  companyId,
  contact,
}: ContactDialogProps) {
  const [contactPopoverOpen, setContactPopoverOpen] = React.useState(false);

  const { data: companyContacts } = useCompanyContacts(companyId);
  const addContact = useAddContact();
  const updateContact = useUpdateContact();

  const contacts = companyContacts ?? [];
  const isEditing = !!contact;

  const form = useForm<ContactFormValues>({
    resolver: zodResolver(contactSchema),
    defaultValues: {
      existingContactId: "",
      contactName: "",
      email: "",
      phone: "",
      role: "",
      isPrimary: false,
    },
  });

  React.useEffect(() => {
    if (open) {
      if (contact) {
        form.reset({
          existingContactId: contact.contactId ?? "",
          contactName: contact.contactName,
          email: contact.email ?? "",
          phone: contact.phone ?? "",
          role: contact.role,
          isPrimary: contact.isPrimary,
        });
      } else {
        form.reset({
          existingContactId: "",
          contactName: "",
          email: "",
          phone: "",
          role: "",
          isPrimary: false,
        });
      }
    }
  }, [open, contact, form]);

  const handleExistingContactSelect = (contactId: string) => {
    const existingContact = contacts.find((c) => c.id === contactId);
    if (existingContact) {
      form.setValue("existingContactId", contactId);
      form.setValue("contactName", existingContact.fullName);
      form.setValue("email", existingContact.email ?? "");
      form.setValue("phone", existingContact.phone ?? existingContact.mobile ?? "");
    }
    setContactPopoverOpen(false);
  };

  const onSubmit = async (values: ContactFormValues) => {
    try {
      if (isEditing && contact) {
        await updateContact.mutateAsync({
          contactId: contact.id,
          data: {
            contactId: values.existingContactId || undefined,
            contactName: values.contactName,
            email: values.email || undefined,
            phone: values.phone || undefined,
            role: values.role,
            isPrimary: values.isPrimary,
          },
        });
        toast.success("Contact updated successfully");
      } else {
        await addContact.mutateAsync({
          proposalId,
          data: {
            contactId: values.existingContactId || undefined,
            contactName: values.contactName,
            email: values.email || undefined,
            phone: values.phone || undefined,
            role: values.role,
            isPrimary: values.isPrimary,
          },
        });
        toast.success("Contact added successfully");
      }
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error(
        isEditing ? "Failed to update contact" : "Failed to add contact",
        { description: message }
      );
    }
  };

  const isPending = addContact.isPending || updateContact.isPending;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>
            {isEditing ? "Edit Contact" : "Add Contact"}
          </DialogTitle>
          <DialogDescription>
            {isEditing
              ? "Update the contact details"
              : "Add a contact to this proposal. You can select from existing company contacts or enter manually."}
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            {contacts.length > 0 && (
              <FormField
                control={form.control}
                name="existingContactId"
                render={({ field }) => (
                  <FormItem className="flex flex-col">
                    <FormLabel>Select from Company Contacts</FormLabel>
                    <Popover
                      open={contactPopoverOpen}
                      onOpenChange={setContactPopoverOpen}
                    >
                      <PopoverTrigger asChild>
                        <FormControl>
                          <Button
                            variant="outline"
                            role="combobox"
                            className={cn(
                              "w-full justify-between",
                              !field.value && "text-muted-foreground"
                            )}
                          >
                            {field.value
                              ? contacts.find((c) => c.id === field.value)?.fullName
                              : "Select an existing contact or enter manually..."}
                            <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                          </Button>
                        </FormControl>
                      </PopoverTrigger>
                      <PopoverContent className="w-[400px] p-0" align="start">
                        <Command>
                          <CommandInput placeholder="Search contacts..." />
                          <CommandList>
                            <CommandEmpty>No contacts found.</CommandEmpty>
                            <CommandGroup>
                              <CommandItem
                                value=""
                                onSelect={() => {
                                  field.onChange("");
                                  form.setValue("contactName", "");
                                  form.setValue("email", "");
                                  form.setValue("phone", "");
                                  setContactPopoverOpen(false);
                                }}
                              >
                                <Check
                                  className={cn(
                                    "mr-2 h-4 w-4",
                                    !field.value ? "opacity-100" : "opacity-0"
                                  )}
                                />
                                <span className="text-muted-foreground">
                                  Enter manually
                                </span>
                              </CommandItem>
                              {contacts.map((c) => (
                                <CommandItem
                                  key={c.id}
                                  value={c.fullName}
                                  onSelect={() => handleExistingContactSelect(c.id)}
                                >
                                  <Check
                                    className={cn(
                                      "mr-2 h-4 w-4",
                                      c.id === field.value
                                        ? "opacity-100"
                                        : "opacity-0"
                                    )}
                                  />
                                  <div className="flex flex-col">
                                    <span>{c.fullName}</span>
                                    {c.jobTitle && (
                                      <span className="text-xs text-muted-foreground">
                                        {c.jobTitle}
                                      </span>
                                    )}
                                  </div>
                                </CommandItem>
                              ))}
                            </CommandGroup>
                          </CommandList>
                        </Command>
                      </PopoverContent>
                    </Popover>
                    <FormMessage />
                  </FormItem>
                )}
              />
            )}

            <FormField
              control={form.control}
              name="contactName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Contact Name *</FormLabel>
                  <FormControl>
                    <Input placeholder="Full name" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="email"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Email</FormLabel>
                  <FormControl>
                    <Input type="email" placeholder="email@example.com" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="phone"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Phone</FormLabel>
                  <FormControl>
                    <Input placeholder="+353 1 234 5678" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="role"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Role *</FormLabel>
                  <Select onValueChange={field.onChange} value={field.value}>
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Select a role" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {CONTACT_ROLES.map((role) => (
                        <SelectItem key={role} value={role}>
                          {role}
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
              name="isPrimary"
              render={({ field }) => (
                <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                  <FormControl>
                    <Checkbox
                      checked={field.value}
                      onCheckedChange={field.onChange}
                    />
                  </FormControl>
                  <div className="space-y-1 leading-none">
                    <FormLabel>Primary Contact</FormLabel>
                    <p className="text-sm text-muted-foreground">
                      Mark this contact as the primary contact for this proposal
                    </p>
                  </div>
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
                {isPending
                  ? "Saving..."
                  : isEditing
                  ? "Update Contact"
                  : "Add Contact"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
