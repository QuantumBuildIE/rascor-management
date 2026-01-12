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
import { useLocations } from "@/lib/api/stock/use-locations";
import type { StockLocation } from "@/types/stock";

interface LocationSearchSelectProps {
  value?: string;
  onValueChange: (locationId: string, location: StockLocation | undefined) => void;
  disabled?: boolean;
  placeholder?: string;
}

export function LocationSearchSelect({
  value,
  onValueChange,
  disabled = false,
  placeholder = "Select a location...",
}: LocationSearchSelectProps) {
  const [open, setOpen] = React.useState(false);
  const { data: locations = [], isLoading } = useLocations();

  const activeLocations = React.useMemo(
    () => locations.filter((l) => l.isActive),
    [locations]
  );

  const selectedLocation = React.useMemo(
    () => locations.find((l) => l.id === value),
    [locations, value]
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
          {selectedLocation ? (
            <span className="truncate">
              <span className="font-medium">{selectedLocation.locationCode}</span>
              {" - "}
              {selectedLocation.locationName}
            </span>
          ) : (
            <span className="text-muted-foreground">{placeholder}</span>
          )}
          <ChevronsUpDownIcon className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[400px] p-0" align="start">
        <Command>
          <CommandInput placeholder="Search locations..." />
          <CommandList>
            <CommandEmpty>No locations found.</CommandEmpty>
            <CommandGroup>
              {activeLocations.map((location) => (
                <CommandItem
                  key={location.id}
                  value={`${location.locationCode} ${location.locationName}`}
                  onSelect={() => {
                    onValueChange(location.id, location);
                    setOpen(false);
                  }}
                >
                  <CheckIcon
                    className={cn(
                      "mr-2 h-4 w-4",
                      value === location.id ? "opacity-100" : "opacity-0"
                    )}
                  />
                  <div className="flex flex-col">
                    <span className="font-medium">{location.locationCode}</span>
                    <span className="text-sm text-muted-foreground">
                      {location.locationName} ({location.locationType})
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
