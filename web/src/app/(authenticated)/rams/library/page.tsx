"use client";

import * as React from "react";
import Link from "next/link";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { HazardsLibrary } from "@/components/rams/library/hazards-library";
import { ControlsLibrary } from "@/components/rams/library/controls-library";
import { LegislationLibrary } from "@/components/rams/library/legislation-library";
import { SopsLibrary } from "@/components/rams/library/sops-library";

export default function RamsLibraryPage() {
  return (
    <div className="space-y-6">
      {/* Breadcrumb */}
      <nav className="flex items-center text-sm text-muted-foreground">
        <Link href="/rams" className="hover:text-foreground">
          RAMS Documents
        </Link>
        <span className="mx-2">/</span>
        <span className="text-foreground">Reference Library</span>
      </nav>

      {/* Header */}
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">RAMS Reference Library</h1>
        <p className="text-muted-foreground">
          Manage hazards, control measures, legislation, and standard operating procedures
        </p>
      </div>

      {/* Tabs */}
      <Tabs defaultValue="hazards" className="space-y-4">
        <TabsList className="grid w-full grid-cols-4 lg:w-auto lg:inline-grid">
          <TabsTrigger value="hazards" className="gap-2">
            <AlertTriangleIcon className="h-4 w-4" />
            <span className="hidden sm:inline">Hazards</span>
          </TabsTrigger>
          <TabsTrigger value="controls" className="gap-2">
            <ShieldCheckIcon className="h-4 w-4" />
            <span className="hidden sm:inline">Controls</span>
          </TabsTrigger>
          <TabsTrigger value="legislation" className="gap-2">
            <BookIcon className="h-4 w-4" />
            <span className="hidden sm:inline">Legislation</span>
          </TabsTrigger>
          <TabsTrigger value="sops" className="gap-2">
            <FileTextIcon className="h-4 w-4" />
            <span className="hidden sm:inline">SOPs</span>
          </TabsTrigger>
        </TabsList>

        <TabsContent value="hazards">
          <HazardsLibrary />
        </TabsContent>

        <TabsContent value="controls">
          <ControlsLibrary />
        </TabsContent>

        <TabsContent value="legislation">
          <LegislationLibrary />
        </TabsContent>

        <TabsContent value="sops">
          <SopsLibrary />
        </TabsContent>
      </Tabs>
    </div>
  );
}

function AlertTriangleIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth={2}
        d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
      />
    </svg>
  );
}

function ShieldCheckIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth={2}
        d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"
      />
    </svg>
  );
}

function BookIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth={2}
        d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"
      />
    </svg>
  );
}

function FileTextIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth={2}
        d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
      />
    </svg>
  );
}
