"use client";

import * as React from "react";
import { useParams, useSearchParams } from "next/navigation";
import { useStockOrder, useStockOrderForDocket } from "@/lib/api/stock/use-stock-orders";
import { useLocations } from "@/lib/api/stock/use-locations";

export default function StockOrderPrintPage() {
  const params = useParams();
  const searchParams = useSearchParams();
  const id = params.id as string;
  const warehouseLocationId = searchParams.get("warehouseLocationId");

  // If warehouseLocationId is provided, use docket endpoint (includes bay codes)
  // Otherwise, use regular endpoint
  const { data: locations = [] } = useLocations();

  // Default to first warehouse location if not specified
  const defaultWarehouseId = React.useMemo(() => {
    const warehouse = locations.find(l => l.locationType === "Warehouse" && l.isActive);
    return warehouse?.id;
  }, [locations]);

  const effectiveWarehouseId = warehouseLocationId || defaultWarehouseId;

  // Use docket endpoint when we have a warehouse location to get bay codes
  const {
    data: orderWithBays,
    isLoading: isLoadingDocket
  } = useStockOrderForDocket(id, effectiveWarehouseId);

  // Fallback to regular order if docket fails
  const { data: regularOrder, isLoading: isLoadingRegular } = useStockOrder(id);

  const order = orderWithBays || regularOrder;
  const isLoading = isLoadingDocket || isLoadingRegular;

  // Auto-trigger print dialog once loaded
  React.useEffect(() => {
    if (order && !isLoading) {
      // Small delay to ensure rendering is complete
      const timer = setTimeout(() => {
        window.print();
      }, 500);
      return () => clearTimeout(timer);
    }
  }, [order, isLoading]);

  if (isLoading || !order) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-lg">Loading order...</div>
      </div>
    );
  }

  if (!order) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-lg text-red-600">Failed to load order details.</div>
      </div>
    );
  }

  return (
    <>
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

          .no-print {
            display: none !important;
          }
        }

        @media screen {
          .print-container {
            max-width: 210mm;
            margin: 0 auto;
            padding: 20px;
            background: white;
          }
        }
      `}</style>

      <div className="print-container bg-white text-black min-h-screen font-sans">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold mb-1">RASCOR Ireland</h1>
          <h2 className="text-xl font-semibold text-gray-700">Stock Order Docket</h2>
        </div>

        {/* Order Details - Two Column Layout */}
        <div className="mb-8 border border-gray-300 rounded">
          <div className="grid grid-cols-2">
            {/* Left Column */}
            <div className="border-r border-gray-300">
              <div className="flex border-b border-gray-300 px-4 py-2">
                <span className="text-gray-600 w-32">Order Number:</span>
                <span className="font-semibold">{order.orderNumber}</span>
              </div>
              <div className="flex border-b border-gray-300 px-4 py-2">
                <span className="text-gray-600 w-32">Order Date:</span>
                <span className="font-semibold">
                  {new Date(order.orderDate).toLocaleDateString("en-IE")}
                </span>
              </div>
              <div className="flex border-b border-gray-300 px-4 py-2">
                <span className="text-gray-600 w-32">Pick From:</span>
                <span className="font-semibold">{order.sourceLocationName}</span>
              </div>
            </div>

            {/* Right Column */}
            <div>
              <div className="flex border-b border-gray-300 px-4 py-2">
                <span className="text-gray-600 w-32">Site/Project:</span>
                <span className="font-semibold">{order.siteName}</span>
              </div>
              <div className="flex border-b border-gray-300 px-4 py-2">
                <span className="text-gray-600 w-32">Requested By:</span>
                <span className="font-semibold">{order.requestedBy}</span>
              </div>
            </div>
          </div>

          {/* Required Date - Full Width */}
          {order.requiredDate && (
            <div className="flex px-4 py-2 border-t border-gray-300">
              <span className="text-gray-600 w-32">Required Date:</span>
              <span className="font-semibold">
                {new Date(order.requiredDate).toLocaleDateString("en-IE")}
              </span>
            </div>
          )}
        </div>

        {/* Products Table */}
        <div className="mb-8">
          <h3 className="text-lg font-bold mb-3 border-b-2 border-black pb-1">
            Products to Pick (sorted by Bay)
          </h3>
          <table className="w-full border-collapse">
            <thead>
              <tr className="bg-gray-100">
                <th className="border border-gray-300 px-3 py-2 text-center font-semibold w-16">
                  Bay
                </th>
                <th className="border border-gray-300 px-3 py-2 text-left font-semibold w-24">
                  Product Code
                </th>
                <th className="border border-gray-300 px-3 py-2 text-left font-semibold">
                  Product
                </th>
                <th className="border border-gray-300 px-3 py-2 text-center font-semibold w-16">
                  Qty
                </th>
                <th className="border border-gray-300 px-3 py-2 text-center font-semibold w-16">
                  Picked
                </th>
              </tr>
            </thead>
            <tbody>
              {order.lines.map((line, index) => (
                <tr key={line.id} className={index % 2 === 1 ? "bg-gray-50" : ""}>
                  <td className="border border-gray-300 px-3 py-2 text-center font-semibold text-sm">
                    {line.bayCode || "-"}
                  </td>
                  <td className="border border-gray-300 px-3 py-2 font-medium text-sm">
                    {line.productCode}
                  </td>
                  <td className="border border-gray-300 px-3 py-2">
                    {line.productName}
                  </td>
                  <td className="border border-gray-300 px-3 py-2 text-center font-semibold">
                    {line.quantityRequested}
                  </td>
                  <td className="border border-gray-300 px-3 py-2">
                    {/* Empty cell for manual picking confirmation */}
                    <div className="h-5 w-full border-b border-dashed border-gray-400"></div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Additional Notes */}
        {order.notes && (
          <div className="mb-8">
            <h3 className="text-lg font-bold mb-3 border-b-2 border-black pb-1">
              Additional Notes
            </h3>
            <div className="border border-gray-300 rounded p-4 min-h-[60px] bg-gray-50">
              {order.notes}
            </div>
          </div>
        )}

        {/* Signature Section */}
        <div className="mt-12 grid grid-cols-2 gap-8">
          <div>
            <div className="border-b-2 border-black mb-2 h-12"></div>
            <p className="font-semibold">Warehouse Signature</p>
          </div>
          <div>
            <div className="border-b-2 border-black mb-2 h-12"></div>
            <p className="font-semibold">Date/Time Collected</p>
          </div>
        </div>

        {/* Print Button - Only visible on screen */}
        <div className="no-print mt-8 pt-8 border-t flex gap-4 justify-center">
          <button
            onClick={() => window.print()}
            className="px-6 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition-colors"
          >
            Print Docket
          </button>
          <button
            onClick={() => window.close()}
            className="px-6 py-2 bg-gray-200 text-gray-800 rounded hover:bg-gray-300 transition-colors"
          >
            Close
          </button>
        </div>
      </div>
    </>
  );
}
