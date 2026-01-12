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
import { useBayLocationsByLocation } from "@/lib/api/stock/use-bay-locations";
import type { BayLocation } from "@/types/stock";

interface BayLocationSearchSelectProps {
  value?: string;
  stockLocationId?: string;
  onValueChange: (bayLocationId: string | undefined, bayLocation: BayLocation | undefined) => void;
  disabled?: boolean;
  placeholder?: string;
}

export function BayLocationSearchSelect({
  value,
  stockLocationId,
  onValueChange,
  disabled = false,
  placeholder = "Select a bay...",
}: BayLocationSearchSelectProps) {
  const [open, setOpen] = React.useState(false);
  const { data: bayLocations = [], isLoading } = useBayLocationsByLocation(stockLocationId ?? "");

  const activeBayLocations = React.useMemo(
    () => bayLocations.filter((b) => b.isActive),
    [bayLocations]
  );

  const selectedBayLocation = React.useMemo(
    () => bayLocations.find((b) => b.id === value),
    [bayLocations, value]
  );

  const isDisabled = disabled || isLoading || !stockLocationId;

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          aria-expanded={open}
          className="w-full justify-between font-normal"
          disabled={isDisabled}
        >
          {selectedBayLocation ? (
            <span className="truncate">
              <span className="font-medium">{selectedBayLocation.bayCode}</span>
              {selectedBayLocation.bayName && ` - ${selectedBayLocation.bayName}`}
            </span>
          ) : (
            <span className="text-muted-foreground">
              {!stockLocationId ? "Select location first" : placeholder}
            </span>
          )}
          <ChevronsUpDownIcon className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[350px] p-0" align="start">
        <Command>
          <CommandInput placeholder="Search bays..." />
          <CommandList>
            <CommandEmpty>No bay locations found.</CommandEmpty>
            <CommandGroup>
              <CommandItem
                value="__none__"
                onSelect={() => {
                  onValueChange(undefined, undefined);
                  setOpen(false);
                }}
              >
                <CheckIcon
                  className={cn(
                    "mr-2 h-4 w-4",
                    !value ? "opacity-100" : "opacity-0"
                  )}
                />
                <span className="text-muted-foreground">No bay selected</span>
              </CommandItem>
              {activeBayLocations.map((bayLocation) => (
                <CommandItem
                  key={bayLocation.id}
                  value={`${bayLocation.bayCode} ${bayLocation.bayName ?? ""}`}
                  onSelect={() => {
                    onValueChange(bayLocation.id, bayLocation);
                    setOpen(false);
                  }}
                >
                  <CheckIcon
                    className={cn(
                      "mr-2 h-4 w-4",
                      value === bayLocation.id ? "opacity-100" : "opacity-0"
                    )}
                  />
                  <div className="flex flex-col">
                    <span className="font-medium">{bayLocation.bayCode}</span>
                    {bayLocation.bayName && (
                      <span className="text-sm text-muted-foreground">
                        {bayLocation.bayName}
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
