"use client";

import * as React from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { QRCodeSVG } from "qrcode.react";
import { Button } from "@/components/ui/button";
import { useStocktake } from "@/lib/api/stock/use-stocktakes";
import { format } from "date-fns";

const statusLabels: Record<string, string> = {
  Draft: "Draft",
  InProgress: "In Progress",
  Completed: "Completed",
  Cancelled: "Cancelled",
};

export default function PrintCountSheetPage() {
  const params = useParams();
  const id = params.id as string;
  const { data: stocktake, isLoading, error } = useStocktake(id);

  const handlePrint = () => {
    window.print();
  };

  const getCountUrl = (lineId: string) => {
    if (typeof window !== "undefined") {
      return `${window.location.origin}/stock/stocktakes/${id}/count/${lineId}`;
    }
    return `/stock/stocktakes/${id}/count/${lineId}`;
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <LoadingSpinner className="h-8 w-8 mx-auto" />
          <p className="mt-2 text-muted-foreground">Loading stocktake...</p>
        </div>
      </div>
    );
  }

  if (error || !stocktake) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <p className="text-destructive">Failed to load stocktake details.</p>
          <Link
            href={`/stock/stocktakes/${id}`}
            className="text-primary underline mt-2 inline-block"
          >
            Back to Stocktake
          </Link>
        </div>
      </div>
    );
  }

  return (
    <>
      {/* Print Styles */}
      <style jsx global>{`
        @media print {
          /* Hide non-print elements */
          .no-print {
            display: none !important;
          }

          /* Reset body margins */
          body {
            -webkit-print-color-adjust: exact !important;
            print-color-adjust: exact !important;
          }

          /* Page setup */
          @page {
            size: A4 portrait;
            margin: 15mm;
          }

          /* Ensure table headers repeat on new pages */
          thead {
            display: table-header-group;
          }

          /* Prevent page breaks inside rows */
          tr {
            page-break-inside: avoid;
          }

          /* Ensure QR codes print properly */
          svg {
            print-color-adjust: exact !important;
          }
        }

        /* Screen-only styles */
        @media screen {
          .print-only {
            display: none !important;
          }
        }
      `}</style>

      {/* Print Actions - Hidden when printing */}
      <div className="no-print p-4 bg-gray-50 border-b flex items-center justify-between sticky top-0 z-10">
        <Link
          href={`/stock/stocktakes/${id}`}
          className="text-sm text-primary hover:underline flex items-center gap-1"
        >
          <ChevronLeftIcon className="h-4 w-4" />
          Back to Stocktake
        </Link>
        <Button onClick={handlePrint} className="bg-violet-600 hover:bg-violet-700">
          <PrinterIcon className="h-4 w-4 mr-2" />
          Print Count Sheet
        </Button>
      </div>

      {/* Printable Content */}
      <div className="max-w-[210mm] mx-auto bg-white p-8 print:p-0 print:max-w-none">
        {/* Header Section */}
        <div className="flex items-start justify-between mb-8">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">STOCK COUNT SHEET</h1>
            <div className="mt-4 space-y-1 text-sm">
              <p>
                <span className="text-gray-500">Stock Take #:</span>{" "}
                <span className="font-semibold">{stocktake.stocktakeNumber}</span>
              </p>
              <p>
                <span className="text-gray-500">Location:</span>{" "}
                <span className="font-semibold">
                  {stocktake.locationCode} - {stocktake.locationName}
                </span>
              </p>
            </div>
          </div>
          <div className="text-right">
            {/* RASCOR Logo Placeholder */}
            <div className="mb-4">
              <span className="text-2xl font-bold text-violet-600">RASCOR</span>
            </div>
            <div className="space-y-1 text-sm">
              <p>
                <span className="text-gray-500">Date:</span>{" "}
                <span className="font-semibold">
                  {format(new Date(stocktake.countDate), "dd/MM/yyyy")}
                </span>
              </p>
              <p>
                <span className="text-gray-500">Status:</span>{" "}
                <span className="font-semibold">{statusLabels[stocktake.status]}</span>
              </p>
              <p>
                <span className="text-gray-500">Counted By:</span>{" "}
                <span className="font-semibold">{stocktake.countedBy}</span>
              </p>
            </div>
          </div>
        </div>

        {/* Instructions */}
        <div className="mb-6 text-sm text-gray-600 border-l-4 border-violet-600 pl-4 py-2 bg-violet-50">
          <p className="font-medium text-gray-900">Instructions:</p>
          <p>
            1. Count each product at the location and write the quantity in the
            &quot;Counted&quot; column.
          </p>
          <p>
            2. Scan the QR code with your phone to quickly enter counts digitally.
          </p>
          <p>3. Add notes for any discrepancies or issues found.</p>
        </div>

        {/* Count Table */}
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="bg-violet-600 text-white">
              <th className="border border-violet-700 px-3 py-2 text-left w-[60px]">
                Scan
              </th>
              <th className="border border-violet-700 px-3 py-2 text-left">
                Product
              </th>
              <th className="border border-violet-700 px-3 py-2 text-center w-[60px]">
                Bay
              </th>
              <th className="border border-violet-700 px-3 py-2 text-center w-[70px]">
                Expected
              </th>
              <th className="border border-violet-700 px-3 py-2 text-center w-[70px]">
                Counted
              </th>
              <th className="border border-violet-700 px-3 py-2 text-left w-[180px]">
                Notes
              </th>
            </tr>
          </thead>
          <tbody>
            {stocktake.lines.map((line, index) => (
              <tr
                key={line.id}
                className={index % 2 === 0 ? "bg-white" : "bg-gray-50"}
              >
                <td className="border border-gray-300 px-2 py-2 text-center">
                  <QRCodeSVG
                    value={getCountUrl(line.id)}
                    size={40}
                    level="L"
                    includeMargin={false}
                  />
                </td>
                <td className="border border-gray-300 px-3 py-2">
                  <div className="font-medium text-gray-900">{line.productName}</div>
                  <div className="text-xs text-gray-500">{line.productCode}</div>
                </td>
                <td className="border border-gray-300 px-3 py-2 text-center font-medium text-xs">
                  {line.bayCode || "-"}
                </td>
                <td className="border border-gray-300 px-3 py-2 text-center font-medium">
                  {line.systemQuantity}
                </td>
                <td className="border border-gray-300 px-3 py-2">
                  {/* Empty cell for manual entry */}
                  <div className="h-6 border-b border-dashed border-gray-400"></div>
                </td>
                <td className="border border-gray-300 px-3 py-2">
                  {/* Empty cell for notes */}
                  <div className="h-6 border-b border-dashed border-gray-400"></div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {/* Footer */}
        <div className="mt-8 pt-4 border-t border-gray-300">
          <div className="flex justify-between text-sm text-gray-500">
            <p>Total Products: {stocktake.lines.length}</p>
            <p>
              Printed:{" "}
              {format(new Date(), "dd/MM/yyyy HH:mm")}
            </p>
          </div>
          {stocktake.notes && (
            <div className="mt-4 p-3 bg-gray-50 rounded text-sm">
              <span className="font-medium">Notes:</span> {stocktake.notes}
            </div>
          )}
        </div>

        {/* Signature Section */}
        <div className="mt-8 grid grid-cols-2 gap-8">
          <div>
            <p className="text-sm text-gray-500 mb-2">Counted By:</p>
            <div className="border-b border-gray-400 h-8"></div>
            <p className="text-xs text-gray-400 mt-1">Signature</p>
          </div>
          <div>
            <p className="text-sm text-gray-500 mb-2">Date:</p>
            <div className="border-b border-gray-400 h-8"></div>
            <p className="text-xs text-gray-400 mt-1">DD/MM/YYYY</p>
          </div>
        </div>
      </div>
    </>
  );
}

function ChevronLeftIcon({ className }: { className?: string }) {
  return (
    <svg
      className={className}
      fill="none"
      stroke="currentColor"
      viewBox="0 0 24 24"
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth={2}
        d="M15 19l-7-7 7-7"
      />
    </svg>
  );
}

function PrinterIcon({ className }: { className?: string }) {
  return (
    <svg
      className={className}
      fill="none"
      stroke="currentColor"
      viewBox="0 0 24 24"
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth={2}
        d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z"
      />
    </svg>
  );
}

function LoadingSpinner({ className }: { className?: string }) {
  return (
    <svg
      className={`animate-spin ${className}`}
      xmlns="http://www.w3.org/2000/svg"
      fill="none"
      viewBox="0 0 24 24"
    >
      <circle
        className="opacity-25"
        cx="12"
        cy="12"
        r="10"
        stroke="currentColor"
        strokeWidth="4"
      />
      <path
        className="opacity-75"
        fill="currentColor"
        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
      />
    </svg>
  );
}
