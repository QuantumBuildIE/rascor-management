using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rascor.Modules.ToolboxTalks.Application.Abstractions.Translations;
using Rascor.Modules.ToolboxTalks.Application.Common.Interfaces;
using Rascor.Modules.ToolboxTalks.Application.Services.Subtitles;
using Rascor.Modules.ToolboxTalks.Domain.Entities;

namespace Rascor.Modules.ToolboxTalks.Application.Commands.GenerateContentTranslations;

/// <summary>
/// Handler for generating content translations for toolbox talk sections and questions.
/// </summary>
public class GenerateContentTranslationsCommandHandler
    : IRequestHandler<GenerateContentTranslationsCommand, GenerateContentTranslationsResult>
{
    private readonly IToolboxTalksDbContext _context;
    private readonly IContentTranslationService _translationService;
    private readonly ILanguageCodeService _languageCodeService;
    private readonly ILogger<GenerateContentTranslationsCommandHandler> _logger;

    public GenerateContentTranslationsCommandHandler(
        IToolboxTalksDbContext context,
        IContentTranslationService translationService,
        ILanguageCodeService languageCodeService,
        ILogger<GenerateContentTranslationsCommandHandler> logger)
    {
        _context = context;
        _translationService = translationService;
        _languageCodeService = languageCodeService;
        _logger = logger;
    }

    public async Task<GenerateContentTranslationsResult> Handle(
        GenerateContentTranslationsCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GenerateContentTranslationsCommandHandler started. " +
            "ToolboxTalkId: {ToolboxTalkId}, TenantId: {TenantId}, Languages: {Languages}",
            request.ToolboxTalkId, request.TenantId, string.Join(", ", request.TargetLanguages));

        // NOTE: IgnoreQueryFilters() because this handler runs in a Hangfire background job context
        // where the DbContext TenantId may not be set. We filter by TenantId explicitly.
        var toolboxTalk = await _context.ToolboxTalks
            .IgnoreQueryFilters()
            .Include(t => t.Sections.Where(s => !s.IsDeleted).OrderBy(s => s.SectionNumber))
            .Include(t => t.Questions.Where(q => !q.IsDeleted).OrderBy(q => q.QuestionNumber))
            .Include(t => t.Translations.Where(tr => !tr.IsDeleted))
            .Include(t => t.Slides.Where(s => !s.IsDeleted).OrderBy(s => s.PageNumber))
                .ThenInclude(s => s.Translations)
            .FirstOrDefaultAsync(t => t.Id == request.ToolboxTalkId
                && t.TenantId == request.TenantId
                && !t.IsDeleted, cancellationToken);

        if (toolboxTalk == null)
        {
            _logger.LogWarning(
                "ToolboxTalk {ToolboxTalkId} not found for TenantId {TenantId}",
                request.ToolboxTalkId, request.TenantId);
            return GenerateContentTranslationsResult.FailureResult("Toolbox talk not found");
        }

        // Get the source language name from the code
        var sourceLanguageName = _languageCodeService.GetLanguageName(toolboxTalk.SourceLanguageCode);

        _logger.LogInformation(
            "Loaded ToolboxTalk '{Title}'. SourceLanguage: {SourceLanguage} ({SourceCode}), " +
            "Sections: {SectionCount}, Questions: {QuestionCount}, " +
            "Existing translations: {TranslationCount}, Slides with text: {SlideCount}",
            toolboxTalk.Title, sourceLanguageName, toolboxTalk.SourceLanguageCode,
            toolboxTalk.Sections.Count, toolboxTalk.Questions.Count,
            toolboxTalk.Translations.Count,
            toolboxTalk.Slides.Count(s => !string.IsNullOrEmpty(s.OriginalText)));

        var results = new List<LanguageTranslationResult>();

        foreach (var language in request.TargetLanguages)
        {
            // Skip translation if target language is the same as source language
            var targetCode = _languageCodeService.GetLanguageCode(language);
            if (string.Equals(targetCode, toolboxTalk.SourceLanguageCode, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "Skipping translation to {Language} ({Code}) - same as source language",
                    language, targetCode);
                continue;
            }

            _logger.LogInformation("Starting translation for language: {Language} (from {Source})", language, sourceLanguageName);
            var result = await TranslateForLanguageAsync(toolboxTalk, language, sourceLanguageName, cancellationToken);
            _logger.LogInformation(
                "Translation result for {Language}: Success={Success}, " +
                "SectionsTranslated={Sections}, QuestionsTranslated={Questions}, " +
                "SlidesTranslated={Slides}, Error={Error}",
                language, result.Success, result.SectionsTranslated, result.QuestionsTranslated,
                result.SlidesTranslated, result.ErrorMessage ?? "none");
            results.Add(result);
        }

        _logger.LogInformation(
            "All translations done. Saving translation entities to database...");

        try
        {
            var rowsAffected = await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "SaveChangesAsync completed. Rows affected: {RowsAffected}. " +
                "Content translation completed for ToolboxTalk {ToolboxTalkId}. Success: {SuccessCount}/{TotalCount}",
                rowsAffected,
                request.ToolboxTalkId,
                results.Count(r => r.Success),
                results.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SaveChangesAsync FAILED for translations. " +
                "ToolboxTalkId: {ToolboxTalkId}, TenantId: {TenantId}, Error: {Error}",
                request.ToolboxTalkId, request.TenantId, ex.Message);
            throw;
        }

        return GenerateContentTranslationsResult.SuccessResult(results);
    }

    private async Task<LanguageTranslationResult> TranslateForLanguageAsync(
        ToolboxTalk toolboxTalk,
        string language,
        string sourceLanguage,
        CancellationToken cancellationToken)
    {
        var languageCode = _languageCodeService.GetLanguageCode(language);

        _logger.LogInformation(
            "Translating ToolboxTalk {ToolboxTalkId} to {Language} ({LanguageCode})",
            toolboxTalk.Id, language, languageCode);

        try
        {
            // Find existing translation or create new one
            var translation = toolboxTalk.Translations
                .FirstOrDefault(t => t.LanguageCode == languageCode);

            var isNew = translation == null;
            translation ??= new ToolboxTalkTranslation
            {
                Id = Guid.NewGuid(),
                TenantId = toolboxTalk.TenantId,
                ToolboxTalkId = toolboxTalk.Id,
                LanguageCode = languageCode
            };

            // Translate title (required - skip this language entirely if title fails)
            var titleResult = await _translationService.TranslateTextAsync(
                toolboxTalk.Title, language, false, cancellationToken, sourceLanguage);

            if (!titleResult.Success)
            {
                _logger.LogWarning(
                    "Title translation failed for ToolboxTalk {ToolboxTalkId} to {Language}: {Error}. " +
                    "Skipping this language entirely.",
                    toolboxTalk.Id, language, titleResult.ErrorMessage);

                // Don't add an entity with empty data - skip this language
                return new LanguageTranslationResult
                {
                    Language = language,
                    LanguageCode = languageCode,
                    Success = false,
                    ErrorMessage = $"Failed to translate title: {titleResult.ErrorMessage}"
                };
            }

            translation.TranslatedTitle = titleResult.TranslatedContent;

            // Translate description if present
            if (!string.IsNullOrWhiteSpace(toolboxTalk.Description))
            {
                var descResult = await _translationService.TranslateTextAsync(
                    toolboxTalk.Description, language, false, cancellationToken, sourceLanguage);

                translation.TranslatedDescription = descResult.Success
                    ? descResult.TranslatedContent
                    : null;

                if (!descResult.Success)
                {
                    _logger.LogWarning(
                        "Description translation failed for ToolboxTalk {ToolboxTalkId} to {Language}: {Error}",
                        toolboxTalk.Id, language, descResult.ErrorMessage);
                }
            }

            // Translate sections
            var sectionsTranslated = 0;
            var translatedSections = new List<TranslatedSectionData>();

            foreach (var section in toolboxTalk.Sections)
            {
                var sectionTitleResult = await _translationService.TranslateTextAsync(
                    section.Title, language, false, cancellationToken, sourceLanguage);

                var sectionContentResult = await _translationService.TranslateTextAsync(
                    section.Content, language, true, cancellationToken, sourceLanguage);

                if (sectionTitleResult.Success && sectionContentResult.Success)
                {
                    translatedSections.Add(new TranslatedSectionData
                    {
                        SectionId = section.Id,
                        Title = sectionTitleResult.TranslatedContent,
                        Content = sectionContentResult.TranslatedContent
                    });
                    sectionsTranslated++;
                }
                else
                {
                    _logger.LogWarning(
                        "Section {SectionId} translation failed for {Language}. Title: {TitleSuccess}, Content: {ContentSuccess}",
                        section.Id, language, sectionTitleResult.Success, sectionContentResult.Success);
                }
            }

            translation.TranslatedSections = JsonSerializer.Serialize(translatedSections);

            // Translate questions - save partial results even if some questions fail
            var questionsTranslated = 0;
            var questionsSkipped = 0;
            var translatedQuestions = new List<TranslatedQuestionData>();

            foreach (var question in toolboxTalk.Questions)
            {
                var questionTextResult = await _translationService.TranslateTextAsync(
                    question.QuestionText, language, false, cancellationToken, sourceLanguage);

                if (!questionTextResult.Success)
                {
                    _logger.LogWarning(
                        "Question {QuestionId} text translation failed for {Language}: {Error}. " +
                        "Skipping question, will fall back to English.",
                        question.Id, language, questionTextResult.ErrorMessage);
                    questionsSkipped++;
                    continue;
                }

                List<string>? translatedOptions = null;
                var allOptionsTranslated = true;

                if (!string.IsNullOrWhiteSpace(question.Options))
                {
                    try
                    {
                        var options = JsonSerializer.Deserialize<List<string>>(question.Options);
                        if (options != null && options.Count > 0)
                        {
                            translatedOptions = new List<string>();
                            foreach (var option in options)
                            {
                                var optionResult = await _translationService.TranslateTextAsync(
                                    option, language, false, cancellationToken, sourceLanguage);
                                if (optionResult.Success)
                                {
                                    translatedOptions.Add(optionResult.TranslatedContent);
                                }
                                else
                                {
                                    _logger.LogWarning(
                                        "Question {QuestionId} option translation failed for {Language}. " +
                                        "Skipping question, will fall back to English.",
                                        question.Id, language);
                                    allOptionsTranslated = false;
                                    break;
                                }
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        _logger.LogWarning("Failed to parse options for question {QuestionId}", question.Id);
                        allOptionsTranslated = false;
                    }
                }

                if (allOptionsTranslated)
                {
                    translatedQuestions.Add(new TranslatedQuestionData
                    {
                        QuestionId = question.Id,
                        QuestionText = questionTextResult.TranslatedContent,
                        Options = translatedOptions
                    });
                    questionsTranslated++;
                }
                else
                {
                    questionsSkipped++;
                }
            }

            translation.TranslatedQuestions = JsonSerializer.Serialize(translatedQuestions);

            // Update metadata
            translation.TranslatedAt = DateTime.UtcNow;
            translation.TranslationProvider = "Claude";

            // Generate translated email templates
            var emailSubjectResult = await _translationService.TranslateTextAsync(
                $"Action Required: Complete Toolbox Talk - {toolboxTalk.Title}",
                language, false, cancellationToken, sourceLanguage);
            translation.EmailSubject = emailSubjectResult.Success
                ? emailSubjectResult.TranslatedContent
                : $"Action Required: Complete Toolbox Talk - {translation.TranslatedTitle}";

            var emailBodyResult = await _translationService.TranslateTextAsync(
                $"You have been assigned a new toolbox talk: {toolboxTalk.Title}. Please complete it by the due date.",
                language, false, cancellationToken, sourceLanguage);
            translation.EmailBody = emailBodyResult.Success
                ? emailBodyResult.TranslatedContent
                : $"You have been assigned a new toolbox talk: {translation.TranslatedTitle}. Please complete it by the due date.";

            // Translate slide text
            var slidesTranslated = 0;
            var slidesWithText = toolboxTalk.Slides
                .Where(s => !string.IsNullOrEmpty(s.OriginalText))
                .ToList();

            foreach (var slide in slidesWithText)
            {
                // Skip if already translated to this language
                if (slide.Translations.Any(t => t.LanguageCode == languageCode))
                {
                    slidesTranslated++;
                    continue;
                }

                var slideTextResult = await _translationService.TranslateTextAsync(
                    slide.OriginalText!, language, false, cancellationToken, sourceLanguage);

                if (slideTextResult.Success && !string.IsNullOrEmpty(slideTextResult.TranslatedContent))
                {
                    var slideTranslation = new ToolboxTalkSlideTranslation
                    {
                        Id = Guid.NewGuid(),
                        SlideId = slide.Id,
                        LanguageCode = languageCode,
                        TranslatedText = slideTextResult.TranslatedContent
                    };
                    _context.ToolboxTalkSlideTranslations.Add(slideTranslation);
                    slidesTranslated++;
                }
                else
                {
                    _logger.LogWarning(
                        "Slide {SlideId} (page {PageNumber}) translation failed for {Language}: {Error}",
                        slide.Id, slide.PageNumber, language, slideTextResult.ErrorMessage);
                }
            }

            // Explicitly add to DbSet so the change tracker registers it as Added.
            // Using navigation collection Add alone is unreliable when the parent entity
            // was already tracked by a previous operation (content generation) on the same
            // scoped DbContext - DetectChanges() may not pick up the new entity.
            if (isNew)
            {
                _context.ToolboxTalkTranslations.Add(translation);
                _logger.LogInformation(
                    "Added new translation entity for {Language} ({LanguageCode}) via DbSet.Add(). " +
                    "Sections: {Sections}, Questions: {Questions} (skipped: {Skipped}), Slides: {Slides}",
                    language, languageCode, sectionsTranslated, questionsTranslated, questionsSkipped, slidesTranslated);
            }
            else
            {
                _logger.LogInformation(
                    "Updated existing translation entity for {Language} ({LanguageCode}). " +
                    "Sections: {Sections}, Questions: {Questions} (skipped: {Skipped}), Slides: {Slides}",
                    language, languageCode, sectionsTranslated, questionsTranslated, questionsSkipped, slidesTranslated);
            }

            return new LanguageTranslationResult
            {
                Language = language,
                LanguageCode = languageCode,
                Success = true,
                SectionsTranslated = sectionsTranslated,
                QuestionsTranslated = questionsTranslated,
                SlidesTranslated = slidesTranslated
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to translate ToolboxTalk {ToolboxTalkId} to {Language}",
                toolboxTalk.Id, language);

            return new LanguageTranslationResult
            {
                Language = language,
                LanguageCode = languageCode,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    // Internal DTOs for JSON serialization (matching existing structure)
    private class TranslatedSectionData
    {
        public Guid SectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    private class TranslatedQuestionData
    {
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public List<string>? Options { get; set; }
    }
}
