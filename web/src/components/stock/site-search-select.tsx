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
import { useAllSites } from "@/lib/api/admin/use-sites";
import type { Site } from "@/types/admin";

interface SiteSearchSelectProps {
  value?: string;
  onValueChange: (siteId: string, site: Site | undefined) => void;
  disabled?: boolean;
  placeholder?: string;
}

export function SiteSearchSelect({
  value,
  onValueChange,
  disabled = false,
  placeholder = "Select a site...",
}: SiteSearchSelectProps) {
  const [open, setOpen] = React.useState(false);
  const { data: sites = [], isLoading } = useAllSites();

  const activeSites = React.useMemo(
    () => sites.filter((s) => s.isActive),
    [sites]
  );

  const selectedSite = React.useMemo(
    () => sites.find((s) => s.id === value),
    [sites, value]
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
          {selectedSite ? (
            <span className="truncate">
              <span className="font-medium">{selectedSite.siteCode}</span>
              {" - "}
              {selectedSite.siteName}
            </span>
          ) : (
            <span className="text-muted-foreground">{placeholder}</span>
          )}
          <ChevronsUpDownIcon className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[400px] p-0" align="start">
        <Command>
          <CommandInput placeholder="Search sites..." />
          <CommandList>
            <CommandEmpty>No sites found.</CommandEmpty>
            <CommandGroup>
              {activeSites.map((site) => (
                <CommandItem
                  key={site.id}
                  value={`${site.siteCode} ${site.siteName}`}
                  onSelect={() => {
                    onValueChange(site.id, site);
                    setOpen(false);
                  }}
                >
                  <CheckIcon
                    className={cn(
                      "mr-2 h-4 w-4",
                      value === site.id ? "opacity-100" : "opacity-0"
                    )}
                  />
                  <div className="flex flex-col">
                    <span className="font-medium">{site.siteCode}</span>
                    <span className="text-sm text-muted-foreground">
                      {site.siteName}
                    </span>
                  </div>
                  {site.city && (
                    <span className="ml-auto text-sm text-muted-foreground">
                      {site.city}
                    </span>
                  )}
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}
