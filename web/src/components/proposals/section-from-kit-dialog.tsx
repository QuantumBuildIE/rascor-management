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
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { Check, ChevronsUpDown } from "lucide-react";
import { useProductKits, useProductKit } from "@/lib/api/stock/use-product-kits";
import { useAddSection } from "@/lib/api/proposals";
import { toast } from "sonner";

const sectionFromKitSchema = z.object({
  kitId: z.string().min(1, "Please select a kit"),
  sectionName: z.string().min(1, "Section name is required"),
  sortOrder: z.number().min(0, "Sort order must be 0 or greater"),
});

type SectionFromKitFormValues = z.infer<typeof sectionFromKitSchema>;

interface SectionFromKitDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  proposalId: string;
  nextSortOrder?: number;
}

export function SectionFromKitDialog({
  open,
  onOpenChange,
  proposalId,
  nextSortOrder = 0,
}: SectionFromKitDialogProps) {
  const [kitPopoverOpen, setKitPopoverOpen] = React.useState(false);
  const [selectedKitId, setSelectedKitId] = React.useState<string | null>(null);

  const { data: kitsData } = useProductKits({ isActive: true, pageSize: 100 });
  const { data: selectedKit } = useProductKit(selectedKitId ?? "");
  const addSection = useAddSection();

  const kits = kitsData?.items ?? [];

  const form = useForm<SectionFromKitFormValues>({
    resolver: zodResolver(sectionFromKitSchema),
    defaultValues: {
      kitId: "",
      sectionName: "",
      sortOrder: nextSortOrder,
    },
  });

  React.useEffect(() => {
    if (open) {
      setSelectedKitId(null);
      form.reset({
        kitId: "",
        sectionName: "",
        sortOrder: nextSortOrder,
      });
    }
  }, [open, nextSortOrder, form]);

  // Update section name when kit is selected
  React.useEffect(() => {
    if (selectedKit) {
      form.setValue("sectionName", selectedKit.kitName);
    }
  }, [selectedKit, form]);

  const onSubmit = async (values: SectionFromKitFormValues) => {
    try {
      await addSection.mutateAsync({
        proposalId,
        sourceKitId: values.kitId,
        sectionName: values.sectionName,
        description: selectedKit?.description,
        sortOrder: values.sortOrder,
      });
      toast.success("Section added from kit successfully");
      onOpenChange(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to add section", { description: message });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Add Section from Kit</DialogTitle>
          <DialogDescription>
            Select a product kit to create a section with all its items
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="kitId"
              render={({ field }) => (
                <FormItem className="flex flex-col">
                  <FormLabel>Product Kit *</FormLabel>
                  <Popover open={kitPopoverOpen} onOpenChange={setKitPopoverOpen}>
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
                            ? kits.find((kit) => kit.id === field.value)?.kitName
                            : "Select a kit..."}
                          <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                        </Button>
                      </FormControl>
                    </PopoverTrigger>
                    <PopoverContent className="w-[400px] p-0" align="start">
                      <Command>
                        <CommandInput placeholder="Search kits..." />
                        <CommandList>
                          <CommandEmpty>No kits found.</CommandEmpty>
                          <CommandGroup>
                            {kits.map((kit) => (
                              <CommandItem
                                key={kit.id}
                                value={kit.kitName}
                                onSelect={() => {
                                  field.onChange(kit.id);
                                  setSelectedKitId(kit.id);
                                  setKitPopoverOpen(false);
                                }}
                              >
                                <Check
                                  className={cn(
                                    "mr-2 h-4 w-4",
                                    kit.id === field.value
                                      ? "opacity-100"
                                      : "opacity-0"
                                  )}
                                />
                                <div className="flex flex-col">
                                  <span>{kit.kitName}</span>
                                  <span className="text-xs text-muted-foreground">
                                    {kit.kitCode} - {kit.itemCount} items
                                  </span>
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

            {selectedKit && (
              <div className="rounded-md border p-4 space-y-2">
                <div className="font-medium">{selectedKit.kitName}</div>
                {selectedKit.description && (
                  <p className="text-sm text-muted-foreground">
                    {selectedKit.description}
                  </p>
                )}
                <div className="flex gap-4 text-sm">
                  <span>
                    <span className="text-muted-foreground">Items:</span>{" "}
                    {selectedKit.itemCount}
                  </span>
                  <span>
                    <span className="text-muted-foreground">Total Price:</span>{" "}
                    {selectedKit.totalPrice.toLocaleString("en-IE", {
                      style: "currency",
                      currency: "EUR",
                    })}
                  </span>
                </div>
              </div>
            )}

            <FormField
              control={form.control}
              name="sectionName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Section Name *</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="Section name (pre-filled from kit)"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="sortOrder"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Sort Order</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      min={0}
                      value={field.value}
                      onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="rounded-md bg-muted p-3 text-sm text-muted-foreground">
              This will create a section with all items from the selected kit.
            </div>

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={addSection.isPending}>
                {addSection.isPending ? "Adding..." : "Add Section"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
