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
import {
  GripVertical,
  MoreVertical,
  Pencil,
  Trash2,
  Link2,
  FileText,
  FileCheck,
  Loader2,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { cn } from "@/lib/utils";
import type { MethodStepDto } from "@/types/rams";
import { useReorderMethodSteps } from "@/lib/api/rams";
import { toast } from "sonner";

interface SortableMethodStepListProps {
  ramsDocumentId: string;
  methodSteps: MethodStepDto[];
  canEdit: boolean;
  onEdit: (step: MethodStepDto) => void;
  onDelete: (stepId: string) => void;
}

interface SortableStepItemProps {
  step: MethodStepDto;
  displayNumber: number;
  canEdit: boolean;
  onEdit: (step: MethodStepDto) => void;
  onDelete: (stepId: string) => void;
}

function SortableStepItem({
  step,
  displayNumber,
  canEdit,
  onEdit,
  onDelete,
}: SortableStepItemProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: step.id, disabled: !canEdit });

  const style: React.CSSProperties = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
    position: "relative" as const,
    zIndex: isDragging ? 1 : 0,
  };

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={cn(
        "p-4 border-b last:border-b-0",
        isDragging && "bg-muted/50 shadow-sm"
      )}
    >
      <div className="flex items-start justify-between gap-4">
        <div className="flex gap-4">
          {canEdit && (
            <div className="flex items-center">
              <button
                type="button"
                className="touch-none cursor-grab p-1 text-muted-foreground hover:text-foreground focus:outline-none"
                {...listeners}
                {...attributes}
              >
                <GripVertical className="h-4 w-4" />
              </button>
            </div>
          )}
          <div className="flex-shrink-0 w-8 h-8 rounded-full bg-primary text-primary-foreground flex items-center justify-center font-semibold">
            {displayNumber}
          </div>
          <div className="space-y-1">
            <h4 className="font-medium">{step.stepTitle}</h4>
            {step.detailedProcedure && (
              <p className="text-sm text-muted-foreground whitespace-pre-wrap">
                {step.detailedProcedure}
              </p>
            )}
            <div className="flex flex-wrap gap-3 text-sm">
              {step.linkedRiskAssessmentTask && (
                <span className="flex items-center gap-1 text-muted-foreground">
                  <Link2 className="h-3 w-3" />
                  Linked: {step.linkedRiskAssessmentTask}
                </span>
              )}
              {step.requiredPermits && (
                <span className="flex items-center gap-1 text-amber-600">
                  <FileText className="h-3 w-3" />
                  Permits: {step.requiredPermits}
                </span>
              )}
              {step.requiresSignoff && (
                <span className="flex items-center gap-1 text-blue-600">
                  <FileCheck className="h-3 w-3" />
                  Requires Sign-off
                </span>
              )}
            </div>
          </div>
        </div>
        {canEdit && (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="h-8 w-8">
                <MoreVertical className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => onEdit(step)}>
                <Pencil className="mr-2 h-4 w-4" />
                Edit
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                className="text-destructive"
                onClick={() => onDelete(step.id)}
              >
                <Trash2 className="mr-2 h-4 w-4" />
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        )}
      </div>
    </div>
  );
}

export function SortableMethodStepList({
  ramsDocumentId,
  methodSteps,
  canEdit,
  onEdit,
  onDelete,
}: SortableMethodStepListProps) {
  const [localSteps, setLocalSteps] = React.useState(methodSteps);
  const [isSaving, setIsSaving] = React.useState(false);
  const reorderMutation = useReorderMethodSteps();

  // Sync local state with props
  React.useEffect(() => {
    setLocalSteps(methodSteps);
  }, [methodSteps]);

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
      const oldIndex = localSteps.findIndex((s) => s.id === active.id);
      const newIndex = localSteps.findIndex((s) => s.id === over.id);
      const reorderedItems = arrayMove(localSteps, oldIndex, newIndex);

      // Update step numbers locally for display
      const updatedItems = reorderedItems.map((item, index) => ({
        ...item,
        stepNumber: index + 1,
      }));

      // Optimistically update UI
      setLocalSteps(updatedItems);

      // Save to server
      setIsSaving(true);
      try {
        const orderedIds = updatedItems.map((s) => s.id);
        await reorderMutation.mutateAsync({
          ramsDocumentId,
          orderedIds,
        });
        toast.success("Method steps reordered");
      } catch (err) {
        console.error("Failed to save reorder:", err);
        // Revert on error
        setLocalSteps(methodSteps);
        toast.error("Failed to reorder method steps");
      } finally {
        setIsSaving(false);
      }
    }
  };

  const sortedSteps = [...localSteps].sort(
    (a, b) => a.stepNumber - b.stepNumber
  );

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
      <DndContext
        sensors={sensors}
        collisionDetection={closestCenter}
        onDragEnd={handleDragEnd}
        modifiers={[restrictToVerticalAxis, restrictToParentElement]}
      >
        <SortableContext
          items={sortedSteps.map((s) => s.id)}
          strategy={verticalListSortingStrategy}
        >
          <div className="divide-y">
            {sortedSteps.map((step, index) => (
              <SortableStepItem
                key={step.id}
                step={step}
                displayNumber={index + 1}
                canEdit={canEdit}
                onEdit={onEdit}
                onDelete={onDelete}
              />
            ))}
          </div>
        </SortableContext>
      </DndContext>
      {canEdit && (
        <div className="mt-2 px-4 pb-2">
          <p className="text-xs text-muted-foreground flex items-center gap-1">
            <GripVertical className="h-3 w-3" />
            Drag steps to reorder
          </p>
        </div>
      )}
    </div>
  );
}
