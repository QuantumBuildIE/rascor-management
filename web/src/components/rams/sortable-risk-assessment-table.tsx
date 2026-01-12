"use client";

import * as React from "react";
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  DragEndEvent,
} from "@dnd-kit/core";
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  useSortable,
  verticalListSortingStrategy,
} from "@dnd-kit/sortable";
import {
  restrictToVerticalAxis,
  restrictToParentElement,
} from "@dnd-kit/modifiers";
import { CSS } from "@dnd-kit/utilities";
import { GripVertical, MoreVertical, Pencil, Trash2, Sparkles, Loader2 } from "lucide-react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { cn } from "@/lib/utils";
import type { RiskAssessmentDto } from "@/types/rams";
import { useReorderRiskAssessments } from "@/lib/api/rams";
import { toast } from "sonner";

interface SortableRiskAssessmentTableProps {
  ramsDocumentId: string;
  riskAssessments: RiskAssessmentDto[];
  canEdit: boolean;
  onEdit: (risk: RiskAssessmentDto) => void;
  onDelete: (riskId: string) => void;
}

function getRiskLevelBadge(level: string): { className: string } {
  switch (level) {
    case "Low":
      return { className: "bg-green-100 text-green-800 hover:bg-green-100" };
    case "Medium":
      return { className: "bg-yellow-100 text-yellow-800 hover:bg-yellow-100" };
    case "High":
      return { className: "bg-red-100 text-red-800 hover:bg-red-100" };
    default:
      return { className: "bg-gray-100 text-gray-800 hover:bg-gray-100" };
  }
}

interface SortableRiskRowProps {
  risk: RiskAssessmentDto;
  index: number;
  canEdit: boolean;
  onEdit: (risk: RiskAssessmentDto) => void;
  onDelete: (riskId: string) => void;
}

function SortableRiskRow({
  risk,
  index,
  canEdit,
  onEdit,
  onDelete,
}: SortableRiskRowProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: risk.id, disabled: !canEdit });

  const style: React.CSSProperties = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
    position: "relative" as const,
    zIndex: isDragging ? 1 : 0,
  };

  return (
    <TableRow
      ref={setNodeRef}
      style={style}
      className={cn(isDragging && "bg-muted")}
    >
      <TableCell className="text-muted-foreground">
        <div className="flex items-center gap-1">
          {canEdit && (
            <button
              type="button"
              className="touch-none cursor-grab p-1 text-muted-foreground hover:text-foreground focus:outline-none"
              {...listeners}
              {...attributes}
            >
              <GripVertical className="h-4 w-4" />
            </button>
          )}
          <span>{index + 1}</span>
        </div>
      </TableCell>
      <TableCell>
        <div className="font-medium">{risk.taskActivity}</div>
        {risk.locationArea && (
          <div className="text-sm text-muted-foreground">
            {risk.locationArea}
          </div>
        )}
      </TableCell>
      <TableCell>
        <div>{risk.hazardIdentified}</div>
        {risk.whoAtRisk && (
          <div className="text-sm text-muted-foreground">
            At risk: {risk.whoAtRisk}
          </div>
        )}
      </TableCell>
      <TableCell className="text-center">
        <Badge
          variant="secondary"
          className={getRiskLevelBadge(risk.initialRiskLevelDisplay).className}
        >
          {risk.initialRiskRating}
        </Badge>
        <div className="text-xs text-muted-foreground mt-1">
          L{risk.initialLikelihood} × S{risk.initialSeverity}
        </div>
      </TableCell>
      <TableCell className="hidden md:table-cell max-w-xs">
        <div className="truncate">{risk.controlMeasures || "-"}</div>
        {risk.isAiGenerated && (
          <Badge variant="outline" className="mt-1 text-xs">
            <Sparkles className="mr-1 h-3 w-3" />
            AI Generated
          </Badge>
        )}
      </TableCell>
      <TableCell className="text-center">
        <Badge
          variant="secondary"
          className={getRiskLevelBadge(risk.residualRiskLevelDisplay).className}
        >
          {risk.residualRiskRating}
        </Badge>
        <div className="text-xs text-muted-foreground mt-1">
          L{risk.residualLikelihood} × S{risk.residualSeverity}
        </div>
      </TableCell>
      {canEdit && (
        <TableCell>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="h-8 w-8">
                <MoreVertical className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => onEdit(risk)}>
                <Pencil className="mr-2 h-4 w-4" />
                Edit
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                className="text-destructive"
                onClick={() => onDelete(risk.id)}
              >
                <Trash2 className="mr-2 h-4 w-4" />
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </TableCell>
      )}
    </TableRow>
  );
}

export function SortableRiskAssessmentTable({
  ramsDocumentId,
  riskAssessments,
  canEdit,
  onEdit,
  onDelete,
}: SortableRiskAssessmentTableProps) {
  const [localRisks, setLocalRisks] = React.useState(riskAssessments);
  const [isSaving, setIsSaving] = React.useState(false);
  const reorderMutation = useReorderRiskAssessments();

  // Sync local state with props
  React.useEffect(() => {
    setLocalRisks(riskAssessments);
  }, [riskAssessments]);

  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 8,
      },
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const handleDragEnd = async (event: DragEndEvent) => {
    const { active, over } = event;

    if (over && active.id !== over.id) {
      const oldIndex = localRisks.findIndex((r) => r.id === active.id);
      const newIndex = localRisks.findIndex((r) => r.id === over.id);
      const reorderedItems = arrayMove(localRisks, oldIndex, newIndex);

      // Optimistically update UI
      setLocalRisks(reorderedItems);

      // Save to server
      setIsSaving(true);
      try {
        const orderedIds = reorderedItems.map((r) => r.id);
        await reorderMutation.mutateAsync({
          ramsDocumentId,
          orderedIds,
        });
        toast.success("Risk assessments reordered");
      } catch (err) {
        console.error("Failed to save reorder:", err);
        // Revert on error
        setLocalRisks(riskAssessments);
        toast.error("Failed to reorder risk assessments");
      } finally {
        setIsSaving(false);
      }
    }
  };

  const sortedRisks = [...localRisks].sort((a, b) => a.sortOrder - b.sortOrder);

  return (
    <div className="relative">
      {isSaving && (
        <div className="absolute top-2 right-2 z-10">
          <Badge variant="secondary" className="gap-1">
            <Loader2 className="h-3 w-3 animate-spin" />
            Saving...
          </Badge>
        </div>
      )}
      <div className="overflow-x-auto">
        <DndContext
          sensors={sensors}
          collisionDetection={closestCenter}
          onDragEnd={handleDragEnd}
          modifiers={[restrictToVerticalAxis, restrictToParentElement]}
        >
          <SortableContext
            items={sortedRisks.map((r) => r.id)}
            strategy={verticalListSortingStrategy}
          >
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-16">#</TableHead>
                  <TableHead>Task/Activity</TableHead>
                  <TableHead>Hazard</TableHead>
                  <TableHead className="text-center">Initial Risk</TableHead>
                  <TableHead className="hidden md:table-cell">
                    Control Measures
                  </TableHead>
                  <TableHead className="text-center">Residual Risk</TableHead>
                  {canEdit && <TableHead className="w-10"></TableHead>}
                </TableRow>
              </TableHeader>
              <TableBody>
                {sortedRisks.map((risk, index) => (
                  <SortableRiskRow
                    key={risk.id}
                    risk={risk}
                    index={index}
                    canEdit={canEdit}
                    onEdit={onEdit}
                    onDelete={onDelete}
                  />
                ))}
              </TableBody>
            </Table>
          </SortableContext>
        </DndContext>
      </div>
      {canEdit && (
        <div className="mt-2 px-4 pb-2">
          <p className="text-xs text-muted-foreground flex items-center gap-1">
            <GripVertical className="h-3 w-3" />
            Drag rows to reorder
          </p>
        </div>
      )}
    </div>
  );
}
