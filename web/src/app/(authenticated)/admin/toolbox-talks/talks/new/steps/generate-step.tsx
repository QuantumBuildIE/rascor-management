'use client';

import { useState, useEffect, useRef, useCallback, useMemo } from 'react';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { Checkbox } from '@/components/ui/checkbox';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Card, CardContent } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import {
  Sparkles,
  Video,
  FileText,
  Loader2,
  Check,
  AlertCircle,
  Info,
  Rocket,
  RefreshCw,
  ArrowRight,
} from 'lucide-react';
import type { ToolboxTalkWizardData, GeneratedSection, GeneratedQuestion } from '../page';
import { generateToolboxTalkContent, getToolboxTalk } from '@/lib/api/toolbox-talks/toolbox-talks';
import { getStoredToken } from '@/lib/api/client';
import { HubConnectionBuilder, HubConnection, LogLevel, HubConnectionState } from '@microsoft/signalr';

interface GenerateStepProps {
  data: ToolboxTalkWizardData;
  updateData: (updates: Partial<ToolboxTalkWizardData>) => void;
  onNext: () => void;
  onBack: () => void;
}

interface GenerationProgress {
  stage: string;
  percentComplete: number;
  message: string;
}

interface GenerationComplete {
  success: boolean;
  partialSuccess?: boolean;
  sectionsGenerated: number;
  questionsGenerated: number;
  hasFinalPortionQuestion: boolean;
  errors: string[];
  warnings: string[];
  totalTokensUsed: number;
  message?: string;
}

export function GenerateStep({ data, updateData, onNext, onBack }: GenerateStepProps) {
  // Local state for generation options
  const [includeVideo, setIncludeVideo] = useState(data.videoUrl ? true : false);
  const [includePdf, setIncludePdf] = useState(data.pdfUrl ? true : false);
  const [minimumSections, setMinimumSections] = useState(data.minimumSections);
  const [minimumQuestions, setMinimumQuestions] = useState(data.minimumQuestions);

  // Generation state
  const [isGenerating, setIsGenerating] = useState(false);
  const [progress, setProgress] = useState<GenerationProgress | null>(null);
  const [result, setResult] = useState<GenerationComplete | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [isConnecting, setIsConnecting] = useState(false);

  const connectionRef = useRef<HubConnection | null>(null);
  const hasConnectedRef = useRef(false);
  const lastProgressTimeRef = useRef<number>(Date.now());

  const hasVideo = !!data.videoUrl;
  const hasPdf = !!data.pdfUrl;
  const hasContent = hasVideo || hasPdf;

  // Check if content actually exists (regardless of generation status)
  // This allows proceeding if partial content was created even when generation "failed"
  const hasGeneratedContent = useMemo(() => {
    const hasSections = data.sections && data.sections.length > 0;
    const hasQuestions = data.questions && data.questions.length > 0;
    return hasSections || hasQuestions;
  }, [data.sections, data.questions]);

  // Can proceed if generation succeeded OR if content actually exists
  const canProceed = !isGenerating && (result?.success || hasGeneratedContent);

  // Set up SignalR connection
  const setupSignalR = useCallback(async () => {
    // Skip if already connected or connecting, or if there's no toolbox talk ID
    if (!data.id || hasConnectedRef.current || connectionRef.current?.state === HubConnectionState.Connected) {
      return;
    }

    setIsConnecting(true);

    try {
      const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5222';
      const token = getStoredToken('accessToken');

      if (!token) {
        setError('Authentication token not found. Please log in again.');
        setIsConnecting(false);
        return;
      }

      const connection = new HubConnectionBuilder()
        .withUrl(`${apiUrl}/hubs/content-generation`, {
          accessTokenFactory: () => token,
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(LogLevel.Information)
        .build();

      // Handle progress updates
      connection.on('ContentGenerationProgress', (payload: {
        toolboxTalkId: string;
        stage: string;
        percentComplete: number;
        message: string;
      }) => {
        if (payload.toolboxTalkId === data.id) {
          lastProgressTimeRef.current = Date.now();
          setProgress({
            stage: payload.stage,
            percentComplete: payload.percentComplete,
            message: payload.message,
          });
        }
      });

      // Handle completion
      connection.on('ContentGenerationComplete', async (payload: {
        toolboxTalkId: string;
        success: boolean;
        partialSuccess?: boolean;
        sectionsGenerated: number;
        questionsGenerated: number;
        hasFinalPortionQuestion: boolean;
        errors: string[];
        warnings: string[];
        totalTokensUsed: number;
        message?: string;
      }) => {
        if (payload.toolboxTalkId === data.id) {
          setResult({
            success: payload.success,
            partialSuccess: payload.partialSuccess,
            sectionsGenerated: payload.sectionsGenerated,
            questionsGenerated: payload.questionsGenerated,
            hasFinalPortionQuestion: payload.hasFinalPortionQuestion,
            errors: payload.errors,
            warnings: payload.warnings,
            totalTokensUsed: payload.totalTokensUsed,
            message: payload.message,
          });

          // Always try to fetch content, even on failure (partial content may exist)
          if (data.id) {
            try {
              const updatedTalk = await getToolboxTalk(data.id);

              // Check if any content was actually created
              const hasContent = (updatedTalk.sections?.length ?? 0) > 0 ||
                                 (updatedTalk.questions?.length ?? 0) > 0;

              if (hasContent) {
                // Map API sections to wizard GeneratedSection format
                const sections: GeneratedSection[] = updatedTalk.sections.map((s) => ({
                  id: s.id,
                  sortOrder: s.sectionNumber,
                  title: s.title,
                  content: s.content,
                  source: s.source || 'Manual',
                  requiresAcknowledgment: s.requiresAcknowledgment,
                }));

                // Map API questions to wizard GeneratedQuestion format
                const questions: GeneratedQuestion[] = updatedTalk.questions.map((q) => {
                  // Find the index of the correct answer in the options array
                  const correctAnswerIndex = q.options?.findIndex(opt => opt === q.correctAnswer) ?? 0;

                  // Map 'Both' to 'Video' since wizard type doesn't include 'Both'
                  const mapSource = (src: string | undefined): 'Video' | 'Pdf' | 'Manual' => {
                    if (src === 'Video' || src === 'Pdf' || src === 'Manual') return src;
                    if (src === 'Both') return 'Video'; // Default 'Both' to 'Video'
                    return 'Manual';
                  };

                  return {
                    id: q.id,
                    sortOrder: q.questionNumber,
                    questionText: q.questionText,
                    questionType: q.questionType === 'TrueFalse' ? 'TrueFalse' : 'MultipleChoice',
                    options: q.options || [],
                    correctAnswerIndex: correctAnswerIndex >= 0 ? correctAnswerIndex : 0,
                    source: mapSource(q.source),
                    points: q.points,
                  };
                });

                // Update wizard state with the fetched content
                updateData({
                  sections,
                  questions,
                  generateFromVideo: includeVideo,
                  generateFromPdf: includePdf,
                  minimumSections,
                  minimumQuestions,
                });

                console.log('✅ Loaded generated content:', {
                  sections: sections.length,
                  questions: questions.length,
                  wasSuccessful: payload.success
                });
              }
            } catch (error) {
              console.error('Failed to fetch generated content:', error);
              // Still show result, but user may need to refresh
            }
          }

          setIsGenerating(false);
        }
      });

      // Handle connection state changes
      connection.onreconnecting(() => {
        console.log('SignalR reconnecting...');
        setIsConnected(false);
      });

      connection.onreconnected(() => {
        console.log('SignalR reconnected');
        setIsConnected(true);
        // Re-subscribe after reconnection
        if (data.id) {
          connection.invoke('SubscribeToToolboxTalk', data.id);
        }
      });

      connection.onclose(() => {
        console.log('SignalR connection closed');
        setIsConnected(false);
        hasConnectedRef.current = false;
      });

      // Start the connection
      await connection.start();
      console.log('SignalR connected');

      // Subscribe to this toolbox talk's updates
      await connection.invoke('SubscribeToToolboxTalk', data.id);
      console.log('Subscribed to toolbox talk', data.id);

      connectionRef.current = connection;
      hasConnectedRef.current = true;
      setIsConnected(true);
      setError(null);
    } catch (err) {
      console.error('Failed to connect to SignalR:', err);
      setError('Failed to establish real-time connection. You can still start generation.');
    } finally {
      setIsConnecting(false);
    }
  }, [data.id]);

  // Connect to SignalR when component mounts and we have a toolbox talk ID
  useEffect(() => {
    if (data.id && !hasConnectedRef.current) {
      setupSignalR();
    }

    return () => {
      // Cleanup on unmount
      if (connectionRef.current) {
        if (data.id) {
          connectionRef.current.invoke('UnsubscribeFromToolboxTalk', data.id).catch(() => {});
        }
        connectionRef.current.stop();
        connectionRef.current = null;
        hasConnectedRef.current = false;
      }
    };
  }, [data.id, setupSignalR]);

  // Timeout fallback: if no SignalR updates received for 45 seconds during generation,
  // check status via API in case SignalR connection was lost
  useEffect(() => {
    if (!isGenerating || result) return;

    const checkInterval = setInterval(async () => {
      const timeSinceLastProgress = Date.now() - lastProgressTimeRef.current;

      // If no progress update in 45 seconds, check status via API
      if (timeSinceLastProgress > 45000 && data.id) {
        console.log('No SignalR updates received, checking status via API...');
        try {
          const updatedTalk = await getToolboxTalk(data.id);

          // Check if content was generated (sections or questions exist)
          if (updatedTalk.sections?.length > 0 || updatedTalk.questions?.length > 0) {
            console.log('Content was generated, SignalR may have missed completion event');

            // Content exists - generation succeeded but we missed the SignalR event
            setResult({
              success: true,
              partialSuccess: true,
              sectionsGenerated: updatedTalk.sections?.length || 0,
              questionsGenerated: updatedTalk.questions?.length || 0,
              hasFinalPortionQuestion: false, // Can't determine from API response
              errors: [],
              warnings: ['Progress updates were delayed, but content was generated successfully'],
              totalTokensUsed: 0,
              message: 'Content generated (status recovered)',
            });

            // Map and update wizard data
            const sections = updatedTalk.sections.map((s) => ({
              id: s.id,
              sortOrder: s.sectionNumber,
              title: s.title,
              content: s.content,
              source: s.source || 'Manual',
              requiresAcknowledgment: s.requiresAcknowledgment,
            }));

            const questions = updatedTalk.questions.map((q) => {
              const correctAnswerIndex = q.options?.findIndex(opt => opt === q.correctAnswer) ?? 0;
              const mapSource = (src: string | undefined): 'Video' | 'Pdf' | 'Manual' => {
                if (src === 'Video' || src === 'Pdf' || src === 'Manual') return src;
                if (src === 'Both') return 'Video';
                return 'Manual';
              };

              return {
                id: q.id,
                sortOrder: q.questionNumber,
                questionText: q.questionText,
                questionType: q.questionType === 'TrueFalse' ? 'TrueFalse' as const : 'MultipleChoice' as const,
                options: q.options || [],
                correctAnswerIndex: correctAnswerIndex >= 0 ? correctAnswerIndex : 0,
                source: mapSource(q.source),
                points: q.points,
              };
            });

            updateData({
              sections,
              questions,
              generateFromVideo: includeVideo,
              generateFromPdf: includePdf,
              minimumSections,
              minimumQuestions,
            });

            setIsGenerating(false);
          } else if (updatedTalk.status === 'Draft') {
            // Status is Draft - generation may have failed
            console.log('Toolbox talk status is Draft, generation may have failed');
            // Don't auto-fail yet, keep waiting for SignalR
          }
        } catch (error) {
          console.error('Status check failed:', error);
        }
      }
    }, 15000); // Check every 15 seconds

    return () => clearInterval(checkInterval);
  }, [isGenerating, result, data.id, includeVideo, includePdf, minimumSections, minimumQuestions, updateData]);

  const handleStartGeneration = async () => {
    if (!data.id) {
      setError('Toolbox Talk not found. Please go back and save the basic info first.');
      return;
    }

    if (!includeVideo && !includePdf) {
      setError('Please select at least one content source');
      return;
    }

    setIsGenerating(true);
    setError(null);
    setProgress(null);
    setResult(null);

    try {
      // STEP 1: Ensure SignalR is connected FIRST (before starting the job)
      let currentConnection = connectionRef.current;

      if (!currentConnection || currentConnection.state !== HubConnectionState.Connected) {
        console.log('SignalR not connected, establishing connection...');

        // If we have an existing connection that's not connected, stop it first
        if (currentConnection && currentConnection.state !== HubConnectionState.Disconnected) {
          try {
            await currentConnection.stop();
          } catch {
            // Ignore stop errors
          }
        }

        // Create a new connection
        const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5222';
        const token = getStoredToken('accessToken');

        if (!token) {
          throw new Error('Authentication token not found. Please log in again.');
        }

        currentConnection = new HubConnectionBuilder()
          .withUrl(`${apiUrl}/hubs/content-generation`, {
            accessTokenFactory: () => token,
          })
          .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
          .configureLogging(LogLevel.Information)
          .build();

        // Set up event handlers
        currentConnection.on('ContentGenerationProgress', (payload: {
          toolboxTalkId: string;
          stage: string;
          percentComplete: number;
          message: string;
        }) => {
          if (payload.toolboxTalkId === data.id) {
            lastProgressTimeRef.current = Date.now();
            setProgress({
              stage: payload.stage,
              percentComplete: payload.percentComplete,
              message: payload.message,
            });
          }
        });

        currentConnection.on('ContentGenerationComplete', async (payload: {
          toolboxTalkId: string;
          success: boolean;
          partialSuccess?: boolean;
          sectionsGenerated: number;
          questionsGenerated: number;
          hasFinalPortionQuestion: boolean;
          errors: string[];
          warnings: string[];
          totalTokensUsed: number;
          message?: string;
        }) => {
          if (payload.toolboxTalkId === data.id) {
            setResult({
              success: payload.success,
              partialSuccess: payload.partialSuccess,
              sectionsGenerated: payload.sectionsGenerated,
              questionsGenerated: payload.questionsGenerated,
              hasFinalPortionQuestion: payload.hasFinalPortionQuestion,
              errors: payload.errors,
              warnings: payload.warnings,
              totalTokensUsed: payload.totalTokensUsed,
              message: payload.message,
            });

            // Always try to fetch content, even on failure (partial content may exist)
            if (data.id) {
              try {
                const updatedTalk = await getToolboxTalk(data.id);

                // Check if any content was actually created
                const hasContent = (updatedTalk.sections?.length ?? 0) > 0 ||
                                   (updatedTalk.questions?.length ?? 0) > 0;

                if (hasContent) {
                  const sections: GeneratedSection[] = updatedTalk.sections.map((s) => ({
                    id: s.id,
                    sortOrder: s.sectionNumber,
                    title: s.title,
                    content: s.content,
                    source: s.source || 'Manual',
                    requiresAcknowledgment: s.requiresAcknowledgment,
                  }));

                  const questions: GeneratedQuestion[] = updatedTalk.questions.map((q) => {
                    const correctAnswerIndex = q.options?.findIndex(opt => opt === q.correctAnswer) ?? 0;
                    const mapSource = (src: string | undefined): 'Video' | 'Pdf' | 'Manual' => {
                      if (src === 'Video' || src === 'Pdf' || src === 'Manual') return src;
                      if (src === 'Both') return 'Video';
                      return 'Manual';
                    };

                    return {
                      id: q.id,
                      sortOrder: q.questionNumber,
                      questionText: q.questionText,
                      questionType: q.questionType === 'TrueFalse' ? 'TrueFalse' : 'MultipleChoice',
                      options: q.options || [],
                      correctAnswerIndex: correctAnswerIndex >= 0 ? correctAnswerIndex : 0,
                      source: mapSource(q.source),
                      points: q.points,
                    };
                  });

                  updateData({
                    sections,
                    questions,
                    generateFromVideo: includeVideo,
                    generateFromPdf: includePdf,
                    minimumSections,
                    minimumQuestions,
                  });

                  console.log('Loaded generated content:', {
                    sections: sections.length,
                    questions: questions.length,
                    wasSuccessful: payload.success
                  });
                }
              } catch (error) {
                console.error('Failed to fetch generated content:', error);
              }
            }

            setIsGenerating(false);
          }
        });

        currentConnection.onreconnecting(() => {
          console.log('SignalR reconnecting...');
          setIsConnected(false);
        });

        currentConnection.onreconnected(() => {
          console.log('SignalR reconnected');
          setIsConnected(true);
          if (data.id) {
            currentConnection?.invoke('SubscribeToToolboxTalk', data.id);
          }
        });

        currentConnection.onclose(() => {
          console.log('SignalR connection closed');
          setIsConnected(false);
          hasConnectedRef.current = false;
        });

        // Start the connection and wait for it
        await currentConnection.start();
        console.log('SignalR connected');

        connectionRef.current = currentConnection;
        hasConnectedRef.current = true;
        setIsConnected(true);
      }

      // STEP 2: Subscribe to this toolbox talk's updates
      await currentConnection.invoke('SubscribeToToolboxTalk', data.id);
      console.log('Subscribed to toolbox talk:', data.id);

      // STEP 3: Now start the generation job (connection is guaranteed)
      console.log('Starting content generation...');
      await generateToolboxTalkContent(data.id, {
        includeVideo,
        includePdf,
        minimumSections,
        minimumQuestions,
        passThreshold: data.passThreshold,
        replaceExisting: true,
        connectionId: currentConnection.connectionId || '',
      });

      // Show initial progress while waiting for SignalR updates
      setProgress({
        stage: 'Starting',
        percentComplete: 5,
        message: 'Starting content generation...',
      });
    } catch (err: unknown) {
      console.error('Generation error:', err);
      const errorMessage = err instanceof Error ? err.message : 'Failed to start content generation';
      setError(errorMessage);
      setIsGenerating(false);
    }
  };

  const handleNext = () => {
    // Update wizard data with generation settings and results
    updateData({
      generateFromVideo: includeVideo,
      generateFromPdf: includePdf,
      minimumSections,
      minimumQuestions,
    });
    onNext();
  };

  const handleRetry = () => {
    setResult(null);
    setProgress(null);
    setError(null);
  };

  const getStageIcon = (stage: string) => {
    switch (stage) {
      case 'Starting':
        return <Loader2 className="h-4 w-4 animate-spin" />;
      case 'Extracting':
        return <FileText className="h-4 w-4" />;
      case 'GeneratingSections':
        return <Sparkles className="h-4 w-4 animate-pulse" />;
      case 'GeneratingQuiz':
        return <Sparkles className="h-4 w-4 animate-pulse" />;
      case 'Saving':
        return <Loader2 className="h-4 w-4 animate-spin" />;
      case 'Complete':
        return <Check className="h-4 w-4 text-green-600" />;
      default:
        return <Loader2 className="h-4 w-4 animate-spin" />;
    }
  };

  const getStageName = (stage: string) => {
    switch (stage) {
      case 'Starting':
        return 'Initializing';
      case 'Extracting':
        return 'Extracting Content';
      case 'GeneratingSections':
        return 'Generating Sections';
      case 'GeneratingQuiz':
        return 'Generating Quiz';
      case 'Saving':
        return 'Saving Results';
      case 'Complete':
        return 'Complete';
      default:
        return stage;
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-lg font-semibold">Generate Content</h2>
        <p className="text-sm text-muted-foreground">
          Use AI to generate sections and quiz questions from your content
        </p>
      </div>

      {/* No content warning */}
      {!hasContent && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertTitle>No Content Available</AlertTitle>
          <AlertDescription>
            Please go back and add a video or PDF to generate content from.
          </AlertDescription>
        </Alert>
      )}

      {/* Connection status indicator */}
      {data.id && !isGenerating && !result && (
        <div className="flex items-center gap-2 text-sm">
          {isConnecting ? (
            <>
              <Loader2 className="h-3 w-3 animate-spin text-muted-foreground" />
              <span className="text-muted-foreground">Connecting to real-time updates...</span>
            </>
          ) : isConnected ? (
            <>
              <div className="h-2 w-2 rounded-full bg-green-500" />
              <span className="text-muted-foreground">Real-time updates connected</span>
            </>
          ) : (
            <>
              <div className="h-2 w-2 rounded-full bg-yellow-500" />
              <span className="text-muted-foreground">
                Real-time updates unavailable
                <Button variant="link" size="sm" className="p-0 ml-1 h-auto" onClick={setupSignalR}>
                  Retry
                </Button>
              </span>
            </>
          )}
        </div>
      )}

      {/* Source Selection - only show when not generating and no result */}
      {!isGenerating && !result && hasContent && (
        <Card>
          <CardContent className="pt-6 space-y-6">
            {/* Content Sources */}
            <div className="space-y-4">
              <h3 className="font-medium">Content Sources</h3>
              <p className="text-sm text-muted-foreground">
                Select which content to use for AI generation
              </p>

              <div className="space-y-3">
                <div className="flex items-center space-x-3">
                  <Checkbox
                    id="generateFromVideo"
                    checked={includeVideo}
                    onCheckedChange={(checked) => setIncludeVideo(checked === true)}
                    disabled={!hasVideo}
                  />
                  <div className="flex items-center gap-2">
                    <Video className="h-4 w-4 text-muted-foreground" />
                    <Label
                      htmlFor="generateFromVideo"
                      className={!hasVideo ? 'text-muted-foreground' : ''}
                    >
                      Generate from video transcript
                      {!hasVideo && ' (no video added)'}
                    </Label>
                  </div>
                </div>

                <div className="flex items-center space-x-3">
                  <Checkbox
                    id="generateFromPdf"
                    checked={includePdf}
                    onCheckedChange={(checked) => setIncludePdf(checked === true)}
                    disabled={!hasPdf}
                  />
                  <div className="flex items-center gap-2">
                    <FileText className="h-4 w-4 text-muted-foreground" />
                    <Label
                      htmlFor="generateFromPdf"
                      className={!hasPdf ? 'text-muted-foreground' : ''}
                    >
                      Generate from PDF content
                      {!hasPdf && ' (no PDF added)'}
                    </Label>
                  </div>
                </div>
              </div>
            </div>

            {/* Generation Settings */}
            <div className="space-y-4 pt-4 border-t">
              <h3 className="font-medium">Generation Settings</h3>

              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="minimumSections">Minimum Sections</Label>
                  <Input
                    id="minimumSections"
                    type="number"
                    min={3}
                    max={20}
                    value={minimumSections}
                    onChange={(e) => setMinimumSections(parseInt(e.target.value) || 7)}
                  />
                  <p className="text-xs text-muted-foreground">
                    AI will generate at least this many training sections
                  </p>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="minimumQuestions">Minimum Quiz Questions</Label>
                  <Input
                    id="minimumQuestions"
                    type="number"
                    min={3}
                    max={15}
                    value={minimumQuestions}
                    onChange={(e) => setMinimumQuestions(parseInt(e.target.value) || 5)}
                  />
                  <p className="text-xs text-muted-foreground">
                    AI will generate at least this many quiz questions
                  </p>
                </div>
              </div>
            </div>

            {/* Final portion question info */}
            {includeVideo && (
              <Alert>
                <Info className="h-4 w-4" />
                <AlertDescription>
                  At least one quiz question will be generated from the final portion
                  of the video (80-100%) to ensure employees watch the entire content.
                </AlertDescription>
              </Alert>
            )}
          </CardContent>
        </Card>
      )}

      {/* Generation Progress */}
      {isGenerating && progress && (
        <Card>
          <CardContent className="pt-6">
            <div className="space-y-4">
              {/* Stage indicator */}
              <div className="flex items-center gap-3">
                {getStageIcon(progress.stage)}
                <div className="flex-1">
                  <p className="font-medium">{getStageName(progress.stage)}</p>
                  <p className="text-sm text-muted-foreground">{progress.message}</p>
                </div>
                <span className="text-sm font-medium text-muted-foreground">
                  {progress.percentComplete}%
                </span>
              </div>

              {/* Progress bar */}
              <Progress value={progress.percentComplete} className="h-2" />

              {/* Stages indicator */}
              <div className="flex justify-between text-xs text-muted-foreground">
                <span className={progress.percentComplete >= 10 ? 'text-primary' : ''}>Extract</span>
                <span className={progress.percentComplete >= 35 ? 'text-primary' : ''}>Sections</span>
                <span className={progress.percentComplete >= 65 ? 'text-primary' : ''}>Quiz</span>
                <span className={progress.percentComplete >= 88 ? 'text-primary' : ''}>Save</span>
              </div>

              {/* Info message */}
              <div className="text-center text-sm text-muted-foreground pt-2 border-t">
                <p>This process usually takes 1-2 minutes.</p>
                <p className="mt-1">
                  You can leave this page - generation will continue in the background.
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Generation Result */}
      {result && (
        <Card className={
          result.success && !result.partialSuccess
            ? 'border-green-200 bg-green-50/50'
            : result.success && result.partialSuccess
            ? 'border-yellow-200 bg-yellow-50/50'
            : 'border-red-200 bg-red-50/50'
        }>
          <CardContent className="pt-6">
            {result.success ? (
              <div className="space-y-4">
                {/* Success/Partial Success header */}
                <div className={`flex items-center gap-2 ${result.partialSuccess ? 'text-yellow-700' : 'text-green-700'}`}>
                  {result.partialSuccess ? (
                    <Info className="h-5 w-5" />
                  ) : (
                    <Check className="h-5 w-5" />
                  )}
                  <span className="font-medium">
                    {result.partialSuccess
                      ? 'Content Generated with Some Limitations'
                      : 'Content Generated Successfully!'}
                  </span>
                </div>

                {/* Partial success explanation */}
                {result.partialSuccess && (
                  <p className="text-sm text-yellow-700">
                    Some content sources were unavailable, but content was successfully generated from available sources.
                  </p>
                )}

                {/* Stats */}
                <div className="grid grid-cols-2 gap-4">
                  <div className="p-4 bg-white rounded-lg border">
                    <p className="text-3xl font-bold text-primary">{result.sectionsGenerated}</p>
                    <p className="text-sm text-muted-foreground">Sections created</p>
                  </div>
                  <div className="p-4 bg-white rounded-lg border">
                    <p className="text-3xl font-bold text-primary">{result.questionsGenerated}</p>
                    <p className="text-sm text-muted-foreground">Quiz questions</p>
                  </div>
                </div>

                {/* Final portion question badge */}
                {result.hasFinalPortionQuestion && (
                  <Alert className="bg-green-100 border-green-200">
                    <Check className="h-4 w-4 text-green-700" />
                    <AlertDescription className="text-green-700">
                      Includes a question from the video&apos;s final portion (80-100%) to ensure full viewing
                    </AlertDescription>
                  </Alert>
                )}

                {/* Warnings */}
                {result.warnings.length > 0 && (
                  <Alert className="bg-yellow-50 border-yellow-200">
                    <Info className="h-4 w-4 text-yellow-700" />
                    <AlertTitle className="text-yellow-800">
                      {result.partialSuccess ? 'Source Limitations' : 'Notes'}
                    </AlertTitle>
                    <AlertDescription className="text-yellow-700">
                      <ul className="list-disc list-inside space-y-1 mt-2">
                        {result.warnings.map((warning, i) => (
                          <li key={i}>{warning}</li>
                        ))}
                      </ul>
                    </AlertDescription>
                  </Alert>
                )}
              </div>
            ) : (
              <div className="space-y-4">
                {/* Failure header */}
                <div className="flex items-center gap-2 text-red-700">
                  <AlertCircle className="h-5 w-5" />
                  <span className="font-medium">
                    {result.message || 'Content Generation Failed'}
                  </span>
                </div>

                {/* Errors */}
                <Alert variant="destructive">
                  <AlertCircle className="h-4 w-4" />
                  <AlertDescription>
                    <ul className="list-disc list-inside space-y-1">
                      {result.errors.map((err, i) => (
                        <li key={i}>{err}</li>
                      ))}
                    </ul>
                  </AlertDescription>
                </Alert>

                {/* Show helpful message if content exists despite "failure" */}
                {hasGeneratedContent && (
                  <Alert className="bg-blue-50 border-blue-200">
                    <Info className="h-4 w-4 text-blue-600" />
                    <AlertTitle className="text-blue-800">Content was partially generated</AlertTitle>
                    <AlertDescription className="text-blue-700">
                      <p className="mt-1">
                        Some content sources weren&apos;t available, but {data.sections?.length || 0} sections
                        and {data.questions?.length || 0} quiz questions were created.
                        You can continue to review and edit the content.
                      </p>
                    </AlertDescription>
                  </Alert>
                )}

                {/* Retry button */}
                <Button onClick={handleRetry} variant="outline" className="gap-2">
                  <RefreshCw className="h-4 w-4" />
                  Try Again
                </Button>
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Error alert */}
      {error && !result && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      {/* Action buttons */}
      <div className="flex justify-between pt-4 border-t">
        <Button
          type="button"
          variant="outline"
          onClick={onBack}
          disabled={isGenerating}
        >
          Back
        </Button>

        <div className="flex gap-2">
          {/* Show generate button when ready */}
          {!isGenerating && !result && hasContent && (
            <Button
              onClick={handleStartGeneration}
              disabled={!includeVideo && !includePdf}
              className="gap-2"
            >
              <Rocket className="h-4 w-4" />
              Generate Sections &amp; Quiz
            </Button>
          )}

          {/* Show next button after successful generation OR if content exists */}
          {canProceed && (
            <Button onClick={handleNext} className="gap-2">
              {result?.success ? 'Next: Review Content' : 'Continue to Review'}
              <ArrowRight className="h-4 w-4" />
            </Button>
          )}

          {/* Show generating state */}
          {isGenerating && (
            <Button disabled className="gap-2">
              <Loader2 className="h-4 w-4 animate-spin" />
              Generating...
            </Button>
          )}

          {/* Skip button when no content sources available */}
          {!hasContent && !hasGeneratedContent && (
            <Button onClick={onNext} variant="outline">
              Skip to Review
            </Button>
          )}

          {/* Skip & Add Manually button when generation failed and no content exists */}
          {result && !result.success && !hasGeneratedContent && (
            <Button onClick={onNext} variant="ghost" className="gap-2">
              Skip &amp; Add Manually
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}
