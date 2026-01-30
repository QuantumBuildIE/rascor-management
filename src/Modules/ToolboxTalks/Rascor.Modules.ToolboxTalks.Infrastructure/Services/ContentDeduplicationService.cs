using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rascor.Core.Application.Interfaces;
using Rascor.Modules.ToolboxTalks.Application.Common.Interfaces;
using Rascor.Modules.ToolboxTalks.Application.Services;
using Rascor.Modules.ToolboxTalks.Domain.Entities;
using Rascor.Modules.ToolboxTalks.Domain.Enums;

namespace Rascor.Modules.ToolboxTalks.Infrastructure.Services;

/// <summary>
/// Implementation of the content deduplication service.
/// Handles file hashing, duplicate detection, and content reuse.
/// </summary>
public class ContentDeduplicationService : IContentDeduplicationService
{
    private readonly IToolboxTalksDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ContentDeduplicationService> _logger;

    public ContentDeduplicationService(
        IToolboxTalksDbContext dbContext,
        ICurrentUserService currentUser,
        IHttpClientFactory httpClientFactory,
        ILogger<ContentDeduplicationService> logger)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public string CalculateFileHash(Stream fileStream)
    {
        using var sha256 = SHA256.Create();
        fileStream.Position = 0;
        var hashBytes = sha256.ComputeHash(fileStream);
        fileStream.Position = 0; // Reset for subsequent operations
        return Convert.ToHexString(hashBytes);
    }

    /// <inheritdoc />
    public async Task<string> CalculateFileHashFromUrlAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Calculating file hash from URL: {FileUrl}", fileUrl);

        var client = _httpClientFactory.CreateClient();

        // Use streaming to avoid loading entire file into memory
        using var response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var sha256 = SHA256.Create();
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
        var hash = Convert.ToHexString(hashBytes);

        _logger.LogDebug("Calculated file hash: {Hash}", hash);
        return hash;
    }

    /// <inheritdoc />
    public async Task<DuplicateCheckResult> CheckForDuplicateAsync(
        Guid tenantId,
        string fileHash,
        FileHashType fileType,
        Guid excludeToolboxTalkId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Checking for duplicate content. TenantId: {TenantId}, FileType: {FileType}, Hash: {Hash}, ExcludeId: {ExcludeId}",
            tenantId, fileType, fileHash, excludeToolboxTalkId);

        // Build the query based on file type
        var query = _dbContext.ToolboxTalks
            .Include(t => t.Sections.Where(s => !s.IsDeleted))
            .Include(t => t.Questions.Where(q => !q.IsDeleted))
            .Include(t => t.Translations.Where(tr => !tr.IsDeleted))
            .Where(t => t.TenantId == tenantId && t.Id != excludeToolboxTalkId && !t.IsDeleted);

        if (fileType == FileHashType.Pdf)
        {
            query = query.Where(t => t.PdfFileHash == fileHash);
        }
        else
        {
            query = query.Where(t => t.VideoFileHash == fileHash);
        }

        var existingTalk = await query.FirstOrDefaultAsync(cancellationToken);

        if (existingTalk == null)
        {
            _logger.LogInformation("No duplicate found for hash: {Hash}", fileHash);
            return new DuplicateCheckResult
            {
                IsDuplicate = false,
                SourceToolboxTalk = null
            };
        }

        _logger.LogInformation(
            "Duplicate found! SourceId: {SourceId}, Title: {Title}, Sections: {Sections}, Questions: {Questions}",
            existingTalk.Id, existingTalk.Title, existingTalk.Sections.Count, existingTalk.Questions.Count);

        // Extract translation languages
        var translationLanguages = existingTalk.Translations
            .Select(t => GetLanguageName(t.LanguageCode))
            .Distinct()
            .ToList();

        return new DuplicateCheckResult
        {
            IsDuplicate = true,
            SourceToolboxTalk = new SourceToolboxTalkInfo
            {
                Id = existingTalk.Id,
                Title = existingTalk.Title,
                ProcessedAt = existingTalk.ContentGeneratedAt,
                SectionCount = existingTalk.Sections.Count,
                QuestionCount = existingTalk.Questions.Count,
                TranslationLanguages = translationLanguages
            }
        };
    }

    /// <inheritdoc />
    public async Task<ContentReuseResult> ReuseContentAsync(
        Guid targetToolboxTalkId,
        Guid sourceToolboxTalkId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting content reuse. TargetId: {TargetId}, SourceId: {SourceId}, TenantId: {TenantId}",
            targetToolboxTalkId, sourceToolboxTalkId, tenantId);

        try
        {
            // Load source toolbox talk with all content
            var source = await _dbContext.ToolboxTalks
                .Include(t => t.Sections.Where(s => !s.IsDeleted))
                .Include(t => t.Questions.Where(q => !q.IsDeleted))
                .Include(t => t.Translations.Where(tr => !tr.IsDeleted))
                .FirstOrDefaultAsync(t => t.Id == sourceToolboxTalkId && t.TenantId == tenantId && !t.IsDeleted, cancellationToken);

            if (source == null)
            {
                _logger.LogWarning("Source toolbox talk {SourceId} not found", sourceToolboxTalkId);
                return new ContentReuseResult
                {
                    Success = false,
                    ErrorMessage = "Source toolbox talk not found"
                };
            }

            // Load target toolbox talk
            var target = await _dbContext.ToolboxTalks
                .Include(t => t.Sections.Where(s => !s.IsDeleted))
                .Include(t => t.Questions.Where(q => !q.IsDeleted))
                .Include(t => t.Translations.Where(tr => !tr.IsDeleted))
                .FirstOrDefaultAsync(t => t.Id == targetToolboxTalkId && t.TenantId == tenantId && !t.IsDeleted, cancellationToken);

            if (target == null)
            {
                _logger.LogWarning("Target toolbox talk {TargetId} not found", targetToolboxTalkId);
                return new ContentReuseResult
                {
                    Success = false,
                    ErrorMessage = "Target toolbox talk not found"
                };
            }

            var currentTime = DateTime.UtcNow;
            var currentUser = _currentUser.UserId ?? "System";

            // Soft delete existing content on target
            foreach (var section in target.Sections.ToList())
            {
                section.IsDeleted = true;
                section.UpdatedAt = currentTime;
                section.UpdatedBy = currentUser;
            }

            foreach (var question in target.Questions.ToList())
            {
                question.IsDeleted = true;
                question.UpdatedAt = currentTime;
                question.UpdatedBy = currentUser;
            }

            foreach (var translation in target.Translations.ToList())
            {
                translation.IsDeleted = true;
                translation.UpdatedAt = currentTime;
                translation.UpdatedBy = currentUser;
            }

            var sectionsCopied = 0;
            var questionsCopied = 0;
            var translationsCopied = 0;

            // Copy sections
            foreach (var sourceSection in source.Sections.OrderBy(s => s.SectionNumber))
            {
                var newSection = new ToolboxTalkSection
                {
                    Id = Guid.NewGuid(),
                    ToolboxTalkId = targetToolboxTalkId,
                    SectionNumber = sourceSection.SectionNumber,
                    Title = sourceSection.Title,
                    Content = sourceSection.Content,
                    RequiresAcknowledgment = sourceSection.RequiresAcknowledgment,
                    Source = sourceSection.Source,
                    VideoTimestamp = sourceSection.VideoTimestamp,
                    CreatedAt = currentTime,
                    CreatedBy = currentUser
                };

                _dbContext.ToolboxTalkSections.Add(newSection);
                sectionsCopied++;
            }

            // Copy questions
            foreach (var sourceQuestion in source.Questions.OrderBy(q => q.QuestionNumber))
            {
                var newQuestion = new ToolboxTalkQuestion
                {
                    Id = Guid.NewGuid(),
                    ToolboxTalkId = targetToolboxTalkId,
                    QuestionNumber = sourceQuestion.QuestionNumber,
                    QuestionText = sourceQuestion.QuestionText,
                    QuestionType = sourceQuestion.QuestionType,
                    Options = sourceQuestion.Options,
                    CorrectAnswer = sourceQuestion.CorrectAnswer,
                    Points = sourceQuestion.Points,
                    Source = sourceQuestion.Source,
                    IsFromVideoFinalPortion = sourceQuestion.IsFromVideoFinalPortion,
                    VideoTimestamp = sourceQuestion.VideoTimestamp,
                    CreatedAt = currentTime,
                    CreatedBy = currentUser
                };

                _dbContext.ToolboxTalkQuestions.Add(newQuestion);
                questionsCopied++;
            }

            // Copy translations
            foreach (var sourceTranslation in source.Translations)
            {
                var newTranslation = new ToolboxTalkTranslation
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ToolboxTalkId = targetToolboxTalkId,
                    LanguageCode = sourceTranslation.LanguageCode,
                    TranslatedTitle = sourceTranslation.TranslatedTitle,
                    TranslatedDescription = sourceTranslation.TranslatedDescription,
                    TranslatedSections = sourceTranslation.TranslatedSections,
                    TranslatedQuestions = sourceTranslation.TranslatedQuestions,
                    EmailSubject = sourceTranslation.EmailSubject,
                    EmailBody = sourceTranslation.EmailBody,
                    TranslatedAt = currentTime,
                    TranslationProvider = "ContentReuse",
                    CreatedAt = currentTime,
                    CreatedBy = currentUser
                };

                _dbContext.ToolboxTalkTranslations.Add(newTranslation);
                translationsCopied++;
            }

            // Copy file hash and URL (reuse the same blob)
            if (!string.IsNullOrEmpty(source.PdfFileHash))
            {
                target.PdfFileHash = source.PdfFileHash;
                target.PdfUrl = source.PdfUrl;
                target.PdfFileName = source.PdfFileName;
                target.ExtractedPdfText = source.ExtractedPdfText;
                target.PdfTextExtractedAt = source.PdfTextExtractedAt;
                target.GeneratedFromPdf = source.GeneratedFromPdf;
            }

            if (!string.IsNullOrEmpty(source.VideoFileHash))
            {
                target.VideoFileHash = source.VideoFileHash;
                target.VideoUrl = source.VideoUrl;
                target.VideoSource = source.VideoSource;
                target.ExtractedVideoTranscript = source.ExtractedVideoTranscript;
                target.VideoTranscriptExtractedAt = source.VideoTranscriptExtractedAt;
                target.GeneratedFromVideo = source.GeneratedFromVideo;
            }

            // Update target metadata
            target.ContentGeneratedAt = currentTime;
            target.RequiresQuiz = questionsCopied > 0;
            target.Status = ToolboxTalkStatus.ReadyForReview;
            target.UpdatedAt = currentTime;
            target.UpdatedBy = currentUser;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Content reuse complete. Sections: {Sections}, Questions: {Questions}, Translations: {Translations}",
                sectionsCopied, questionsCopied, translationsCopied);

            return new ContentReuseResult
            {
                Success = true,
                SectionsCopied = sectionsCopied,
                QuestionsCopied = questionsCopied,
                TranslationsCopied = translationsCopied
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during content reuse. TargetId: {TargetId}, SourceId: {SourceId}",
                targetToolboxTalkId, sourceToolboxTalkId);

            return new ContentReuseResult
            {
                Success = false,
                ErrorMessage = $"Failed to reuse content: {ex.Message}"
            };
        }
    }

    /// <inheritdoc />
    public async Task UpdateFileHashAsync(
        Guid toolboxTalkId,
        string fileHash,
        FileHashType fileType,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Updating file hash. ToolboxTalkId: {Id}, FileType: {Type}, Hash: {Hash}",
            toolboxTalkId, fileType, fileHash);

        var toolboxTalk = await _dbContext.ToolboxTalks
            .FirstOrDefaultAsync(t => t.Id == toolboxTalkId && t.TenantId == tenantId && !t.IsDeleted, cancellationToken);

        if (toolboxTalk == null)
        {
            _logger.LogWarning("Toolbox talk {Id} not found for hash update", toolboxTalkId);
            return;
        }

        if (fileType == FileHashType.Pdf)
        {
            toolboxTalk.PdfFileHash = fileHash;
        }
        else
        {
            toolboxTalk.VideoFileHash = fileHash;
        }

        toolboxTalk.UpdatedAt = DateTime.UtcNow;
        toolboxTalk.UpdatedBy = _currentUser.UserId ?? "System";

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the display name for a language code
    /// </summary>
    private static string GetLanguageName(string languageCode)
    {
        return languageCode.ToLowerInvariant() switch
        {
            "pl" => "Polish",
            "ro" => "Romanian",
            "pt" => "Portuguese",
            "es" => "Spanish",
            "fr" => "French",
            "de" => "German",
            "it" => "Italian",
            "ru" => "Russian",
            "uk" => "Ukrainian",
            "lt" => "Lithuanian",
            "lv" => "Latvian",
            "bg" => "Bulgarian",
            "hu" => "Hungarian",
            "cs" => "Czech",
            "sk" => "Slovak",
            "hr" => "Croatian",
            _ => languageCode.ToUpperInvariant()
        };
    }
}
