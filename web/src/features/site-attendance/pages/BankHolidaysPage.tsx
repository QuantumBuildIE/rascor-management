'use client';

import { useState } from 'react';
import { format, isSameDay, startOfMonth, endOfMonth, eachDayOfInterval, getDay, addMonths, subMonths, isToday } from 'date-fns';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { Label } from '@/components/ui/label';
import { useBankHolidays, useCreateBankHoliday, useDeleteBankHoliday, type BankHoliday } from '../hooks/useSiteAttendance';
import { toast } from 'sonner';
import { ChevronLeft, ChevronRight, Plus, Trash2, Calendar as CalendarIcon } from 'lucide-react';
import { cn } from '@/lib/utils';

const WEEKDAYS = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

export default function BankHolidaysPage() {
  const [currentMonth, setCurrentMonth] = useState(new Date());
  const [selectedDate, setSelectedDate] = useState<Date | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [holidayToDelete, setHolidayToDelete] = useState<BankHoliday | null>(null);
  const [newHolidayName, setNewHolidayName] = useState('');

  const { data: holidays, isLoading } = useBankHolidays();
  const createHoliday = useCreateBankHoliday();
  const deleteHoliday = useDeleteBankHoliday();

  const monthStart = startOfMonth(currentMonth);
  const monthEnd = endOfMonth(currentMonth);
  const daysInMonth = eachDayOfInterval({ start: monthStart, end: monthEnd });

  // Get the day of the week for the first day (0 = Sunday, we want Monday = 0)
  const startDayOfWeek = (getDay(monthStart) + 6) % 7;

  const getHolidayForDate = (date: Date): BankHoliday | undefined => {
    return holidays?.find((h) => isSameDay(new Date(h.date), date));
  };

  const handlePreviousMonth = () => {
    setCurrentMonth(subMonths(currentMonth, 1));
  };

  const handleNextMonth = () => {
    setCurrentMonth(addMonths(currentMonth, 1));
  };

  const handleDateClick = (date: Date) => {
    const existingHoliday = getHolidayForDate(date);
    if (existingHoliday) {
      setHolidayToDelete(existingHoliday);
      setDeleteDialogOpen(true);
    } else {
      setSelectedDate(date);
      setNewHolidayName('');
      setDialogOpen(true);
    }
  };

  const handleCreateHoliday = async () => {
    if (!selectedDate) return;

    try {
      await createHoliday.mutateAsync({
        date: format(selectedDate, 'yyyy-MM-dd'),
        name: newHolidayName || undefined,
      });
      toast.success('Bank holiday added');
      setDialogOpen(false);
      setSelectedDate(null);
      setNewHolidayName('');
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Failed to add holiday';
      toast.error('Error', { description: message });
    }
  };

  const handleDeleteHoliday = async () => {
    if (!holidayToDelete) return;

    try {
      await deleteHoliday.mutateAsync(holidayToDelete.id);
      toast.success('Bank holiday removed');
      setDeleteDialogOpen(false);
      setHolidayToDelete(null);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Failed to remove holiday';
      toast.error('Error', { description: message });
    }
  };

  const upcomingHolidays = holidays
    ?.filter((h) => new Date(h.date) >= new Date())
    .sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime())
    .slice(0, 5);

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold">Bank Holidays</h1>
          <p className="text-muted-foreground">Manage bank holidays for attendance calculations</p>
        </div>
        <div className="grid gap-6 lg:grid-cols-3">
          <Card className="lg:col-span-2">
            <CardHeader>
              <Skeleton className="h-6 w-32" />
            </CardHeader>
            <CardContent>
              <Skeleton className="h-64 w-full" />
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <Skeleton className="h-6 w-32" />
            </CardHeader>
            <CardContent>
              <Skeleton className="h-48 w-full" />
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Bank Holidays</h1>
          <p className="text-muted-foreground">Manage bank holidays for attendance calculations</p>
        </div>
        <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
          <DialogTrigger asChild>
            <Button onClick={() => { setSelectedDate(new Date()); setNewHolidayName(''); }}>
              <Plus className="mr-2 h-4 w-4" />
              Add Holiday
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Add Bank Holiday</DialogTitle>
              <DialogDescription>
                Add a new bank holiday to exclude from attendance calculations.
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="date">Date</Label>
                <Input
                  id="date"
                  type="date"
                  value={selectedDate ? format(selectedDate, 'yyyy-MM-dd') : ''}
                  onChange={(e) => setSelectedDate(e.target.value ? new Date(e.target.value) : null)}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="name">Holiday Name (optional)</Label>
                <Input
                  id="name"
                  placeholder="e.g., Christmas Day"
                  value={newHolidayName}
                  onChange={(e) => setNewHolidayName(e.target.value)}
                />
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleCreateHoliday} disabled={!selectedDate || createHoliday.isPending}>
                {createHoliday.isPending ? 'Adding...' : 'Add Holiday'}
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        {/* Calendar */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="flex items-center gap-2">
                <CalendarIcon className="h-5 w-5" />
                {format(currentMonth, 'MMMM yyyy')}
              </CardTitle>
              <div className="flex gap-1">
                <Button variant="outline" size="icon" onClick={handlePreviousMonth}>
                  <ChevronLeft className="h-4 w-4" />
                </Button>
                <Button variant="outline" size="icon" onClick={handleNextMonth}>
                  <ChevronRight className="h-4 w-4" />
                </Button>
              </div>
            </div>
            <CardDescription>
              Click on a date to add or remove a bank holiday
            </CardDescription>
          </CardHeader>
          <CardContent>
            {/* Weekday headers */}
            <div className="grid grid-cols-7 gap-1 mb-2">
              {WEEKDAYS.map((day) => (
                <div
                  key={day}
                  className="text-center text-sm font-medium text-muted-foreground py-2"
                >
                  {day}
                </div>
              ))}
            </div>

            {/* Calendar grid */}
            <div className="grid grid-cols-7 gap-1">
              {/* Empty cells for days before the first of the month */}
              {Array.from({ length: startDayOfWeek }).map((_, index) => (
                <div key={`empty-${index}`} className="aspect-square" />
              ))}

              {/* Days of the month */}
              {daysInMonth.map((date) => {
                const holiday = getHolidayForDate(date);
                const dayOfWeek = getDay(date);
                const isWeekend = dayOfWeek === 0 || dayOfWeek === 6;

                return (
                  <button
                    key={date.toISOString()}
                    onClick={() => handleDateClick(date)}
                    className={cn(
                      'aspect-square flex flex-col items-center justify-center rounded-lg text-sm transition-colors relative',
                      'hover:bg-muted cursor-pointer',
                      isToday(date) && 'ring-2 ring-primary ring-offset-2',
                      isWeekend && !holiday && 'text-muted-foreground',
                      holiday && 'bg-red-100 text-red-900 hover:bg-red-200 dark:bg-red-900/20 dark:text-red-400 dark:hover:bg-red-900/30'
                    )}
                    title={holiday?.name || undefined}
                  >
                    <span className="font-medium">{format(date, 'd')}</span>
                    {holiday && (
                      <span className="absolute bottom-1 w-1.5 h-1.5 rounded-full bg-red-600" />
                    )}
                  </button>
                );
              })}
            </div>

            {/* Legend */}
            <div className="flex items-center gap-6 mt-6 pt-4 border-t text-sm text-muted-foreground">
              <div className="flex items-center gap-2">
                <div className="w-4 h-4 rounded bg-red-100 dark:bg-red-900/20" />
                <span>Bank Holiday</span>
              </div>
              <div className="flex items-center gap-2">
                <div className="w-4 h-4 rounded ring-2 ring-primary ring-offset-2" />
                <span>Today</span>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Upcoming Holidays List */}
        <Card>
          <CardHeader>
            <CardTitle>Upcoming Holidays</CardTitle>
            <CardDescription>
              Next {upcomingHolidays?.length || 0} bank holidays
            </CardDescription>
          </CardHeader>
          <CardContent>
            {upcomingHolidays && upcomingHolidays.length > 0 ? (
              <div className="space-y-3">
                {upcomingHolidays.map((holiday) => (
                  <div
                    key={holiday.id}
                    className="flex items-center justify-between p-3 rounded-lg border bg-muted/50"
                  >
                    <div>
                      <div className="font-medium">
                        {format(new Date(holiday.date), 'EEEE, d MMMM yyyy')}
                      </div>
                      {holiday.name && (
                        <div className="text-sm text-muted-foreground">{holiday.name}</div>
                      )}
                    </div>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="text-destructive hover:text-destructive"
                      onClick={() => {
                        setHolidayToDelete(holiday);
                        setDeleteDialogOpen(true);
                      }}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground text-center py-8">
                No upcoming bank holidays
              </p>
            )}

            {/* All holidays count */}
            <div className="mt-6 pt-4 border-t">
              <div className="flex items-center justify-between text-sm">
                <span className="text-muted-foreground">Total holidays this year</span>
                <Badge variant="secondary">
                  {holidays?.filter((h) => new Date(h.date).getFullYear() === new Date().getFullYear()).length || 0}
                </Badge>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Remove Bank Holiday</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to remove this bank holiday?
              {holidayToDelete && (
                <span className="block mt-2 font-medium text-foreground">
                  {format(new Date(holidayToDelete.date), 'EEEE, d MMMM yyyy')}
                  {holidayToDelete.name && ` - ${holidayToDelete.name}`}
                </span>
              )}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDeleteHoliday}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {deleteHoliday.isPending ? 'Removing...' : 'Remove'}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
