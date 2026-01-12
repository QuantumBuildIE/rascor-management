"use client";

import * as React from "react";
import { CheckIcon, ChevronsUpDownIcon } from "lucide-react";
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
import { useSuppliers } from "@/lib/api/stock/use-suppliers";
import type { Supplier } from "@/types/stock";

interface SupplierSearchSelectProps {
  value?: string;
  onValueChange: (supplierId: string, supplier: Supplier | undefined) => void;
  disabled?: boolean;
  placeholder?: string;
}

export function SupplierSearchSelect({
  value,
  onValueChange,
  disabled = false,
  placeholder = "Select a supplier...",
}: SupplierSearchSelectProps) {
  const [open, setOpen] = React.useState(false);
  const { data: suppliers = [], isLoading } = useSuppliers();

  const activeSuppliers = React.useMemo(
    () => suppliers.filter((s) => s.isActive),
    [suppliers]
  );

  const selectedSupplier = React.useMemo(
    () => suppliers.find((s) => s.id === value),
    [suppliers, value]
  );

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          aria-expanded={open}
          className="w-full justify-between font-normal"
          disabled={disabled || isLoading}
        >
          {selectedSupplier ? (
            <span className="truncate">
              <span className="font-medium">{selectedSupplier.supplierCode}</span>
              {" - "}
              {selectedSupplier.supplierName}
            </span>
          ) : (
            <span className="text-muted-foreground">{placeholder}</span>
          )}
          <ChevronsUpDownIcon className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[400px] p-0" align="start">
        <Command>
          <CommandInput placeholder="Search suppliers..." />
          <CommandList>
            <CommandEmpty>No suppliers found.</CommandEmpty>
            <CommandGroup>
              {activeSuppliers.map((supplier) => (
                <CommandItem
                  key={supplier.id}
                  value={`${supplier.supplierCode} ${supplier.supplierName}`}
                  onSelect={() => {
                    onValueChange(supplier.id, supplier);
                    setOpen(false);
                  }}
                >
                  <CheckIcon
                    className={cn(
                      "mr-2 h-4 w-4",
                      value === supplier.id ? "opacity-100" : "opacity-0"
                    )}
                  />
                  <div className="flex flex-col">
                    <span className="font-medium">{supplier.supplierCode}</span>
                    <span className="text-sm text-muted-foreground">
                      {supplier.supplierName}
                    </span>
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
