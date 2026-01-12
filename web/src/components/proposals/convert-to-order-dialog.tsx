"use client";

import * as React from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { format } from "date-fns";
import {
  CalendarIcon,
  Package,
  AlertTriangle,
  CheckCircle2,
  Info,
  ExternalLink,
} from "lucide-react";
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
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { Calendar } from "@/components/ui/calendar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
import { cn } from "@/lib/utils";
import {
  usePreviewConversion,
  useConvertToStockOrders,
  type Proposal,
  type ConversionMode,
  type ConversionPreview,
} from "@/lib/api/proposals";
import { useAllSites } from "@/lib/api/admin/use-sites";
import { useLocations } from "@/lib/api/stock/use-locations";
import { toast } from "sonner";

const formSchema = z.object({
  siteId: z.string().min(1, "Please select a delivery site"),
  sourceLocationId: z.string().min(1, "Please select a source warehouse"),
  requiredDate: z.date().optional(),
  notes: z.string().optional(),
  mode: z.enum(["AllItems", "SelectedSections", "SelectedItems"]),
  selectedSectionIds: z.array(z.string()).optional(),
  selectedLineItemIds: z.array(z.string()).optional(),
});

type FormValues = z.infer<typeof formSchema>;

interface ConvertToOrderDialogProps {
  proposal: Proposal;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

type DialogStep = "configure" | "preview" | "success";

export function ConvertToOrderDialog({
  proposal,
  open,
  onOpenChange,
}: ConvertToOrderDialogProps) {
  const router = useRouter();
  const [step, setStep] = React.useState<DialogStep>("configure");
  const [preview, setPreview] = React.useState<ConversionPreview | null>(null);
  const [createdOrderId, setCreatedOrderId] = React.useState<string | null>(null);
  const [createdOrderNumber, setCreatedOrderNumber] = React.useState<string>("");

  // Data fetching
  const { data: sites = [], isLoading: sitesLoading } = useAllSites();
  const { data: locations = [], isLoading: locationsLoading } = useLocations();

  // Mutations
  const previewMutation = usePreviewConversion();
  const convertMutation = useConvertToStockOrders();

  // Form setup
  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      siteId: "",
      sourceLocationId: "",
      requiredDate: undefined,
      notes: "",
      mode: "AllItems",
      selectedSectionIds: [],
      selectedLineItemIds: [],
    },
  });

  const mode = form.watch("mode");
  const selectedSectionIds = form.watch("selectedSectionIds") || [];
  const selectedLineItemIds = form.watch("selectedLineItemIds") || [];

  // Reset dialog state when closed
  React.useEffect(() => {
    if (!open) {
      setStep("configure");
      setPreview(null);
      setCreatedOrderId(null);
      setCreatedOrderNumber("");
      form.reset();
    }
  }, [open, form]);

  // Filter to only warehouse locations
  const warehouseLocations = React.useMemo(() => {
    return locations.filter(
      (loc) => loc.locationType === "Warehouse" && loc.isActive
    );
  }, [locations]);

  // Filter to only active sites
  const activeSites = React.useMemo(() => {
    return sites.filter((site) => site.isActive);
  }, [sites]);

  // Format currency
  const formatCurrency = (value: number, currency: string = "EUR") => {
    return value.toLocaleString("en-IE", {
      style: "currency",
      currency,
    });
  };

  // Handle preview
  const handlePreview = async () => {
    const values = form.getValues();

    // Validate form
    const isValid = await form.trigger();
    if (!isValid) return;

    try {
      const result = await previewMutation.mutateAsync({
        proposalId: proposal.id,
        data: {
          siteId: values.siteId,
          sourceLocationId: values.sourceLocationId,
          requiredDate: values.requiredDate?.toISOString(),
          notes: values.notes,
          mode: values.mode as ConversionMode,
          selectedSectionIds:
            values.mode === "SelectedSections" ? values.selectedSectionIds : undefined,
          selectedLineItemIds:
            values.mode === "SelectedItems" ? values.selectedLineItemIds : undefined,
        },
      });
      setPreview(result);
      setStep("preview");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to preview conversion", { description: message });
    }
  };

  // Handle conversion
  const handleConvert = async () => {
    const values = form.getValues();

    try {
      const result = await convertMutation.mutateAsync({
        proposalId: proposal.id,
        data: {
          siteId: values.siteId,
          sourceLocationId: values.sourceLocationId,
          requiredDate: values.requiredDate?.toISOString(),
          notes: values.notes,
          mode: values.mode as ConversionMode,
          selectedSectionIds:
            values.mode === "SelectedSections" ? values.selectedSectionIds : undefined,
          selectedLineItemIds:
            values.mode === "SelectedItems" ? values.selectedLineItemIds : undefined,
        },
      });

      if (result.success && result.createdOrders.length > 0) {
        const order = result.createdOrders[0];
        setCreatedOrderId(order.stockOrderId);
        setCreatedOrderNumber(order.orderNumber);
        setStep("success");

        // Show warnings if any
        if (result.warnings.length > 0) {
          result.warnings.forEach((warning) => {
            toast.warning(warning);
          });
        }

        toast.success("Stock order created successfully", {
          description: `Order ${order.orderNumber} created with ${order.itemCount} items`,
        });
      } else {
        toast.error("Failed to create stock order", {
          description: result.errorMessage || "Unknown error",
        });
      }
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to convert to stock order", { description: message });
    }
  };

  // Toggle section selection
  const toggleSection = (sectionId: string) => {
    const current = form.getValues("selectedSectionIds") || [];
    if (current.includes(sectionId)) {
      form.setValue(
        "selectedSectionIds",
        current.filter((id) => id !== sectionId)
      );
    } else {
      form.setValue("selectedSectionIds", [...current, sectionId]);
    }
  };

  // Toggle line item selection
  const toggleLineItem = (lineItemId: string) => {
    const current = form.getValues("selectedLineItemIds") || [];
    if (current.includes(lineItemId)) {
      form.setValue(
        "selectedLineItemIds",
        current.filter((id) => id !== lineItemId)
      );
    } else {
      form.setValue("selectedLineItemIds", [...current, lineItemId]);
    }
  };

  // Render configuration step
  const renderConfigureStep = () => (
    <Form {...form}>
      <form className="space-y-4">
        {/* Site Selection */}
        <FormField
          control={form.control}
          name="siteId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Delivery Site *</FormLabel>
              <Select
                onValueChange={field.onChange}
                defaultValue={field.value}
                disabled={sitesLoading}
              >
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder="Select delivery site" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  {activeSites.map((site) => (
                    <SelectItem key={site.id} value={site.id}>
                      {site.siteName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <FormDescription>
                Where the stock will be delivered
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        {/* Source Location Selection */}
        <FormField
          control={form.control}
          name="sourceLocationId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Source Warehouse *</FormLabel>
              <Select
                onValueChange={field.onChange}
                defaultValue={field.value}
                disabled={locationsLoading}
              >
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder="Select source warehouse" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  {warehouseLocations.map((location) => (
                    <SelectItem key={location.id} value={location.id}>
                      {location.locationName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <FormDescription>
                Where the stock will be picked from
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        {/* Required Date */}
        <FormField
          control={form.control}
          name="requiredDate"
          render={({ field }) => (
            <FormItem className="flex flex-col">
              <FormLabel>Required Date</FormLabel>
              <Popover>
                <PopoverTrigger asChild>
                  <FormControl>
                    <Button
                      variant="outline"
                      className={cn(
                        "w-full pl-3 text-left font-normal",
                        !field.value && "text-muted-foreground"
                      )}
                    >
                      {field.value ? (
                        format(field.value, "PPP")
                      ) : (
                        <span>Pick a date</span>
                      )}
                      <CalendarIcon className="ml-auto h-4 w-4 opacity-50" />
                    </Button>
                  </FormControl>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0" align="start">
                  <Calendar
                    mode="single"
                    selected={field.value}
                    onSelect={field.onChange}
                    disabled={(date) =>
                      date < new Date(new Date().setHours(0, 0, 0, 0))
                    }
                    initialFocus
                  />
                </PopoverContent>
              </Popover>
              <FormDescription>
                When the stock is needed (optional)
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        {/* Conversion Mode */}
        <FormField
          control={form.control}
          name="mode"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Items to Convert</FormLabel>
              <Select onValueChange={field.onChange} defaultValue={field.value}>
                <FormControl>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  <SelectItem value="AllItems">All Items</SelectItem>
                  <SelectItem value="SelectedSections">Selected Sections</SelectItem>
                  <SelectItem value="SelectedItems">Selected Items</SelectItem>
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />

        {/* Section Selection */}
        {mode === "SelectedSections" && (
          <div className="space-y-2">
            <FormLabel>Select Sections</FormLabel>
            <div className="rounded-md border p-4 space-y-2">
              {proposal.sections.map((section) => (
                <div key={section.id} className="flex items-center space-x-2">
                  <Checkbox
                    id={`section-${section.id}`}
                    checked={selectedSectionIds.includes(section.id)}
                    onCheckedChange={() => toggleSection(section.id)}
                  />
                  <label
                    htmlFor={`section-${section.id}`}
                    className="text-sm font-medium leading-none cursor-pointer flex-1"
                  >
                    {section.sectionName}
                    <span className="text-muted-foreground ml-2">
                      ({section.lineItems.length} items)
                    </span>
                  </label>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Line Item Selection */}
        {mode === "SelectedItems" && (
          <div className="space-y-2">
            <FormLabel>Select Items</FormLabel>
            <div className="rounded-md border p-4 max-h-64 overflow-y-auto space-y-4">
              {proposal.sections.map((section) => (
                <div key={section.id} className="space-y-2">
                  <div className="font-medium text-sm">{section.sectionName}</div>
                  {section.lineItems.map((item) => (
                    <div key={item.id} className="flex items-center space-x-2 pl-4">
                      <Checkbox
                        id={`item-${item.id}`}
                        checked={selectedLineItemIds.includes(item.id)}
                        onCheckedChange={() => toggleLineItem(item.id)}
                      />
                      <label
                        htmlFor={`item-${item.id}`}
                        className="text-sm leading-none cursor-pointer flex-1"
                      >
                        <span className="font-medium">{item.productCode || "Ad-hoc"}</span>
                        {" - "}
                        {item.description}
                        <span className="text-muted-foreground ml-2">
                          (Qty: {item.quantity})
                        </span>
                        {!item.productId && (
                          <Badge variant="outline" className="ml-2 text-xs">
                            Ad-hoc
                          </Badge>
                        )}
                      </label>
                    </div>
                  ))}
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Notes */}
        <FormField
          control={form.control}
          name="notes"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Notes</FormLabel>
              <FormControl>
                <Textarea
                  placeholder="Any additional notes for the stock order..."
                  className="resize-none"
                  rows={3}
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
      </form>
    </Form>
  );

  // Render preview step
  const renderPreviewStep = () => {
    if (!preview) return null;

    return (
      <div className="space-y-4">
        {/* Summary */}
        <div className="grid grid-cols-3 gap-4 p-4 bg-muted rounded-lg">
          <div>
            <div className="text-sm text-muted-foreground">Total Items</div>
            <div className="text-2xl font-bold">{preview.totalItems}</div>
          </div>
          <div>
            <div className="text-sm text-muted-foreground">Total Quantity</div>
            <div className="text-2xl font-bold">{preview.totalQuantity}</div>
          </div>
          <div>
            <div className="text-sm text-muted-foreground">Total Value</div>
            <div className="text-2xl font-bold">
              {formatCurrency(preview.totalValue, proposal.currency)}
            </div>
          </div>
        </div>

        {/* Warnings */}
        {preview.hasStockWarnings && (
          <div className="flex items-start gap-2 p-3 bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg">
            <AlertTriangle className="h-5 w-5 text-yellow-600 dark:text-yellow-500 mt-0.5" />
            <div className="text-sm">
              <div className="font-medium text-yellow-800 dark:text-yellow-400">
                Insufficient Stock Warning
              </div>
              <div className="text-yellow-700 dark:text-yellow-500">
                Some items have insufficient stock at the selected warehouse. The order will still be created.
              </div>
            </div>
          </div>
        )}

        {preview.hasAdHocItems && (
          <div className="flex items-start gap-2 p-3 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg">
            <Info className="h-5 w-5 text-blue-600 dark:text-blue-500 mt-0.5" />
            <div className="text-sm">
              <div className="font-medium text-blue-800 dark:text-blue-400">
                Ad-hoc Items
              </div>
              <div className="text-blue-700 dark:text-blue-500">
                Ad-hoc items (no linked product) will be skipped.
              </div>
            </div>
          </div>
        )}

        {/* Items Table */}
        <div className="border rounded-lg overflow-hidden max-h-64 overflow-y-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Product</TableHead>
                <TableHead>Description</TableHead>
                <TableHead className="text-right">Qty</TableHead>
                <TableHead className="text-right">Available</TableHead>
                <TableHead className="text-center">Status</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {preview.items.map((item, index) => (
                <TableRow key={index}>
                  <TableCell className="font-medium">
                    {item.productCode || "-"}
                  </TableCell>
                  <TableCell className="max-w-[200px] truncate">
                    {item.description}
                  </TableCell>
                  <TableCell className="text-right">{item.quantity}</TableCell>
                  <TableCell className="text-right">
                    {item.isAdHocItem ? "-" : item.availableStock}
                  </TableCell>
                  <TableCell className="text-center">
                    {item.isAdHocItem ? (
                      <Badge variant="outline" className="text-xs">
                        <Info className="h-3 w-3 mr-1" />
                        Skipped
                      </Badge>
                    ) : item.hasSufficientStock ? (
                      <Badge variant="default" className="text-xs bg-green-600">
                        <CheckCircle2 className="h-3 w-3 mr-1" />
                        OK
                      </Badge>
                    ) : (
                      <Badge variant="destructive" className="text-xs">
                        <AlertTriangle className="h-3 w-3 mr-1" />
                        Low Stock
                      </Badge>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      </div>
    );
  };

  // Render success step
  const renderSuccessStep = () => (
    <div className="text-center py-8 space-y-4">
      <div className="mx-auto w-16 h-16 rounded-full bg-green-100 dark:bg-green-900/30 flex items-center justify-center">
        <CheckCircle2 className="h-8 w-8 text-green-600 dark:text-green-500" />
      </div>
      <div>
        <h3 className="text-lg font-medium">Stock Order Created</h3>
        <p className="text-muted-foreground">
          Order <strong>{createdOrderNumber}</strong> has been created successfully.
        </p>
      </div>
      <Button
        variant="outline"
        onClick={() => {
          onOpenChange(false);
          router.push(`/stock/orders/${createdOrderId}`);
        }}
      >
        <ExternalLink className="h-4 w-4 mr-2" />
        View Stock Order
      </Button>
    </div>
  );

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Package className="h-5 w-5" />
            {step === "success"
              ? "Conversion Complete"
              : step === "preview"
              ? "Preview Conversion"
              : "Convert to Stock Order"}
          </DialogTitle>
          {step === "configure" && (
            <DialogDescription>
              Create a stock order from proposal {proposal.proposalNumber} v
              {proposal.version}
            </DialogDescription>
          )}
        </DialogHeader>

        {step === "configure" && renderConfigureStep()}
        {step === "preview" && renderPreviewStep()}
        {step === "success" && renderSuccessStep()}

        {step !== "success" && (
          <DialogFooter>
            {step === "preview" && (
              <Button
                variant="outline"
                onClick={() => setStep("configure")}
                disabled={convertMutation.isPending}
              >
                Back
              </Button>
            )}
            <Button
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={previewMutation.isPending || convertMutation.isPending}
            >
              Cancel
            </Button>
            {step === "configure" && (
              <Button
                onClick={handlePreview}
                disabled={previewMutation.isPending}
              >
                {previewMutation.isPending ? (
                  <>
                    <span className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-primary-foreground border-r-transparent" />
                    Loading Preview...
                  </>
                ) : (
                  "Preview"
                )}
              </Button>
            )}
            {step === "preview" && (
              <Button
                onClick={handleConvert}
                disabled={convertMutation.isPending || (preview !== null && preview.items.filter(i => !i.isAdHocItem).length === 0)}
              >
                {convertMutation.isPending ? (
                  <>
                    <span className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-primary-foreground border-r-transparent" />
                    Creating Order...
                  </>
                ) : (
                  "Create Stock Order"
                )}
              </Button>
            )}
          </DialogFooter>
        )}
      </DialogContent>
    </Dialog>
  );
}
