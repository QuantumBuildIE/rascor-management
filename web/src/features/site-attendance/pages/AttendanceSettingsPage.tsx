'use client';

import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Checkbox } from '@/components/ui/checkbox';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { useAttendanceSettings, useUpdateAttendanceSettings } from '../hooks/useSiteAttendance';
import { toast } from 'sonner';
import { Settings, Bell, Clock, MapPin } from 'lucide-react';

const settingsSchema = z.object({
  expectedHoursPerDay: z.number().min(0).max(24),
  workStartTime: z.string().regex(/^\d{2}:\d{2}$/, 'Invalid time format (HH:MM)'),
  lateThresholdMinutes: z.number().min(0).max(480),
  includeSaturday: z.boolean(),
  includeSunday: z.boolean(),
  geofenceRadiusMeters: z.number().min(10).max(5000),
  noiseThresholdMeters: z.number().min(0).max(10000),
  spaGracePeriodMinutes: z.number().min(0).max(120),
  enablePushNotifications: z.boolean(),
  enableEmailNotifications: z.boolean(),
  enableSmsNotifications: z.boolean(),
  notificationTitle: z.string().min(1).max(100),
  notificationMessage: z.string().min(1).max(500),
});

type SettingsFormData = z.infer<typeof settingsSchema>;

export default function AttendanceSettingsPage() {
  const { data: settings, isLoading } = useAttendanceSettings();
  const updateSettings = useUpdateAttendanceSettings();

  const form = useForm<SettingsFormData>({
    resolver: zodResolver(settingsSchema) as any,
    defaultValues: {
      expectedHoursPerDay: 7.5,
      workStartTime: '08:00',
      lateThresholdMinutes: 15,
      includeSaturday: false,
      includeSunday: false,
      geofenceRadiusMeters: 100,
      noiseThresholdMeters: 500,
      spaGracePeriodMinutes: 30,
      enablePushNotifications: true,
      enableEmailNotifications: false,
      enableSmsNotifications: false,
      notificationTitle: 'Attendance Reminder',
      notificationMessage: 'Please check in at your assigned site.',
    },
  });

  useEffect(() => {
    if (settings) {
      form.reset({
        expectedHoursPerDay: settings.expectedHoursPerDay,
        workStartTime: settings.workStartTime,
        lateThresholdMinutes: settings.lateThresholdMinutes,
        includeSaturday: settings.includeSaturday,
        includeSunday: settings.includeSunday,
        geofenceRadiusMeters: settings.geofenceRadiusMeters,
        noiseThresholdMeters: settings.noiseThresholdMeters,
        spaGracePeriodMinutes: settings.spaGracePeriodMinutes,
        enablePushNotifications: settings.enablePushNotifications,
        enableEmailNotifications: settings.enableEmailNotifications,
        enableSmsNotifications: settings.enableSmsNotifications,
        notificationTitle: settings.notificationTitle,
        notificationMessage: settings.notificationMessage,
      });
    }
  }, [settings, form]);

  const onSubmit = async (data: SettingsFormData) => {
    try {
      await updateSettings.mutateAsync(data);
      toast.success('Settings updated successfully');
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Failed to update settings';
      toast.error('Error', { description: message });
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold">Attendance Settings</h1>
          <p className="text-muted-foreground">Configure site attendance tracking parameters</p>
        </div>
        <div className="grid gap-6 lg:grid-cols-2">
          {[1, 2, 3, 4].map((i) => (
            <Card key={i}>
              <CardHeader>
                <Skeleton className="h-6 w-32" />
                <Skeleton className="h-4 w-48" />
              </CardHeader>
              <CardContent className="space-y-4">
                <Skeleton className="h-10 w-full" />
                <Skeleton className="h-10 w-full" />
                <Skeleton className="h-10 w-full" />
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Attendance Settings</h1>
        <p className="text-muted-foreground">Configure site attendance tracking parameters</p>
      </div>

      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
          <div className="grid gap-6 lg:grid-cols-2">
            {/* Working Hours Settings */}
            <Card>
              <CardHeader>
                <div className="flex items-center gap-2">
                  <Clock className="h-5 w-5 text-muted-foreground" />
                  <CardTitle>Working Hours</CardTitle>
                </div>
                <CardDescription>
                  Configure expected working hours and schedule
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <FormField
                  control={form.control}
                  name="expectedHoursPerDay"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Expected Hours Per Day</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          step="0.5"
                          {...field}
                          onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                        />
                      </FormControl>
                      <FormDescription>
                        Standard working hours expected per day
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="workStartTime"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Work Start Time</FormLabel>
                      <FormControl>
                        <Input type="time" {...field} />
                      </FormControl>
                      <FormDescription>
                        Standard shift start time
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="lateThresholdMinutes"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Late Threshold (minutes)</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          {...field}
                          onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                        />
                      </FormControl>
                      <FormDescription>
                        Minutes after start time to mark as late
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <div className="space-y-3 pt-2">
                  <FormField
                    control={form.control}
                    name="includeSaturday"
                    render={({ field }) => (
                      <FormItem className="flex flex-row items-center space-x-3 space-y-0">
                        <FormControl>
                          <Checkbox
                            checked={field.value}
                            onCheckedChange={field.onChange}
                          />
                        </FormControl>
                        <FormLabel className="font-normal">
                          Include Saturday as working day
                        </FormLabel>
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="includeSunday"
                    render={({ field }) => (
                      <FormItem className="flex flex-row items-center space-x-3 space-y-0">
                        <FormControl>
                          <Checkbox
                            checked={field.value}
                            onCheckedChange={field.onChange}
                          />
                        </FormControl>
                        <FormLabel className="font-normal">
                          Include Sunday as working day
                        </FormLabel>
                      </FormItem>
                    )}
                  />
                </div>
              </CardContent>
            </Card>

            {/* Geofencing Settings */}
            <Card>
              <CardHeader>
                <div className="flex items-center gap-2">
                  <MapPin className="h-5 w-5 text-muted-foreground" />
                  <CardTitle>Geofencing</CardTitle>
                </div>
                <CardDescription>
                  Configure location tracking parameters
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <FormField
                  control={form.control}
                  name="geofenceRadiusMeters"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Geofence Radius (meters)</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          {...field}
                          onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                        />
                      </FormControl>
                      <FormDescription>
                        Distance from site center to trigger attendance
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="noiseThresholdMeters"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Noise Threshold (meters)</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          {...field}
                          onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                        />
                      </FormControl>
                      <FormDescription>
                        Distance beyond which events are marked as noise
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="spaGracePeriodMinutes"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>SPA Grace Period (minutes)</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          {...field}
                          onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                        />
                      </FormControl>
                      <FormDescription>
                        Time allowed for short site absences
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </CardContent>
            </Card>

            {/* Notification Settings */}
            <Card className="lg:col-span-2">
              <CardHeader>
                <div className="flex items-center gap-2">
                  <Bell className="h-5 w-5 text-muted-foreground" />
                  <CardTitle>Notifications</CardTitle>
                </div>
                <CardDescription>
                  Configure attendance notification channels and messages
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="grid gap-6 lg:grid-cols-2">
                  <div className="space-y-4">
                    <div className="space-y-3">
                      <FormField
                        control={form.control}
                        name="enablePushNotifications"
                        render={({ field }) => (
                          <FormItem className="flex flex-row items-center space-x-3 space-y-0">
                            <FormControl>
                              <Checkbox
                                checked={field.value}
                                onCheckedChange={field.onChange}
                              />
                            </FormControl>
                            <FormLabel className="font-normal">
                              Enable Push Notifications
                            </FormLabel>
                          </FormItem>
                        )}
                      />

                      <FormField
                        control={form.control}
                        name="enableEmailNotifications"
                        render={({ field }) => (
                          <FormItem className="flex flex-row items-center space-x-3 space-y-0">
                            <FormControl>
                              <Checkbox
                                checked={field.value}
                                onCheckedChange={field.onChange}
                              />
                            </FormControl>
                            <FormLabel className="font-normal">
                              Enable Email Notifications
                            </FormLabel>
                          </FormItem>
                        )}
                      />

                      <FormField
                        control={form.control}
                        name="enableSmsNotifications"
                        render={({ field }) => (
                          <FormItem className="flex flex-row items-center space-x-3 space-y-0">
                            <FormControl>
                              <Checkbox
                                checked={field.value}
                                onCheckedChange={field.onChange}
                              />
                            </FormControl>
                            <FormLabel className="font-normal">
                              Enable SMS Notifications
                            </FormLabel>
                          </FormItem>
                        )}
                      />
                    </div>
                  </div>

                  <div className="space-y-4">
                    <FormField
                      control={form.control}
                      name="notificationTitle"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Notification Title</FormLabel>
                          <FormControl>
                            <Input {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="notificationMessage"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Notification Message</FormLabel>
                          <FormControl>
                            <Input {...field} />
                          </FormControl>
                          <FormDescription>
                            Default message sent with attendance reminders
                          </FormDescription>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          <div className="flex justify-end">
            <Button type="submit" disabled={updateSettings.isPending}>
              {updateSettings.isPending ? 'Saving...' : 'Save Settings'}
            </Button>
          </div>
        </form>
      </Form>
    </div>
  );
}

