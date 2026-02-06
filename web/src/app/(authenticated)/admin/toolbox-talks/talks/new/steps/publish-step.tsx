'use client';

import { useState, useMemo } from 'react';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { Badge } from '@/components/ui/badge';
import {
  Info,
  CheckCircle2,
  Loader2,
  BookOpen,
  HelpCircle,
  Video,
  FileText,
  AlertCircle,
  PartyPopper,
  Send,
  Save,
  Target,
  Clock,
  Sparkles,
} from 'lucide-react';
import { toast } from 'sonner';
import { updateToolboxTalk } from '@/lib/api/toolbox-talks/toolbox-talks';
import type { ToolboxTalkWizardData, GeneratedSection, GeneratedQuestion } from '../page';
import type {
  CreateToolboxTalkSectionRequest,
  CreateToolboxTalkQuestionRequest,
  ToolboxTalkStatus,
  QuestionType,
  VideoSource,
} from '@/types/toolbox-talks';

interface PublishStepProps {
  data: ToolboxTalkWizardData;
  updateData: (updates: Partial<ToolboxTalkWizardData>) => void;
  onBack: () => void;
  onComplete: () => void;
}

// Transform wizard section to API format
function transformSection(section: GeneratedSection): CreateToolboxTalkSectionRequest {
  console.log('[DEBUG] transformSection input:', {
    id: section.id,
    sortOrder: section.sortOrder,
    source: section.source,
    title: section.title?.substring(0, 40),
  });

  const result = {
    id: section.id,
    sectionNumber: section.sortOrder,
    title: section.title,
    content: section.content,
    requiresAcknowledgment: section.requiresAcknowledgment,
    source: section.source,
  };

  console.log('[DEBUG] transformSection output:', {
    id: result.id,
    sectionNumber: result.sectionNumber,
    source: result.source,
  });

  return result;
}

// Transform wizard question to API format
function transformQuestion(question: GeneratedQuestion): CreateToolboxTalkQuestionRequest {
  console.log('[DEBUG] transformQuestion input:', {
    id: question.id,
    sortOrder: question.sortOrder,
    source: question.source,
    questionType: question.questionType,
  });

  const result = {
    id: question.id,
    questionNumber: question.sortOrder,
    questionText: question.questionText,
    questionType: question.questionType as QuestionType,
    options: question.questionType === 'TrueFalse' ? undefined : question.options,
    correctAnswer: question.options[question.correctAnswerIndex],
    points: question.points,
    source: question.source,
  };

  console.log('[DEBUG] transformQuestion output:', {
    id: result.id,
    questionNumber: result.questionNumber,
    source: result.source,
  });

  return result;
}

// Map wizard video source to API video source
function mapVideoSource(source: string | null): VideoSource {
  switch (source) {
    case 'DirectUrl':
      return 'DirectUrl';
    case 'YouTube':
      return 'YouTube';
    case 'GoogleDrive':
      return 'GoogleDrive';
    case 'Vimeo':
      return 'Vimeo';
    default:
      return 'None';
  }
}

export function PublishStep({
  data,
  updateData,
  onBack,
  onComplete,
}: PublishStepProps) {
  const [isPublishing, setIsPublishing] = useState(false);
  const [isSavingDraft, setIsSavingDraft] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);

  const hasSections = data.sections.length > 0;
  const hasQuestions = data.questions.length > 0;
  const hasVideo = !!data.videoUrl;
  const hasPdf = !!data.pdfUrl;

  // Calculate quiz stats
  const totalPoints = useMemo(
    () => data.questions.reduce((sum, q) => sum + q.points, 0),
    [data.questions]
  );

  const passingPoints = useMemo(
    () => Math.ceil((data.passThreshold / 100) * totalPoints),
    [data.passThreshold, totalPoints]
  );

  // Calculate minimum correct answers needed (assuming equal points)
  const avgPointsPerQuestion = totalPoints / (data.questions.length || 1);
  const minCorrectAnswers = Math.ceil(passingPoints / avgPointsPerQuestion);

  // Validation checks
  const validationIssues: string[] = [];
  if (!hasSections) {
    validationIssues.push('At least one training section is required');
  }
  if (data.requiresQuiz && !hasQuestions) {
    validationIssues.push('Quiz is enabled but no questions have been added');
  }
  if (data.requiresQuiz && data.questions.length < 3) {
    validationIssues.push('At least 3 quiz questions are recommended');
  }
  if (data.passThreshold < 50 || data.passThreshold > 100) {
    validationIssues.push('Pass threshold must be between 50% and 100%');
  }

  // Check for incomplete sections/questions
  const incompleteSections = data.sections.filter(
    (s) => !s.title.trim() || !s.content.trim()
  );
  const incompleteQuestions = data.questions.filter(
    (q) =>
      !q.questionText.trim() ||
      q.options.some((o) => !o.trim()) ||
      q.correctAnswerIndex < 0
  );

  if (incompleteSections.length > 0) {
    validationIssues.push(
      `${incompleteSections.length} section(s) have missing title or content`
    );
  }
  if (incompleteQuestions.length > 0) {
    validationIssues.push(
      `${incompleteQuestions.length} question(s) have incomplete information`
    );
  }

  const canPublish = validationIssues.length === 0;
  // Allow saving as draft with fewer restrictions
  const canSaveDraft = hasSections || hasQuestions || data.title;

  const saveToolboxTalk = async (status: ToolboxTalkStatus) => {
    if (!data.id) {
      toast.error('Toolbox Talk not found', {
        description: 'Please go back to the first step and try again.',
      });
      return;
    }

    console.log('[DEBUG] saveToolboxTalk called with status:', status);
    console.log('[DEBUG] Sections in wizard state:', data.sections.map(s => ({
      id: s.id,
      sortOrder: s.sortOrder,
      source: s.source,
      title: s.title?.substring(0, 30),
    })));
    console.log('[DEBUG] Questions in wizard state:', data.questions.map(q => ({
      id: q.id,
      sortOrder: q.sortOrder,
      source: q.source,
      questionType: q.questionType,
    })));

    const transformedSections = data.sections.map(transformSection);
    const transformedQuestions = data.requiresQuiz ? data.questions.map(transformQuestion) : [];

    console.log('[DEBUG] Transformed sections to send:', transformedSections.map(s => ({
      id: s.id,
      sectionNumber: s.sectionNumber,
      source: s.source,
    })));
    console.log('[DEBUG] Transformed questions to send:', transformedQuestions.map(q => ({
      id: q.id,
      questionNumber: q.questionNumber,
      source: q.source,
    })));

    try {
      await updateToolboxTalk(data.id, {
        id: data.id,
        title: data.title,
        description: data.description || undefined,
        frequency: data.frequency,
        videoUrl: data.videoUrl || undefined,
        videoSource: mapVideoSource(data.videoSource),
        minimumVideoWatchPercent: data.minimumVideoWatchPercent,
        requiresQuiz: data.requiresQuiz,
        passingScore: data.requiresQuiz ? data.passThreshold : undefined,
        isActive: status === 'Published' ? data.isActive : false,
        sections: transformedSections,
        questions: transformedQuestions,
      });

      return true;
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Unknown error occurred';
      throw new Error(message);
    }
  };

  const handlePublish = async () => {
    setIsPublishing(true);
    try {
      await saveToolboxTalk('Published');
      setShowSuccess(true);
      toast.success('Toolbox Talk published successfully!', {
        description: 'It is now available for scheduling and assignments.',
      });

      // Wait a moment for the success animation, then redirect
      setTimeout(() => {
        onComplete();
      }, 2000);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to publish';
      toast.error('Failed to publish', { description: message });
      setIsPublishing(false);
    }
  };

  const handleSaveDraft = async () => {
    setIsSavingDraft(true);
    try {
      await saveToolboxTalk('Draft');
      toast.success('Draft saved successfully', {
        description: 'You can continue editing later.',
      });
      onComplete();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to save';
      toast.error('Failed to save draft', { description: message });
    } finally {
      setIsSavingDraft(false);
    }
  };

  // Success celebration screen
  if (showSuccess) {
    return (
      <div className="flex flex-col items-center justify-center py-12 space-y-6">
        <div className="relative">
          <div className="absolute inset-0 animate-ping">
            <PartyPopper className="h-16 w-16 text-primary/30" />
          </div>
          <PartyPopper className="h-16 w-16 text-primary" />
        </div>
        <div className="text-center space-y-2">
          <h2 className="text-2xl font-bold">Toolbox Talk Published!</h2>
          <p className="text-muted-foreground">
            Your training is now ready to be assigned to employees.
          </p>
        </div>
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <Loader2 className="h-4 w-4 animate-spin" />
          <span>Redirecting to talks list...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-lg font-semibold">Publish Settings</h2>
        <p className="text-sm text-muted-foreground">
          Review your Toolbox Talk and configure final settings before publishing
        </p>
      </div>

      {/* Summary Card */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-base flex items-center gap-2">
            <Sparkles className="h-4 w-4" />
            Summary
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Title and Description */}
          <div>
            <h4 className="text-lg font-semibold">{data.title || 'Untitled'}</h4>
            <p className="text-sm text-muted-foreground line-clamp-2">
              {data.description || 'No description provided'}
            </p>
            {data.category && (
              <Badge variant="secondary" className="mt-2">
                {data.category}
              </Badge>
            )}
          </div>

          <Separator />

          {/* Content Sources */}
          <div className="space-y-2">
            <h5 className="text-sm font-medium text-muted-foreground">Content Sources</h5>
            <div className="flex flex-wrap gap-3">
              {hasVideo ? (
                <div className="flex items-center gap-2 text-sm">
                  <div className="flex items-center justify-center h-8 w-8 rounded-full bg-blue-100 dark:bg-blue-900/30">
                    <Video className="h-4 w-4 text-blue-600 dark:text-blue-400" />
                  </div>
                  <div>
                    <span className="font-medium">Video</span>
                    <CheckCircle2 className="h-3 w-3 text-green-500 inline ml-1" />
                  </div>
                </div>
              ) : (
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <div className="flex items-center justify-center h-8 w-8 rounded-full bg-muted">
                    <Video className="h-4 w-4" />
                  </div>
                  <span>No video</span>
                </div>
              )}

              {hasPdf ? (
                <div className="flex items-center gap-2 text-sm">
                  <div className="flex items-center justify-center h-8 w-8 rounded-full bg-orange-100 dark:bg-orange-900/30">
                    <FileText className="h-4 w-4 text-orange-600 dark:text-orange-400" />
                  </div>
                  <div>
                    <span className="font-medium">PDF</span>
                    <CheckCircle2 className="h-3 w-3 text-green-500 inline ml-1" />
                  </div>
                </div>
              ) : (
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <div className="flex items-center justify-center h-8 w-8 rounded-full bg-muted">
                    <FileText className="h-4 w-4" />
                  </div>
                  <span>No PDF</span>
                </div>
              )}
            </div>
          </div>

          <Separator />

          {/* Stats Grid */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
            <div className="text-center p-3 rounded-lg bg-muted/50">
              <div className="flex items-center justify-center gap-1 mb-1">
                <BookOpen className="h-4 w-4 text-muted-foreground" />
              </div>
              <p className="text-2xl font-bold">{data.sections.length}</p>
              <p className="text-xs text-muted-foreground">Sections</p>
            </div>

            <div className="text-center p-3 rounded-lg bg-muted/50">
              <div className="flex items-center justify-center gap-1 mb-1">
                <HelpCircle className="h-4 w-4 text-muted-foreground" />
              </div>
              <p className="text-2xl font-bold">{data.questions.length}</p>
              <p className="text-xs text-muted-foreground">Questions</p>
            </div>

            <div className="text-center p-3 rounded-lg bg-muted/50">
              <div className="flex items-center justify-center gap-1 mb-1">
                <Target className="h-4 w-4 text-muted-foreground" />
              </div>
              <p className="text-2xl font-bold">{totalPoints}</p>
              <p className="text-xs text-muted-foreground">Total Points</p>
            </div>

            <div className="text-center p-3 rounded-lg bg-muted/50">
              <div className="flex items-center justify-center gap-1 mb-1">
                <Clock className="h-4 w-4 text-muted-foreground" />
              </div>
              <p className="text-2xl font-bold capitalize">{data.frequency}</p>
              <p className="text-xs text-muted-foreground">Frequency</p>
            </div>
          </div>

          {/* Final Portion Question Indicator */}
          {hasVideo && data.questions.some((q) => q.source === 'Video') && (
            <Alert className="bg-blue-50 border-blue-200 dark:bg-blue-950/30 dark:border-blue-900">
              <Info className="h-4 w-4 text-blue-600 dark:text-blue-400" />
              <AlertDescription className="text-blue-700 dark:text-blue-300">
                Includes quiz question(s) from video content to ensure complete viewing
              </AlertDescription>
            </Alert>
          )}
        </CardContent>
      </Card>

      {/* Settings */}
      <div className="space-y-4">
        <h3 className="font-medium">Quiz Settings</h3>

        <div className="flex items-center justify-between rounded-lg border p-4">
          <div className="space-y-0.5">
            <Label htmlFor="requiresQuiz" className="text-base">
              Require Quiz
            </Label>
            <p className="text-sm text-muted-foreground">
              Employees must pass a quiz to complete this training
            </p>
          </div>
          <Switch
            id="requiresQuiz"
            checked={data.requiresQuiz}
            onCheckedChange={(checked) => updateData({ requiresQuiz: checked })}
          />
        </div>

        {data.requiresQuiz && (
          <div className="space-y-4 rounded-lg border p-4">
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <Label htmlFor="passThreshold">Passing Score</Label>
                <span className="text-2xl font-bold text-primary">
                  {data.passThreshold}%
                </span>
              </div>

              {/* Custom slider using input range */}
              <div className="space-y-2">
                <input
                  id="passThreshold"
                  type="range"
                  min={50}
                  max={100}
                  step={5}
                  value={data.passThreshold}
                  onChange={(e) =>
                    updateData({ passThreshold: parseInt(e.target.value) || 80 })
                  }
                  className="w-full h-2 bg-muted rounded-lg appearance-none cursor-pointer accent-primary"
                />
                <div className="flex justify-between text-xs text-muted-foreground">
                  <span>50%</span>
                  <span>75%</span>
                  <span>100%</span>
                </div>
              </div>

              {/* Passing preview */}
              {hasQuestions && (
                <div className="p-3 rounded-lg bg-muted/50 text-sm">
                  <p className="font-medium">What this means:</p>
                  <p className="text-muted-foreground mt-1">
                    Employees must score at least{' '}
                    <span className="font-medium text-foreground">
                      {passingPoints} of {totalPoints} points
                    </span>
                    {data.questions.every((q) => q.points === 1) && (
                      <>
                        {' '}
                        ({minCorrectAnswers} of {data.questions.length} questions
                        correct)
                      </>
                    )}
                  </p>
                </div>
              )}
            </div>
          </div>
        )}

        {hasVideo && (
          <>
            <Separator />
            <h3 className="font-medium">Video Settings</h3>

            <div className="rounded-lg border p-4 space-y-4">
              <div className="flex items-center justify-between">
                <Label htmlFor="minimumVideoWatchPercent">
                  Minimum Watch Percentage
                </Label>
                <span className="text-2xl font-bold text-primary">
                  {data.minimumVideoWatchPercent}%
                </span>
              </div>

              <input
                id="minimumVideoWatchPercent"
                type="range"
                min={50}
                max={100}
                step={5}
                value={data.minimumVideoWatchPercent}
                onChange={(e) =>
                  updateData({
                    minimumVideoWatchPercent: parseInt(e.target.value) || 80,
                  })
                }
                className="w-full h-2 bg-muted rounded-lg appearance-none cursor-pointer accent-primary"
              />
              <div className="flex justify-between text-xs text-muted-foreground">
                <span>50%</span>
                <span>75%</span>
                <span>100%</span>
              </div>
              <p className="text-xs text-muted-foreground">
                Employees must watch at least this percentage of the video before
                proceeding
              </p>
            </div>
          </>
        )}

        <Separator />

        <div className="flex items-center justify-between rounded-lg border p-4">
          <div className="space-y-0.5">
            <Label htmlFor="isActive" className="text-base">
              Active Status
            </Label>
            <p className="text-sm text-muted-foreground">
              Active talks can be scheduled and assigned to employees
            </p>
          </div>
          <Switch
            id="isActive"
            checked={data.isActive}
            onCheckedChange={(checked) => updateData({ isActive: checked })}
          />
        </div>
      </div>

      {/* Validation Issues */}
      {validationIssues.length > 0 && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertTitle>Cannot Publish</AlertTitle>
          <AlertDescription>
            <ul className="list-disc list-inside mt-2 space-y-1">
              {validationIssues.map((issue, index) => (
                <li key={index}>{issue}</li>
              ))}
            </ul>
          </AlertDescription>
        </Alert>
      )}

      {/* Ready to Publish */}
      {canPublish && (
        <Alert className="border-green-200 bg-green-50 text-green-800 dark:border-green-800 dark:bg-green-950 dark:text-green-200">
          <CheckCircle2 className="h-4 w-4" />
          <AlertTitle>Ready to Publish</AlertTitle>
          <AlertDescription>
            Your Toolbox Talk is complete and ready to be published. Once
            published, it can be scheduled and assigned to employees.
          </AlertDescription>
        </Alert>
      )}

      <div className="flex justify-between pt-4 border-t">
        <Button
          type="button"
          variant="outline"
          onClick={onBack}
          disabled={isPublishing || isSavingDraft}
        >
          Back
        </Button>
        <div className="flex gap-2">
          <Button
            type="button"
            variant="outline"
            onClick={handleSaveDraft}
            disabled={isPublishing || isSavingDraft || !canSaveDraft}
          >
            {isSavingDraft ? (
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            ) : (
              <Save className="mr-2 h-4 w-4" />
            )}
            Save as Draft
          </Button>
          <Button
            onClick={handlePublish}
            disabled={!canPublish || isPublishing || isSavingDraft}
          >
            {isPublishing ? (
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            ) : (
              <Send className="mr-2 h-4 w-4" />
            )}
            Publish Toolbox Talk
          </Button>
        </div>
      </div>
    </div>
  );
}
