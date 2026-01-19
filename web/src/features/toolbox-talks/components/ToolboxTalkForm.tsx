'use client';

import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Checkbox } from '@/components/ui/checkbox';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
  FormDescription,
} from '@/components/ui/form';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Switch } from '@/components/ui/switch';
import { SectionEditor } from './SectionEditor';
import { QuestionEditor } from './QuestionEditor';
import { useCreateToolboxTalk, useUpdateToolboxTalk } from '@/lib/api/toolbox-talks';
import type {
  ToolboxTalk,
  ToolboxTalkFrequency,
  VideoSource,
  QuestionType,
} from '@/types/toolbox-talks';
import { toast } from 'sonner';

// ============================================
// Form Schema
// ============================================

const sectionSchema = z.object({
  sectionNumber: z.number().min(1),
  title: z.string().min(1, 'Title is required'),
  content: z.string().min(1, 'Content is required'),
  requiresAcknowledgment: z.boolean(),
});

const questionSchema = z.object({
  questionNumber: z.number().min(1),
  questionText: z.string().min(1, 'Question text is required'),
  questionType: z.enum(['MultipleChoice', 'TrueFalse', 'ShortAnswer'] as const),
  options: z.array(z.string()).nullable(),
  correctAnswer: z.string().min(1, 'Correct answer is required'),
  points: z.number().min(1),
});

const toolboxTalkFormSchema = z.object({
  title: z.string().min(1, 'Title is required').max(200),
  description: z.string().max(1000).optional().nullable(),
  frequency: z.enum(['Once', 'Weekly', 'Monthly', 'Annually'] as const),
  videoUrl: z.string().url('Must be a valid URL').optional().nullable().or(z.literal('')),
  videoSource: z.enum(['None', 'YouTube', 'GoogleDrive', 'Vimeo', 'DirectUrl'] as const),
  attachmentUrl: z.string().url('Must be a valid URL').optional().nullable().or(z.literal('')),
  minimumVideoWatchPercent: z.number().min(0).max(100),
  requiresQuiz: z.boolean(),
  passingScore: z.number().min(0).max(100).optional().nullable(),
  isActive: z.boolean(),
  sections: z.array(sectionSchema),
  questions: z.array(questionSchema).optional(),
}).refine(
  (data) => {
    // Sections are required only if no video is provided (matches backend validation)
    const hasVideo = data.videoUrl && data.videoUrl.trim() !== '' && data.videoSource !== 'None';
    if (!hasVideo && data.sections.length === 0) {
      return false;
    }
    return true;
  },
  {
    message: 'At least one section is required when no video is provided',
    path: ['sections'],
  }
);

type ToolboxTalkFormValues = z.infer<typeof toolboxTalkFormSchema>;

// ============================================
// Component
// ============================================

const FREQUENCY_OPTIONS: { value: ToolboxTalkFrequency; label: string }[] = [
  { value: 'Once', label: 'Once' },
  { value: 'Weekly', label: 'Weekly' },
  { value: 'Monthly', label: 'Monthly' },
  { value: 'Annually', label: 'Annually' },
];

const VIDEO_SOURCE_OPTIONS: { value: VideoSource; label: string }[] = [
  { value: 'None', label: 'No Video' },
  { value: 'YouTube', label: 'YouTube' },
  { value: 'Vimeo', label: 'Vimeo' },
  { value: 'GoogleDrive', label: 'Google Drive' },
  { value: 'DirectUrl', label: 'Direct URL' },
];

interface ToolboxTalkFormProps {
  talk?: ToolboxTalk;
  onSuccess?: (talk: ToolboxTalk) => void;
  onCancel?: () => void;
}

export function ToolboxTalkForm({ talk, onSuccess, onCancel }: ToolboxTalkFormProps) {
  const isEditing = !!talk;

  const createMutation = useCreateToolboxTalk();
  const updateMutation = useUpdateToolboxTalk();

  const form = useForm<ToolboxTalkFormValues>({
    resolver: zodResolver(toolboxTalkFormSchema) as any,
    defaultValues: {
      title: talk?.title ?? '',
      description: talk?.description ?? '',
      frequency: talk?.frequency ?? 'Once',
      videoUrl: talk?.videoUrl ?? '',
      videoSource: talk?.videoSource ?? 'None',
      attachmentUrl: talk?.attachmentUrl ?? '',
      minimumVideoWatchPercent: talk?.minimumVideoWatchPercent ?? 80,
      requiresQuiz: talk?.requiresQuiz ?? false,
      passingScore: talk?.passingScore ?? 70,
      isActive: talk?.isActive ?? true,
      sections: talk?.sections?.map((s) => ({
        sectionNumber: s.sectionNumber,
        title: s.title,
        content: s.content,
        requiresAcknowledgment: s.requiresAcknowledgment,
      })) ?? [],
      questions: talk?.questions?.map((q) => ({
        questionNumber: q.questionNumber,
        questionText: q.questionText,
        questionType: q.questionType as QuestionType,
        options: q.options,
        correctAnswer: q.correctAnswer ?? '',
        points: q.points,
      })) ?? [],
    },
  });

  const watchRequiresQuiz = form.watch('requiresQuiz');
  const watchVideoSource = form.watch('videoSource');

  // Reset passing score when quiz is disabled
  useEffect(() => {
    if (!watchRequiresQuiz) {
      form.setValue('passingScore', null);
    } else if (form.getValues('passingScore') === null) {
      form.setValue('passingScore', 70);
    }
  }, [watchRequiresQuiz, form]);

  const isSubmitting = createMutation.isPending || updateMutation.isPending;

  async function onSubmit(values: ToolboxTalkFormValues) {
    // Custom validation for quiz requirements
    if (values.requiresQuiz) {
      if (!values.questions || values.questions.length === 0) {
        form.setError('questions', { message: 'At least one question is required when quiz is enabled' });
        return;
      }
      if (values.passingScore == null) {
        form.setError('passingScore', { message: 'Passing score is required when quiz is enabled' });
        return;
      }
    }

    try {
      // Transform form values to API request format
      const sections = values.sections.map((s) => ({
        sectionNumber: s.sectionNumber,
        title: s.title,
        content: s.content,
        requiresAcknowledgment: s.requiresAcknowledgment,
      }));

      const questions = values.requiresQuiz && values.questions
        ? values.questions.map((q) => ({
            questionNumber: q.questionNumber,
            questionText: q.questionText,
            questionType: q.questionType,
            options: q.options?.filter(Boolean) ?? undefined,
            correctAnswer: q.correctAnswer,
            points: q.points,
          }))
        : undefined;

      const requestData = {
        title: values.title,
        description: values.description || undefined,
        frequency: values.frequency,
        videoUrl: values.videoUrl || undefined,
        videoSource: values.videoSource,
        attachmentUrl: values.attachmentUrl || undefined,
        minimumVideoWatchPercent: values.minimumVideoWatchPercent,
        requiresQuiz: values.requiresQuiz,
        passingScore: values.requiresQuiz && values.passingScore != null ? values.passingScore : undefined,
        isActive: values.isActive,
        sections,
        questions,
      };

      if (isEditing && talk) {
        const result = await updateMutation.mutateAsync({
          id: talk.id,
          data: {
            id: talk.id,
            ...requestData,
          },
        });
        toast.success('Toolbox talk updated successfully');
        onSuccess?.(result);
      } else {
        const result = await createMutation.mutateAsync(requestData);
        toast.success('Toolbox talk created successfully');
        onSuccess?.(result);
      }
    } catch (error) {
      const message = error instanceof Error ? error.message : 'An error occurred';
      toast.error(isEditing ? 'Failed to update toolbox talk' : 'Failed to create toolbox talk', {
        description: message,
      });
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        {/* Basic Information */}
        <Card>
          <CardHeader>
            <CardTitle>Basic Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <FormField
                control={form.control}
                name="title"
                render={({ field }) => (
                  <FormItem className="sm:col-span-2">
                    <FormLabel>Title *</FormLabel>
                    <FormControl>
                      <Input placeholder="Enter toolbox talk title..." {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="description"
                render={({ field }) => (
                  <FormItem className="sm:col-span-2">
                    <FormLabel>Description</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder="Enter a brief description..."
                        className="min-h-[100px]"
                        {...field}
                        value={field.value ?? ''}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="frequency"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Frequency *</FormLabel>
                    <Select value={field.value} onValueChange={field.onChange}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select frequency" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {FREQUENCY_OPTIONS.map((option) => (
                          <SelectItem key={option.value} value={option.value}>
                            {option.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormDescription>
                      How often should employees complete this talk?
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="isActive"
                render={({ field }) => (
                  <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                    <div className="space-y-0.5">
                      <FormLabel className="text-base">Active</FormLabel>
                      <FormDescription>
                        Only active talks can be scheduled
                      </FormDescription>
                    </div>
                    <FormControl>
                      <Switch
                        checked={field.value}
                        onCheckedChange={field.onChange}
                      />
                    </FormControl>
                  </FormItem>
                )}
              />
            </div>
          </CardContent>
        </Card>

        {/* Video Settings */}
        <Card>
          <CardHeader>
            <CardTitle>Video Content</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <FormField
                control={form.control}
                name="videoSource"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Video Source</FormLabel>
                    <Select value={field.value} onValueChange={field.onChange}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select source" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {VIDEO_SOURCE_OPTIONS.map((option) => (
                          <SelectItem key={option.value} value={option.value}>
                            {option.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {watchVideoSource !== 'None' && (
                <FormField
                  control={form.control}
                  name="minimumVideoWatchPercent"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Minimum Watch %</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          min={0}
                          max={100}
                          {...field}
                          onChange={(e) => field.onChange(Number(e.target.value))}
                        />
                      </FormControl>
                      <FormDescription>
                        Required percentage of video to watch
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              )}

              {watchVideoSource !== 'None' && (
                <FormField
                  control={form.control}
                  name="videoUrl"
                  render={({ field }) => (
                    <FormItem className="sm:col-span-2">
                      <FormLabel>Video URL</FormLabel>
                      <FormControl>
                        <Input
                          placeholder="https://..."
                          {...field}
                          value={field.value ?? ''}
                        />
                      </FormControl>
                      <FormDescription>
                        {watchVideoSource === 'YouTube' && 'Enter YouTube video URL'}
                        {watchVideoSource === 'Vimeo' && 'Enter Vimeo video URL'}
                        {watchVideoSource === 'GoogleDrive' && 'Enter Google Drive video URL'}
                        {watchVideoSource === 'DirectUrl' && 'Enter direct video file URL'}
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              )}

              <FormField
                control={form.control}
                name="attachmentUrl"
                render={({ field }) => (
                  <FormItem className="sm:col-span-2">
                    <FormLabel>Attachment URL</FormLabel>
                    <FormControl>
                      <Input
                        placeholder="https://..."
                        {...field}
                        value={field.value ?? ''}
                      />
                    </FormControl>
                    <FormDescription>
                      Link to downloadable attachment (PDF, document, etc.)
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
          </CardContent>
        </Card>

        {/* Quiz Settings */}
        <Card>
          <CardHeader>
            <CardTitle>Quiz Settings</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <FormField
              control={form.control}
              name="requiresQuiz"
              render={({ field }) => (
                <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                  <FormControl>
                    <Checkbox
                      checked={field.value}
                      onCheckedChange={field.onChange}
                    />
                  </FormControl>
                  <div className="space-y-1 leading-none">
                    <FormLabel>Requires Quiz</FormLabel>
                    <FormDescription>
                      Employees must pass a quiz to complete this toolbox talk.
                    </FormDescription>
                  </div>
                </FormItem>
              )}
            />

            {watchRequiresQuiz && (
              <FormField
                control={form.control}
                name="passingScore"
                render={({ field }) => (
                  <FormItem className="max-w-xs">
                    <FormLabel>Passing Score (%) *</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        min={0}
                        max={100}
                        {...field}
                        value={field.value ?? 70}
                        onChange={(e) => field.onChange(Number(e.target.value))}
                      />
                    </FormControl>
                    <FormDescription>
                      Minimum percentage required to pass the quiz
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
            )}
          </CardContent>
        </Card>

        {/* Sections */}
        <SectionEditor form={form} fieldName="sections" />

        {/* Questions (conditional) */}
        {watchRequiresQuiz && (
          <QuestionEditor form={form} fieldName="questions" />
        )}

        {/* Form actions */}
        <div className="flex justify-end gap-4">
          {onCancel && (
            <Button type="button" variant="outline" onClick={onCancel}>
              Cancel
            </Button>
          )}
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? (
              <>
                <LoadingSpinner className="mr-2 h-4 w-4" />
                {isEditing ? 'Updating...' : 'Creating...'}
              </>
            ) : isEditing ? (
              'Update Toolbox Talk'
            ) : (
              'Create Toolbox Talk'
            )}
          </Button>
        </div>
      </form>
    </Form>
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

