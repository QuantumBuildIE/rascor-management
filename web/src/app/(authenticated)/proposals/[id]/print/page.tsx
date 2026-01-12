"use client";

import * as React from "react";
import { useParams } from "next/navigation";
import { format } from "date-fns";
import { Printer, Download, ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  useProposal,
  downloadProposalPdf,
  type ProposalSection,
  type ProposalLineItem,
} from "@/lib/api/proposals";
import { useHasAnyPermission } from "@/lib/auth/use-auth";
import { toast } from "sonner";
import Link from "next/link";

export default function ProposalPrintPage() {
  const params = useParams();
  const id = params.id as string;

  // Permissions
  const canViewCostings = useHasAnyPermission([
    "Proposals.ViewCostings",
    "Proposals.Admin",
  ]);

  // State
  const [showCosting, setShowCosting] = React.useState(false);
  const [isDownloading, setIsDownloading] = React.useState(false);

  // Data fetching
  const { data: proposal, isLoading, error } = useProposal(id, canViewCostings);

  // Format currency
  const formatCurrency = (value: number, currency: string = "EUR") => {
    return value.toLocaleString("en-IE", {
      style: "currency",
      currency,
    });
  };

  // Handle print
  const handlePrint = () => {
    window.print();
  };

  // Handle PDF download
  const handleDownloadPdf = async () => {
    if (!proposal) return;
    setIsDownloading(true);
    try {
      await downloadProposalPdf(
        proposal.id,
        proposal.proposalNumber,
        proposal.version,
        showCosting
      );
      toast.success("PDF downloaded successfully");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to download PDF", { description: message });
    } finally {
      setIsDownloading(false);
    }
  };

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="text-muted-foreground">Loading proposal...</div>
      </div>
    );
  }

  if (error || !proposal) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="text-destructive">Failed to load proposal</div>
      </div>
    );
  }

  // Company details (hardcoded for now)
  const company = {
    name: "RASCOR Ireland",
    addressLine1: "Unit 1, Rascor Business Park",
    addressLine2: "Dublin, Ireland",
    phone: "+353 1 XXX XXXX",
    email: "info@rascor.ie",
    vatNumber: "IE1234567X",
    regNumber: "123456",
  };

  // Get primary contact details
  const primaryContact = proposal.contacts.find((c) => c.isPrimary);

  return (
    <>
      {/* Print controls - hidden when printing */}
      <div className="fixed left-0 right-0 top-0 z-50 flex items-center justify-between border-b bg-background px-4 py-2 print:hidden">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href={`/proposals/${proposal.id}`}>
              <ArrowLeft className="mr-2 h-4 w-4" />
              Back to Proposal
            </Link>
          </Button>
          <span className="text-sm text-muted-foreground">
            {proposal.proposalNumber} v{proposal.version}
          </span>
        </div>
        <div className="flex items-center gap-2">
          {canViewCostings && (
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={showCosting}
                onChange={(e) => setShowCosting(e.target.checked)}
                className="h-4 w-4 rounded border-gray-300"
              />
              Include costing
            </label>
          )}
          <Button variant="outline" onClick={handleDownloadPdf} disabled={isDownloading}>
            <Download className="mr-2 h-4 w-4" />
            {isDownloading ? "Generating..." : "Download PDF"}
          </Button>
          <Button onClick={handlePrint}>
            <Printer className="mr-2 h-4 w-4" />
            Print
          </Button>
        </div>
      </div>

      {/* Print content */}
      <div className="mx-auto max-w-[210mm] bg-white px-10 pb-10 pt-16 print:max-w-none print:p-0 print:pt-0">
        <style jsx global>{`
          @media print {
            @page {
              size: A4;
              margin: 15mm;
            }
            body {
              -webkit-print-color-adjust: exact;
              print-color-adjust: exact;
            }
            .page-break {
              page-break-before: always;
            }
          }
        `}</style>

        {/* Header */}
        <div className="mb-6 border-b-2 border-[#1e3a5f] pb-4">
          <div className="flex items-start justify-between">
            <div>
              <h1 className="text-xl font-bold text-[#1e3a5f]">{company.name}</h1>
              <p className="text-sm text-gray-500">{company.addressLine1}</p>
              <p className="text-sm text-gray-500">{company.addressLine2}</p>
              <p className="mt-1 text-xs text-gray-500">
                Phone: {company.phone} | Email: {company.email}
              </p>
            </div>
            <div className="text-right">
              <h2 className="text-2xl font-bold text-[#1e3a5f]">PROPOSAL</h2>
            </div>
          </div>
          <div className="mt-4 flex justify-between text-sm">
            <div>
              <p>
                <span className="text-gray-500">Proposal No: </span>
                <strong>{proposal.proposalNumber}</strong>
              </p>
              <p>
                <span className="text-gray-500">Version: </span>
                <strong>{proposal.version}</strong>
              </p>
            </div>
            <div className="text-right">
              <p>
                <span className="text-gray-500">Date: </span>
                <strong>{format(new Date(proposal.proposalDate), "dd MMM yyyy")}</strong>
              </p>
              {proposal.validUntilDate && (
                <p>
                  <span className="text-gray-500">Valid Until: </span>
                  <strong>{format(new Date(proposal.validUntilDate), "dd MMM yyyy")}</strong>
                </p>
              )}
            </div>
          </div>
        </div>

        {/* Client & Project */}
        <div className="mb-6 grid grid-cols-2 gap-4">
          <div className="rounded border border-gray-200 p-4">
            <h3 className="mb-2 text-xs font-bold uppercase text-[#1e3a5f]">TO:</h3>
            <p className="font-semibold">{proposal.companyName}</p>
            {proposal.primaryContactName && (
              <p className="text-sm">Attn: {proposal.primaryContactName}</p>
            )}
            {primaryContact?.email && <p className="text-xs">{primaryContact.email}</p>}
            {primaryContact?.phone && <p className="text-xs">{primaryContact.phone}</p>}
          </div>
          <div className="rounded border border-gray-200 p-4">
            <h3 className="mb-2 text-xs font-bold uppercase text-[#1e3a5f]">PROJECT:</h3>
            <p className="font-semibold">{proposal.projectName}</p>
            {proposal.projectAddress && <p className="text-sm">{proposal.projectAddress}</p>}
            {proposal.projectDescription && (
              <p className="mt-1 text-xs text-gray-600">{proposal.projectDescription}</p>
            )}
          </div>
        </div>

        {/* Sections & Line Items */}
        {proposal.sections
          .sort((a, b) => a.sortOrder - b.sortOrder)
          .map((section: ProposalSection) => (
            <div key={section.id} className="mb-6">
              {/* Section header */}
              <div className="mb-2 rounded-t bg-gray-100 p-2">
                <h3 className="font-bold text-[#1e3a5f]">{section.sectionName.toUpperCase()}</h3>
                {section.description && (
                  <p className="text-xs text-gray-600">{section.description}</p>
                )}
              </div>

              {/* Line items table */}
              {section.lineItems.length > 0 && (
                <table className="w-full border-collapse text-sm">
                  <thead>
                    <tr className="bg-[#1e3a5f] text-white">
                      <th className="border px-2 py-1 text-left">Item</th>
                      <th className="border px-2 py-1 text-left">Description</th>
                      <th className="border px-2 py-1 text-right">Qty</th>
                      <th className="border px-2 py-1 text-left">Unit</th>
                      {showCosting && (
                        <th className="border px-2 py-1 text-right">Unit Cost</th>
                      )}
                      <th className="border px-2 py-1 text-right">Unit Price</th>
                      {showCosting && (
                        <>
                          <th className="border px-2 py-1 text-right">Line Cost</th>
                          <th className="border px-2 py-1 text-right">Margin</th>
                        </>
                      )}
                      <th className="border px-2 py-1 text-right">Total</th>
                    </tr>
                  </thead>
                  <tbody>
                    {section.lineItems
                      .sort((a, b) => a.sortOrder - b.sortOrder)
                      .map((item: ProposalLineItem, idx: number) => (
                        <tr
                          key={item.id}
                          className={idx % 2 === 0 ? "bg-white" : "bg-gray-50"}
                        >
                          <td className="border px-2 py-1 text-xs">
                            {item.productCode || "-"}
                          </td>
                          <td className="border px-2 py-1">{item.description}</td>
                          <td className="border px-2 py-1 text-right">
                            {item.quantity.toFixed(2)}
                          </td>
                          <td className="border px-2 py-1">{item.unit}</td>
                          {showCosting && (
                            <td className="border px-2 py-1 text-right">
                              {formatCurrency(item.unitCost, proposal.currency)}
                            </td>
                          )}
                          <td className="border px-2 py-1 text-right">
                            {formatCurrency(item.unitPrice, proposal.currency)}
                          </td>
                          {showCosting && (
                            <>
                              <td className="border px-2 py-1 text-right">
                                {formatCurrency(item.lineCost, proposal.currency)}
                              </td>
                              <td
                                className={`border px-2 py-1 text-right ${
                                  item.lineMargin >= 0 ? "text-green-600" : "text-red-600"
                                }`}
                              >
                                {item.marginPercent.toFixed(1)}%
                              </td>
                            </>
                          )}
                          <td className="border px-2 py-1 text-right font-medium">
                            {formatCurrency(item.lineTotal, proposal.currency)}
                          </td>
                        </tr>
                      ))}
                  </tbody>
                </table>
              )}

              {/* Section subtotal */}
              <div className="mt-1 text-right text-sm">
                <span className="text-gray-500">Section Subtotal: </span>
                <strong>{formatCurrency(section.sectionTotal, proposal.currency)}</strong>
              </div>
            </div>
          ))}

        {/* Totals */}
        <div className="mb-6 flex justify-end">
          <div className="w-64 rounded border border-gray-200 bg-gray-50 p-4">
            <div className="flex justify-between text-sm">
              <span>Subtotal:</span>
              <span>{formatCurrency(proposal.subtotal, proposal.currency)}</span>
            </div>
            {(proposal.discountPercent > 0 || proposal.discountAmount > 0) && (
              <div className="flex justify-between text-sm text-red-600">
                <span>Discount ({proposal.discountPercent.toFixed(1)}%):</span>
                <span>-{formatCurrency(proposal.discountAmount, proposal.currency)}</span>
              </div>
            )}
            <div className="flex justify-between text-sm">
              <span>Net Total:</span>
              <span>{formatCurrency(proposal.netTotal, proposal.currency)}</span>
            </div>
            <div className="flex justify-between text-sm">
              <span>VAT ({proposal.vatRate.toFixed(0)}%):</span>
              <span>{formatCurrency(proposal.vatAmount, proposal.currency)}</span>
            </div>
            <div className="mt-2 flex justify-between border-t border-[#1e3a5f] pt-2 text-lg font-bold text-[#1e3a5f]">
              <span>GRAND TOTAL:</span>
              <span>{formatCurrency(proposal.grandTotal, proposal.currency)}</span>
            </div>
          </div>
        </div>

        {/* Internal costing summary */}
        {showCosting && proposal.totalMargin !== undefined && (
          <div className="mb-6 flex justify-end">
            <div className="w-80 rounded border-2 border-orange-400 bg-yellow-50 p-4">
              <p className="mb-2 text-xs font-bold text-red-600">
                INTERNAL - DO NOT SHARE WITH CLIENT
              </p>
              <div className="flex justify-between text-sm">
                <span>Total Cost:</span>
                <span>{formatCurrency(proposal.totalCost ?? 0, proposal.currency)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span>Total Margin:</span>
                <span
                  className={
                    (proposal.totalMargin ?? 0) >= 0 ? "text-green-600" : "text-red-600"
                  }
                >
                  {formatCurrency(proposal.totalMargin ?? 0, proposal.currency)} (
                  {(proposal.marginPercent ?? 0).toFixed(1)}%)
                </span>
              </div>
            </div>
          </div>
        )}

        {/* Terms */}
        {(proposal.paymentTerms || proposal.termsAndConditions) && (
          <div className="mb-6 space-y-4">
            {proposal.paymentTerms && (
              <div className="rounded border border-gray-200 p-3">
                <h3 className="mb-1 text-sm font-bold text-[#1e3a5f]">PAYMENT TERMS</h3>
                <p className="whitespace-pre-wrap text-sm">{proposal.paymentTerms}</p>
              </div>
            )}
            {proposal.termsAndConditions && (
              <div className="rounded border border-gray-200 p-3">
                <h3 className="mb-1 text-sm font-bold text-[#1e3a5f]">
                  TERMS & CONDITIONS
                </h3>
                <p className="whitespace-pre-wrap text-sm">{proposal.termsAndConditions}</p>
              </div>
            )}
          </div>
        )}

        {/* Signature section */}
        <div className="rounded border border-gray-200 p-6">
          <h3 className="mb-4 text-sm font-bold text-[#1e3a5f]">ACCEPTANCE</h3>
          <p className="mb-6 text-sm">
            I accept this proposal and agree to the terms and conditions stated above.
          </p>
          <div className="mb-6 flex gap-6">
            <div className="flex-1">
              <div className="mb-1 border-b border-gray-400"></div>
              <p className="text-xs text-gray-500">Signature</p>
            </div>
            <div className="w-40">
              <div className="mb-1 border-b border-gray-400"></div>
              <p className="text-xs text-gray-500">Date</p>
            </div>
          </div>
          <div className="flex gap-6">
            <div className="flex-1">
              <div className="mb-1 border-b border-gray-400"></div>
              <p className="text-xs text-gray-500">Print Name</p>
            </div>
            <div className="w-40">
              <div className="mb-1 border-b border-gray-400"></div>
              <p className="text-xs text-gray-500">Position</p>
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="mt-6 border-t border-gray-200 pt-4 text-center text-xs text-gray-500">
          <p>
            {company.name} | VAT: {company.vatNumber} | Reg: {company.regNumber}
          </p>
          <p className="mt-1">Generated: {format(new Date(), "dd MMM yyyy HH:mm")}</p>
        </div>
      </div>
    </>
  );
}
