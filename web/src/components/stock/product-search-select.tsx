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
import { useAllProducts } from "@/lib/api/stock/use-products";
import type { Product } from "@/types/stock";

interface ProductSearchSelectProps {
  value?: string;
  onValueChange: (productId: string, product: Product | undefined) => void;
  disabled?: boolean;
  excludeProductIds?: string[];
  placeholder?: string;
}

export function ProductSearchSelect({
  value,
  onValueChange,
  disabled = false,
  excludeProductIds = [],
  placeholder = "Select a product...",
}: ProductSearchSelectProps) {
  const [open, setOpen] = React.useState(false);
  const { data: products = [], isLoading } = useAllProducts();

  const activeProducts = React.useMemo(
    () =>
      products
        .filter((p) => p.isActive && !excludeProductIds.includes(p.id)),
    [products, excludeProductIds]
  );

  const selectedProduct = React.useMemo(
    () => products.find((p) => p.id === value),
    [products, value]
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
          {selectedProduct ? (
            <span className="truncate">
              <span className="font-medium">{selectedProduct.productCode}</span>
              {" - "}
              {selectedProduct.productName}
            </span>
          ) : (
            <span className="text-muted-foreground">{placeholder}</span>
          )}
          <ChevronsUpDownIcon className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[400px] p-0" align="start">
        <Command>
          <CommandInput placeholder="Search products..." />
          <CommandList>
            <CommandEmpty>No products found.</CommandEmpty>
            <CommandGroup>
              {activeProducts.map((product) => (
                <CommandItem
                  key={product.id}
                  value={`${product.productCode} ${product.productName}`}
                  onSelect={() => {
                    onValueChange(product.id, product);
                    setOpen(false);
                  }}
                >
                  <CheckIcon
                    className={cn(
                      "mr-2 h-4 w-4",
                      value === product.id ? "opacity-100" : "opacity-0"
                    )}
                  />
                  <div className="flex flex-col">
                    <span className="font-medium">{product.productCode}</span>
                    <span className="text-sm text-muted-foreground">
                      {product.productName}
                    </span>
                  </div>
                  <span className="ml-auto text-sm text-muted-foreground">
                    {product.baseRate.toFixed(2)}
                  </span>
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}
