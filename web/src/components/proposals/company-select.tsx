"use client";

import * as React from "react";
import { Check, ChevronsUpDown, Building2, Loader2 } from "lucide-react";
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
import { useAllCompanies } from "@/lib/api/admin/use-companies";

interface CompanySelectProps {
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
}

export function CompanySelect({ value, onChange, disabled }: CompanySelectProps) {
  const [open, setOpen] = React.useState(false);
  const { data: companies, isLoading } = useAllCompanies();

  const activeCompanies = React.useMemo(() => {
    return companies?.filter((c) => c.isActive) ?? [];
  }, [companies]);

  const selectedCompany = React.useMemo(() => {
    return activeCompanies.find((company) => company.id === value);
  }, [activeCompanies, value]);

  if (isLoading) {
    return (
      <Button variant="outline" className="w-full justify-start" disabled>
        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
        Loading companies...
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
          {selectedCompany ? (
            <span className="flex items-center gap-2 truncate">
              <Building2 className="h-4 w-4 shrink-0" />
              <span className="truncate">
                {selectedCompany.companyName}
                {selectedCompany.companyCode && (
                  <span className="text-muted-foreground ml-1">
                    ({selectedCompany.companyCode})
                  </span>
                )}
              </span>
            </span>
          ) : (
            <span className="text-muted-foreground">Select a company...</span>
          )}
          <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[400px] p-0" align="start">
        <Command>
          <CommandInput placeholder="Search companies..." />
          <CommandList>
            <CommandEmpty>No company found.</CommandEmpty>
            <CommandGroup>
              {activeCompanies.map((company) => (
                <CommandItem
                  key={company.id}
                  value={`${company.companyName} ${company.companyCode}`}
                  onSelect={() => {
                    onChange(company.id);
                    setOpen(false);
                  }}
                >
                  <Check
                    className={cn(
                      "mr-2 h-4 w-4",
                      value === company.id ? "opacity-100" : "opacity-0"
                    )}
                  />
                  <div className="flex flex-col">
                    <span className="font-medium">{company.companyName}</span>
                    {company.companyCode && (
                      <span className="text-sm text-muted-foreground">
                        {company.companyCode}
                        {company.companyType && ` - ${company.companyType}`}
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
  );
}
