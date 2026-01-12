"use client";

import * as React from "react";
import { Check, ChevronsUpDown, User, Loader2 } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
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
import { useCompanyContacts } from "@/lib/api/admin/use-contacts";

interface ContactSelectProps {
  companyId: string;
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
}

export function ContactSelect({
  companyId,
  value,
  onChange,
  disabled,
}: ContactSelectProps) {
  const [open, setOpen] = React.useState(false);
  const { data: contacts, isLoading } = useCompanyContacts(companyId);

  const activeContacts = React.useMemo(() => {
    return contacts?.filter((c) => c.isActive) ?? [];
  }, [contacts]);

  const selectedContact = React.useMemo(() => {
    return activeContacts.find((contact) => contact.id === value);
  }, [activeContacts, value]);

  if (!companyId) {
    return (
      <Button variant="outline" className="w-full justify-start" disabled>
        <span className="text-muted-foreground">Select a company first</span>
      </Button>
    );
  }

  if (isLoading) {
    return (
      <Button variant="outline" className="w-full justify-start" disabled>
        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
        Loading contacts...
      </Button>
    );
  }

  if (activeContacts.length === 0) {
    return (
      <Button variant="outline" className="w-full justify-start" disabled>
        <span className="text-muted-foreground">No contacts for this company</span>
      </Button>
    );
  }

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          aria-expanded={open}
          className="w-full justify-between"
          disabled={disabled}
        >
          {selectedContact ? (
            <span className="flex items-center gap-2 truncate">
              <User className="h-4 w-4 shrink-0" />
              <span className="truncate">
                {selectedContact.fullName}
                {selectedContact.jobTitle && (
                  <span className="text-muted-foreground ml-1">
                    ({selectedContact.jobTitle})
                  </span>
                )}
              </span>
            </span>
          ) : (
            <span className="text-muted-foreground">Select a contact...</span>
          )}
          <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[400px] p-0" align="start">
        <Command>
          <CommandInput placeholder="Search contacts..." />
          <CommandList>
            <CommandEmpty>No contact found.</CommandEmpty>
            <CommandGroup>
              {/* Option to clear selection */}
              <CommandItem
                value="__none__"
                onSelect={() => {
                  onChange("");
                  setOpen(false);
                }}
              >
                <Check
                  className={cn(
                    "mr-2 h-4 w-4",
                    !value ? "opacity-100" : "opacity-0"
                  )}
                />
                <span className="text-muted-foreground">No contact selected</span>
              </CommandItem>
              {activeContacts.map((contact) => (
                <CommandItem
                  key={contact.id}
                  value={`${contact.fullName} ${contact.email || ""} ${contact.jobTitle || ""}`}
                  onSelect={() => {
                    onChange(contact.id);
                    setOpen(false);
                  }}
                >
                  <Check
                    className={cn(
                      "mr-2 h-4 w-4",
                      value === contact.id ? "opacity-100" : "opacity-0"
                    )}
                  />
                  <div className="flex flex-col">
                    <div className="flex items-center gap-2">
                      <span className="font-medium">{contact.fullName}</span>
                      {contact.isPrimaryContact && (
                        <span className="text-xs bg-primary/10 text-primary px-1.5 py-0.5 rounded">
                          Primary
                        </span>
                      )}
                    </div>
                    <div className="text-sm text-muted-foreground">
                      {contact.jobTitle && <span>{contact.jobTitle}</span>}
                      {contact.jobTitle && contact.email && <span> - </span>}
                      {contact.email && <span>{contact.email}</span>}
                    </div>
                  </div>
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}
