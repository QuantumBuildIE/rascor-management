"use client";

import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { HazardsLibrary } from "@/components/rams/library/hazards-library";
import { ControlsLibrary } from "@/components/rams/library/controls-library";
import { LegislationLibrary } from "@/components/rams/library/legislation-library";
import { SopsLibrary } from "@/components/rams/library/sops-library";
import { AlertTriangle, ShieldCheck, BookOpen, FileText } from "lucide-react";

export default function AdminRamsLibraryPage() {
  return (
    <div className="space-y-6">
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
            <AlertTriangle className="h-4 w-4" />
            <span className="hidden sm:inline">Hazards</span>
          </TabsTrigger>
          <TabsTrigger value="controls" className="gap-2">
            <ShieldCheck className="h-4 w-4" />
            <span className="hidden sm:inline">Controls</span>
          </TabsTrigger>
          <TabsTrigger value="legislation" className="gap-2">
            <BookOpen className="h-4 w-4" />
            <span className="hidden sm:inline">Legislation</span>
          </TabsTrigger>
          <TabsTrigger value="sops" className="gap-2">
            <FileText className="h-4 w-4" />
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
