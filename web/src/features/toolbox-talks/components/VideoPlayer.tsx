'use client';

import * as React from 'react';
import {
  Play,
  Pause,
  Volume2,
  VolumeX,
  Maximize,
  Check,
  AlertCircle,
  Loader2,
  Subtitles,
  MessageSquareOff,
  Download,
  Languages,
} from 'lucide-react';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { cn } from '@/lib/utils';
import { useVideoSubtitles, type AvailableSubtitle } from '@/lib/api/toolbox-talks/use-video-subtitles';
import { downloadSrtFile } from '@/lib/api/toolbox-talks/subtitle-processing';
import { downloadMySrtFile } from '@/lib/api/toolbox-talks/my-toolbox-talks';
import type { VideoSource } from '@/types/toolbox-talks';

interface VideoPlayerProps {
  videoUrl: string;
  videoSource: VideoSource;
  minimumWatchPercent: number;
  currentWatchPercent: number | null;
  onProgressUpdate: (percent: number) => Promise<void>;
  /** The toolbox talk ID for fetching subtitles (admin context) */
  toolboxTalkId?: string;
  /** The scheduled talk ID for fetching subtitles (employee context) */
  scheduledTalkId?: string;
  /** Employee's preferred language code (e.g., 'es', 'pl') */
  preferredLanguageCode?: string;
  className?: string;
}

// Extract video ID from various URL formats
function getVideoEmbedUrl(url: string, source: VideoSource): string | null {
  if (!url) return null;

  switch (source) {
    case 'YouTube': {
      // Handle various YouTube URL formats
      const patterns = [
        /(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/embed\/)([a-zA-Z0-9_-]{11})/,
        /youtube\.com\/v\/([a-zA-Z0-9_-]{11})/,
      ];
      for (const pattern of patterns) {
        const match = url.match(pattern);
        if (match) {
          return `https://www.youtube.com/embed/${match[1]}?enablejsapi=1&origin=${typeof window !== 'undefined' ? window.location.origin : ''}`;
        }
      }
      return url;
    }
    case 'Vimeo': {
      // Handle Vimeo URL formats
      const match = url.match(/vimeo\.com\/(\d+)/);
      if (match) {
        return `https://player.vimeo.com/video/${match[1]}`;
      }
      return url;
    }
    case 'GoogleDrive': {
      // Handle Google Drive URL formats
      const match = url.match(/drive\.google\.com\/file\/d\/([a-zA-Z0-9_-]+)/);
      if (match) {
        return `https://drive.google.com/file/d/${match[1]}/preview`;
      }
      return url;
    }
    case 'DirectUrl':
      return url;
    default:
      return url;
  }
}

// Subtitle track selector component
interface SubtitleSelectorProps {
  subtitles: AvailableSubtitle[];
  activeSubtitle: AvailableSubtitle | null;
  captionsEnabled: boolean;
  onSubtitleChange: (subtitle: AvailableSubtitle | null) => void;
  onToggleCaptions: () => void;
  isLoading?: boolean;
}

function SubtitleSelector({
  subtitles,
  activeSubtitle,
  captionsEnabled,
  onSubtitleChange,
  onToggleCaptions,
  isLoading,
}: SubtitleSelectorProps) {
  if (isLoading) {
    return (
      <Button variant="ghost" size="icon" className="text-white hover:bg-white/20" disabled>
        <Loader2 className="h-5 w-5 animate-spin" />
      </Button>
    );
  }

  if (subtitles.length === 0) {
    return null;
  }

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" className="text-white hover:bg-white/20">
          {captionsEnabled ? (
            <Subtitles className="h-5 w-5" />
          ) : (
            <MessageSquareOff className="h-5 w-5" />
          )}
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="min-w-[160px]">
        <DropdownMenuItem onClick={onToggleCaptions}>
          <div className="flex items-center gap-2 w-full">
            {captionsEnabled ? (
              <MessageSquareOff className="h-4 w-4" />
            ) : (
              <Subtitles className="h-4 w-4" />
            )}
            <span>{captionsEnabled ? 'Turn off captions' : 'Turn on captions'}</span>
          </div>
        </DropdownMenuItem>
        {subtitles.length > 1 && (
          <>
            <DropdownMenuSeparator />
            <div className="px-2 py-1.5 text-xs font-medium text-muted-foreground">
              Languages
            </div>
            {subtitles.map((subtitle) => (
              <DropdownMenuItem
                key={subtitle.languageCode}
                onClick={() => onSubtitleChange(subtitle)}
                className="flex items-center justify-between"
              >
                <span>{subtitle.languageName}</span>
                {activeSubtitle?.languageCode === subtitle.languageCode && (
                  <Check className="h-4 w-4" />
                )}
              </DropdownMenuItem>
            ))}
          </>
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

// Embedded video subtitle notice
interface EmbeddedSubtitleNoticeProps {
  subtitles: AvailableSubtitle[];
  toolboxTalkId: string;
  /** If provided, uses employee-specific download endpoint */
  scheduledTalkId?: string;
}

function EmbeddedSubtitleNotice({ subtitles, toolboxTalkId, scheduledTalkId }: EmbeddedSubtitleNoticeProps) {
  const [downloading, setDownloading] = React.useState<string | null>(null);

  if (subtitles.length === 0) {
    return null;
  }

  const handleDownload = async (languageCode: string) => {
    try {
      setDownloading(languageCode);
      // Use employee-specific endpoint if scheduledTalkId is provided
      if (scheduledTalkId) {
        await downloadMySrtFile(scheduledTalkId, languageCode);
      } else {
        await downloadSrtFile(toolboxTalkId, languageCode);
      }
    } catch (error) {
      console.error('Failed to download subtitle:', error);
    } finally {
      setDownloading(null);
    }
  };

  return (
    <div className="flex items-start gap-2 p-3 bg-muted/50 rounded-lg text-sm">
      <Languages className="h-4 w-4 mt-0.5 text-muted-foreground flex-shrink-0" />
      <div className="flex-1 min-w-0">
        <p className="font-medium">Subtitles available</p>
        <p className="text-muted-foreground text-xs mt-0.5">
          This embedded video may have its own captions. Translated subtitles are available for download:
        </p>
        <div className="flex flex-wrap gap-1.5 mt-2">
          {subtitles.map((subtitle) => (
            <button
              key={subtitle.languageCode}
              onClick={() => handleDownload(subtitle.languageCode)}
              disabled={downloading === subtitle.languageCode}
              className="inline-flex items-center gap-1 px-2 py-1 text-xs bg-background border rounded hover:bg-accent transition-colors disabled:opacity-50"
            >
              {downloading === subtitle.languageCode ? (
                <Loader2 className="h-3 w-3 animate-spin" />
              ) : (
                <Download className="h-3 w-3" />
              )}
              {subtitle.languageName}
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}

export function VideoPlayer({
  videoUrl,
  videoSource,
  minimumWatchPercent,
  currentWatchPercent,
  onProgressUpdate,
  toolboxTalkId,
  scheduledTalkId,
  preferredLanguageCode,
  className,
}: VideoPlayerProps) {
  const videoRef = React.useRef<HTMLVideoElement>(null);
  const [isPlaying, setIsPlaying] = React.useState(false);
  const [isMuted, setIsMuted] = React.useState(false);
  const [watchPercent, setWatchPercent] = React.useState(currentWatchPercent || 0);
  const [isUpdating, setIsUpdating] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);
  const lastReportedPercent = React.useRef(currentWatchPercent || 0);

  // Subtitle state
  const [captionsEnabled, setCaptionsEnabled] = React.useState(true);
  const [activeSubtitle, setActiveSubtitle] = React.useState<AvailableSubtitle | null>(null);

  const embedUrl = getVideoEmbedUrl(videoUrl, videoSource);
  const isEmbedded = videoSource !== 'DirectUrl';
  const requirementMet = watchPercent >= minimumWatchPercent;

  // Fetch available subtitles
  // If scheduledTalkId is provided, use employee-specific endpoints
  // Otherwise, use admin endpoints with toolboxTalkId
  const useEmployeeEndpoint = !!scheduledTalkId;
  const subtitleId = scheduledTalkId || toolboxTalkId || '';
  const {
    subtitles,
    preferredSubtitle,
    isLoading: subtitlesLoading,
    hasSubtitles,
  } = useVideoSubtitles(subtitleId, preferredLanguageCode, useEmployeeEndpoint);

  // Set active subtitle to preferred language when loaded
  React.useEffect(() => {
    if (preferredSubtitle && !activeSubtitle) {
      setActiveSubtitle(preferredSubtitle);
    } else if (subtitles.length > 0 && !activeSubtitle) {
      // Default to first available (usually English)
      setActiveSubtitle(subtitles[0]);
    }
  }, [preferredSubtitle, subtitles, activeSubtitle]);

  // Update video track when subtitle changes
  React.useEffect(() => {
    const video = videoRef.current;
    if (!video || isEmbedded) return;

    // Get all text tracks
    const tracks = video.textTracks;
    for (let i = 0; i < tracks.length; i++) {
      const track = tracks[i];
      if (activeSubtitle && track.language === activeSubtitle.languageCode) {
        track.mode = captionsEnabled ? 'showing' : 'hidden';
      } else {
        track.mode = 'hidden';
      }
    }
  }, [activeSubtitle, captionsEnabled, isEmbedded]);

  // Handle video progress for direct URLs
  const handleTimeUpdate = React.useCallback(() => {
    const video = videoRef.current;
    if (!video || video.duration === 0) return;

    const percent = Math.round((video.currentTime / video.duration) * 100);
    // Only update if percentage increased (prevent going back)
    if (percent > watchPercent) {
      setWatchPercent(percent);
    }
  }, [watchPercent]);

  // Report progress to server periodically
  React.useEffect(() => {
    if (watchPercent <= lastReportedPercent.current) return;
    if (isUpdating) return;

    // Report every 10% increment
    const shouldReport =
      Math.floor(watchPercent / 10) > Math.floor(lastReportedPercent.current / 10) ||
      (watchPercent >= minimumWatchPercent && lastReportedPercent.current < minimumWatchPercent);

    if (shouldReport) {
      setIsUpdating(true);
      onProgressUpdate(watchPercent)
        .then(() => {
          lastReportedPercent.current = watchPercent;
        })
        .catch(() => {
          // Silently fail - will retry on next update
        })
        .finally(() => {
          setIsUpdating(false);
        });
    }
  }, [watchPercent, minimumWatchPercent, onProgressUpdate, isUpdating]);

  // For embedded videos, we track time manually via an interval
  React.useEffect(() => {
    if (!isEmbedded || !isPlaying) return;

    // For embedded videos, increment progress while playing
    // This is a simplified approach since we can't directly access the video time
    const interval = setInterval(() => {
      setWatchPercent((prev) => {
        // Increment by ~1% every 6 seconds (assuming ~10 min video)
        const newPercent = Math.min(prev + 0.5, 100);
        return newPercent;
      });
    }, 3000);

    return () => clearInterval(interval);
  }, [isEmbedded, isPlaying]);

  const handlePlay = () => {
    if (videoRef.current) {
      videoRef.current.play();
      setIsPlaying(true);
    } else if (isEmbedded) {
      // For embedded videos, just toggle state
      setIsPlaying(true);
    }
  };

  const handlePause = () => {
    if (videoRef.current) {
      videoRef.current.pause();
      setIsPlaying(false);
    } else if (isEmbedded) {
      setIsPlaying(false);
    }
  };

  const toggleMute = () => {
    if (videoRef.current) {
      videoRef.current.muted = !videoRef.current.muted;
      setIsMuted(!isMuted);
    }
  };

  const handleFullscreen = () => {
    if (videoRef.current) {
      videoRef.current.requestFullscreen();
    }
  };

  const handleToggleCaptions = () => {
    setCaptionsEnabled(!captionsEnabled);
  };

  const handleSubtitleChange = (subtitle: AvailableSubtitle | null) => {
    setActiveSubtitle(subtitle);
    if (subtitle) {
      setCaptionsEnabled(true);
    }
  };

  if (!embedUrl) {
    return (
      <Card className={className}>
        <CardContent className="flex items-center justify-center h-64">
          <div className="text-center text-muted-foreground">
            <AlertCircle className="h-8 w-8 mx-auto mb-2" />
            <p>Invalid video URL</p>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className={className}>
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <CardTitle className="text-base">Training Video</CardTitle>
          {requirementMet ? (
            <div className="flex items-center gap-1 text-sm text-green-600">
              <Check className="h-4 w-4" />
              <span>Requirement met</span>
            </div>
          ) : (
            <div className="text-sm text-muted-foreground">
              Watch at least {minimumWatchPercent}% to continue
            </div>
          )}
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Video container */}
        <div className="relative aspect-video bg-black rounded-lg overflow-hidden">
          {isEmbedded ? (
            <iframe
              src={embedUrl}
              className="w-full h-full"
              allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
              allowFullScreen
              title="Training Video"
              onLoad={() => setError(null)}
              // Note: onError for iframes doesn't reliably detect content loading issues
              // for cross-origin iframes like Google Drive, YouTube, etc.
              // The iframe will still load the page even if Google Drive shows an error.
            />
          ) : (
            <video
              ref={videoRef}
              src={embedUrl}
              className="w-full h-full"
              onTimeUpdate={handleTimeUpdate}
              onPlay={() => setIsPlaying(true)}
              onPause={() => setIsPlaying(false)}
              onError={() => setError('Failed to load video')}
              onLoadedData={() => setError(null)}
              controls={false}
              crossOrigin="anonymous"
            >
              {/* Render subtitle tracks */}
              {subtitles.map((subtitle, index) => (
                <track
                  key={subtitle.languageCode}
                  kind="subtitles"
                  src={subtitle.vttUrl}
                  srcLang={subtitle.languageCode}
                  label={subtitle.languageName}
                  default={index === 0 && captionsEnabled}
                />
              ))}
            </video>
          )}

          {/* Error overlay */}
          {error && (
            <div className="absolute inset-0 flex items-center justify-center bg-black/80">
              <div className="text-center text-white">
                <AlertCircle className="h-8 w-8 mx-auto mb-2" />
                <p>{error}</p>
              </div>
            </div>
          )}

          {/* Custom controls for direct video (hidden for embedded) */}
          {!isEmbedded && !error && (
            <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/80 to-transparent p-4">
              <div className="flex items-center justify-between gap-4">
                <div className="flex items-center gap-2">
                  <Button
                    variant="ghost"
                    size="icon"
                    className="text-white hover:bg-white/20"
                    onClick={isPlaying ? handlePause : handlePlay}
                  >
                    {isPlaying ? (
                      <Pause className="h-5 w-5" />
                    ) : (
                      <Play className="h-5 w-5" />
                    )}
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="text-white hover:bg-white/20"
                    onClick={toggleMute}
                  >
                    {isMuted ? (
                      <VolumeX className="h-5 w-5" />
                    ) : (
                      <Volume2 className="h-5 w-5" />
                    )}
                  </Button>
                </div>
                <div className="flex items-center gap-2">
                  {/* Subtitle selector */}
                  {subtitleId && (
                    <SubtitleSelector
                      subtitles={subtitles}
                      activeSubtitle={activeSubtitle}
                      captionsEnabled={captionsEnabled}
                      onSubtitleChange={handleSubtitleChange}
                      onToggleCaptions={handleToggleCaptions}
                      isLoading={subtitlesLoading}
                    />
                  )}
                  <Button
                    variant="ghost"
                    size="icon"
                    className="text-white hover:bg-white/20"
                    onClick={handleFullscreen}
                  >
                    <Maximize className="h-5 w-5" />
                  </Button>
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Subtitle notice for embedded videos */}
        {isEmbedded && subtitleId && hasSubtitles && (
          <EmbeddedSubtitleNotice subtitles={subtitles} toolboxTalkId={subtitleId} scheduledTalkId={scheduledTalkId} />
        )}

        {/* Progress tracking */}
        <div className="space-y-2">
          <div className="flex items-center justify-between text-sm">
            <span className="text-muted-foreground">
              Video Progress
              {isUpdating && <Loader2 className="inline-block ml-2 h-3 w-3 animate-spin" />}
            </span>
            <span
              className={cn(
                'font-medium',
                requirementMet ? 'text-green-600' : 'text-foreground'
              )}
            >
              {Math.round(watchPercent)}% watched
            </span>
          </div>
          <div className="relative">
            <Progress value={watchPercent} className="h-2" />
            {/* Minimum requirement marker */}
            <div
              className="absolute top-0 bottom-0 w-0.5 bg-yellow-500"
              style={{ left: `${minimumWatchPercent}%` }}
            />
          </div>
          <div className="flex justify-between text-xs text-muted-foreground">
            <span>0%</span>
            <span
              className="text-yellow-600 font-medium"
              style={{ marginLeft: `${minimumWatchPercent - 20}%` }}
            >
              {minimumWatchPercent}% required
            </span>
            <span>100%</span>
          </div>
        </div>

        {/* Instructions for embedded videos */}
        {isEmbedded && (
          <p className="text-xs text-muted-foreground text-center">
            Click on the video player to start. Progress will be tracked automatically.
          </p>
        )}

        {/* Subtitle indicator for direct video */}
        {!isEmbedded && hasSubtitles && activeSubtitle && (
          <div className="flex items-center gap-2 text-xs text-muted-foreground">
            <Subtitles className="h-3.5 w-3.5" />
            <span>
              Captions: {activeSubtitle.languageName}
              {!captionsEnabled && ' (off)'}
            </span>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

// Skeleton for loading state
export function VideoPlayerSkeleton() {
  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="h-5 w-32 bg-muted rounded animate-pulse" />
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="aspect-video bg-muted rounded-lg animate-pulse" />
        <div className="space-y-2">
          <div className="flex justify-between">
            <div className="h-4 w-24 bg-muted rounded animate-pulse" />
            <div className="h-4 w-16 bg-muted rounded animate-pulse" />
          </div>
          <div className="h-2 w-full bg-muted rounded animate-pulse" />
        </div>
      </CardContent>
    </Card>
  );
}
