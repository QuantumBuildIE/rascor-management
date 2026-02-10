'use client';

import { useState, useMemo } from 'react';
import { format } from 'date-fns';
import { CalendarIcon, SearchIcon, UsersIcon } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Checkbox } from '@/components/ui/checkbox';
import { Calendar } from '@/components/ui/calendar';
import { Label } from '@/components/ui/label';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { useAllEmployees } from '@/lib/api/admin/use-employees';
import { useAssignCourse } from '@/lib/api/toolbox-talks/use-course-assignments';
import { toast } from 'sonner';
import { cn } from '@/lib/utils';

interface AssignCourseDialogProps {
  course: { id: string; title: string };
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function AssignCourseDialog({ course, open, onOpenChange }: AssignCourseDialogProps) {
  const { data: employees, isLoading: employeesLoading } = useAllEmployees();
  const assignMutation = useAssignCourse();

  const [selectedEmployees, setSelectedEmployees] = useState<string[]>([]);
  const [employeeSearch, setEmployeeSearch] = useState('');
  const [dueDate, setDueDate] = useState<Date | undefined>(undefined);

  const filteredEmployees = useMemo(() => {
    return (employees ?? []).filter((employee) => {
      if (!employeeSearch.trim()) return true;
      const search = employeeSearch.toLowerCase();
      return (
        employee.firstName?.toLowerCase().includes(search) ||
        employee.lastName?.toLowerCase().includes(search) ||
        `${employee.firstName} ${employee.lastName}`.toLowerCase().includes(search) ||
        employee.email?.toLowerCase().includes(search)
      );
    });
  }, [employees, employeeSearch]);

  const handleEmployeeToggle = (employeeId: string, checked: boolean) => {
    setSelectedEmployees((prev) =>
      checked ? [...prev, employeeId] : prev.filter((id) => id !== employeeId)
    );
  };

  const handleSelectAll = () => {
    const allFilteredIds = filteredEmployees.map((e) => e.id);
    setSelectedEmployees((prev) => {
      const newSet = new Set(prev);
      allFilteredIds.forEach((id) => newSet.add(id));
      return Array.from(newSet);
    });
  };

  const handleClearAll = () => {
    setSelectedEmployees([]);
  };

  const handleAssign = async () => {
    if (selectedEmployees.length === 0) {
      toast.error('Please select at least one employee');
      return;
    }

    try {
      await assignMutation.mutateAsync({
        courseId: course.id,
        employeeIds: selectedEmployees,
        dueDate: dueDate ? dueDate.toISOString() : undefined,
      });
      toast.success('Course assigned', {
        description: `Assigned "${course.title}" to ${selectedEmployees.length} employee${selectedEmployees.length !== 1 ? 's' : ''}`,
      });
      handleClose();
    } catch (error: unknown) {
      let message = 'Failed to assign course';
      if (error && typeof error === 'object' && 'response' in error) {
        const axiosError = error as { response?: { data?: { message?: string } } };
        if (axiosError.response?.data?.message) {
          message = axiosError.response.data.message;
        }
      } else if (error instanceof Error) {
        message = error.message;
      }
      toast.error('Error', { description: message });
    }
  };

  const handleClose = () => {
    setSelectedEmployees([]);
    setEmployeeSearch('');
    setDueDate(undefined);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Assign Course</DialogTitle>
          <DialogDescription>
            Assign &quot;{course.title}&quot; to employees.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          {/* Due Date */}
          <div className="space-y-2">
            <Label>Due Date (optional)</Label>
            <Popover>
              <PopoverTrigger asChild>
                <Button
                  variant="outline"
                  className={cn(
                    'w-full justify-start text-left font-normal',
                    !dueDate && 'text-muted-foreground'
                  )}
                >
                  <CalendarIcon className="mr-2 h-4 w-4" />
                  {dueDate ? format(dueDate, 'PPP') : 'Select a due date'}
                </Button>
              </PopoverTrigger>
              <PopoverContent className="w-auto p-0" align="start">
                <Calendar
                  mode="single"
                  selected={dueDate}
                  onSelect={setDueDate}
                  disabled={(date) => date < new Date()}
                  initialFocus
                />
              </PopoverContent>
            </Popover>
          </div>

          {/* Employee Selection */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <Label>Select Employees</Label>
              <div className="flex items-center gap-2">
                <Button type="button" variant="ghost" size="sm" onClick={handleSelectAll}>
                  Select All
                </Button>
                <Button type="button" variant="ghost" size="sm" onClick={handleClearAll}>
                  Clear
                </Button>
              </div>
            </div>

            {/* Search */}
            <div className="relative">
              <SearchIcon className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="Search employees..."
                value={employeeSearch}
                onChange={(e) => setEmployeeSearch(e.target.value)}
                className="pl-9"
              />
            </div>

            {/* Employee List */}
            <div className="rounded-md border max-h-[200px] overflow-y-auto">
              {employeesLoading ? (
                <div className="p-4 text-center text-sm text-muted-foreground">
                  Loading employees...
                </div>
              ) : filteredEmployees.length === 0 ? (
                <div className="p-4 text-center text-sm text-muted-foreground">
                  No employees found
                </div>
              ) : (
                filteredEmployees.map((employee) => (
                  <label
                    key={employee.id}
                    className="flex items-center gap-3 p-3 hover:bg-muted/50 cursor-pointer border-b last:border-b-0"
                  >
                    <Checkbox
                      checked={selectedEmployees.includes(employee.id)}
                      onCheckedChange={(checked) =>
                        handleEmployeeToggle(employee.id, !!checked)
                      }
                    />
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium">
                        {employee.firstName} {employee.lastName}
                      </p>
                      {employee.email && (
                        <p className="text-xs text-muted-foreground truncate">
                          {employee.email}
                        </p>
                      )}
                    </div>
                  </label>
                ))
              )}
            </div>

            {/* Selected count */}
            {selectedEmployees.length > 0 && (
              <div className="flex items-center gap-2 text-sm text-muted-foreground">
                <UsersIcon className="h-4 w-4" />
                <span>{selectedEmployees.length} employee{selectedEmployees.length !== 1 ? 's' : ''} selected</span>
              </div>
            )}
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={handleClose}>
            Cancel
          </Button>
          <Button
            onClick={handleAssign}
            disabled={selectedEmployees.length === 0 || assignMutation.isPending}
          >
            {assignMutation.isPending ? 'Assigning...' : `Assign to ${selectedEmployees.length} employee${selectedEmployees.length !== 1 ? 's' : ''}`}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
