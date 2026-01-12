"use client";

import * as React from "react";
import { CheckIcon, ChevronsUpDownIcon, XIcon } from "lucide-react";
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
import { usePurchaseOrdersBySupplier } from "@/lib/api/stock/use-purchase-orders";
import type { PurchaseOrder } from "@/types/stock";

interface PurchaseOrderSearchSelectProps {
  value?: string;
  supplierId?: string;
  onValueChange: (purchaseOrderId: string | undefined, purchaseOrder: PurchaseOrder | undefined) => void;
  disabled?: boolean;
  placeholder?: string;
}

export function PurchaseOrderSearchSelect({
  value,
  supplierId,
  onValueChange,
  disabled = false,
  placeholder = "Select a purchase order (optional)...",
}: PurchaseOrderSearchSelectProps) {
  const [open, setOpen] = React.useState(false);
  const { data: purchaseOrders = [], isLoading } = usePurchaseOrdersBySupplier(supplierId ?? "");

  // Filter to only confirmed or partially received POs
  const availablePOs = React.useMemo(
    () => purchaseOrders.filter((po) =>
      po.status === "Confirmed" || po.status === "PartiallyReceived"
    ),
    [purchaseOrders]
  );

  const selectedPO = React.useMemo(
    () => purchaseOrders.find((po) => po.id === value),
    [purchaseOrders, value]
  );

  const handleClear = (e: React.MouseEvent) => {
    e.stopPropagation();
    onValueChange(undefined, undefined);
  };

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          aria-expanded={open}
          className="w-full justify-between font-normal"
          disabled={disabled || isLoading || !supplierId}
        >
          {selectedPO ? (
            <span className="flex items-center gap-2 truncate">
              <span className="font-medium">{selectedPO.poNumber}</span>
              <span className="text-muted-foreground">
                ({selectedPO.totalValue.toFixed(2)})
              </span>
            </span>
          ) : (
            <span className="text-muted-foreground">
              {!supplierId ? "Select a supplier first" : placeholder}
            </span>
          )}
          <div className="flex items-center gap-1">
            {selectedPO && (
              <span
                role="button"
                tabIndex={0}
                onClick={handleClear}
                onKeyDown={(e) => e.key === 'Enter' && handleClear(e as unknown as React.MouseEvent)}
                className="h-4 w-4 rounded hover:bg-muted cursor-pointer"
              >
                <XIcon className="h-4 w-4 opacity-50 hover:opacity-100" />
              </span>
            )}
            <ChevronsUpDownIcon className="h-4 w-4 shrink-0 opacity-50" />
          </div>
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[450px] p-0" align="start">
        <Command>
          <CommandInput placeholder="Search purchase orders..." />
          <CommandList>
            <CommandEmpty>No open purchase orders found for this supplier.</CommandEmpty>
            <CommandGroup>
              {availablePOs.map((po) => (
                <CommandItem
                  key={po.id}
                  value={`${po.poNumber} ${po.supplierName}`}
                  onSelect={() => {
                    onValueChange(po.id, po);
                    setOpen(false);
                  }}
                >
                  <CheckIcon
                    className={cn(
                      "mr-2 h-4 w-4",
                      value === po.id ? "opacity-100" : "opacity-0"
                    )}
                  />
                  <div className="flex flex-col flex-1">
                    <div className="flex items-center justify-between">
                      <span className="font-medium">{po.poNumber}</span>
                      <span className="text-sm text-muted-foreground">
                        {po.totalValue.toFixed(2)}
                      </span>
                    </div>
                    <span className="text-sm text-muted-foreground">
                      {po.status} - {new Date(po.orderDate).toLocaleDateString()}
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
