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
            "[DEBUG] GenerateContentTranslationsCommandHandler started. " +
            "ToolboxTalkId: {ToolboxTalkId}, TenantId: {TenantId}, Languages: {Languages}",
            request.ToolboxTalkId, request.TenantId, string.Join(", ", request.TargetLanguages));

        // Load the toolbox talk with sections and questions
        var toolboxTalk = await _context.ToolboxTalks
            .Include(t => t.Sections.OrderBy(s => s.SectionNumber))
            .Include(t => t.Questions.OrderBy(q => q.QuestionNumber))
            .Include(t => t.Translations)
            .FirstOrDefaultAsync(t => t.Id == request.ToolboxTalkId && t.TenantId == request.TenantId, cancellationToken);

        if (toolboxTalk == null)
        {
            _logger.LogWarning(
                "[DEBUG] ToolboxTalk {ToolboxTalkId} not found for TenantId {TenantId}",
                request.ToolboxTalkId, request.TenantId);
            return GenerateContentTranslationsResult.FailureResult("Toolbox talk not found");
        }

        _logger.LogInformation(
            "[DEBUG] Loaded ToolboxTalk. Title: {Title}, Sections: {SectionCount}, Questions: {QuestionCount}, " +
            "Existing translations: {TranslationCount}",
            toolboxTalk.Title, toolboxTalk.Sections.Count, toolboxTalk.Questions.Count, toolboxTalk.Translations.Count);

        _logger.LogInformation(
            "[DEBUG] Section IDs: {Ids}",
            string.Join(", ", toolboxTalk.Sections.Select(s => $"{s.Id} (#{s.SectionNumber}: {s.Title?.Substring(0, Math.Min(30, s.Title?.Length ?? 0))})")));

        _logger.LogInformation(
            "[DEBUG] Question IDs: {Ids}",
            string.Join(", ", toolboxTalk.Questions.Select(q => $"{q.Id} (#{q.QuestionNumber})")));

        if (toolboxTalk.Translations.Count > 0)
        {
            _logger.LogInformation(
                "[DEBUG] Existing translation languages: {Languages}",
                string.Join(", ", toolboxTalk.Translations.Select(t => $"{t.LanguageCode} (Id: {t.Id})")));
        }

        var results = new List<LanguageTranslationResult>();

        foreach (var language in request.TargetLanguages)
        {
            _logger.LogInformation("[DEBUG] Starting translation for language: {Language}", language);
            var result = await TranslateForLanguageAsync(toolboxTalk, language, cancellationToken);
            _logger.LogInformation(
                "[DEBUG] Translation result for {Language}: Success={Success}, " +
                "SectionsTranslated={Sections}, QuestionsTranslated={Questions}, Error={Error}",
                language, result.Success, result.SectionsTranslated, result.QuestionsTranslated,
                result.ErrorMessage ?? "none");
            results.Add(result);
        }

        _logger.LogInformation(
            "[DEBUG] All translations done. Saving {TranslationCount} translation entities to database...",
            toolboxTalk.Translations.Count);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "[DEBUG] SaveChangesAsync completed for translations. " +
            "Content translation completed for ToolboxTalk {ToolboxTalkId}. Success: {SuccessCount}/{TotalCount}",
            request.ToolboxTalkId,
            results.Count(r => r.Success),
            results.Count);

        return GenerateContentTranslationsResult.SuccessResult(results);
    }

    private async Task<LanguageTranslationResult> TranslateForLanguageAsync(
        ToolboxTalk toolboxTalk,
        string language,
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

            if (translation == null)
            {
                translation = new ToolboxTalkTranslation
                {
                    Id = Guid.NewGuid(),
                    TenantId = toolboxTalk.TenantId,
                    ToolboxTalkId = toolboxTalk.Id,
                    LanguageCode = languageCode
                };
                toolboxTalk.Translations.Add(translation);
            }

            // Translate title
            var titleResult = await _translationService.TranslateTextAsync(
                toolboxTalk.Title, language, false, cancellationToken);

            if (!titleResult.Success)
            {
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
                    toolboxTalk.Description, language, false, cancellationToken);

                // Only set translated description if translation succeeded
                // Null description will cause retrieval to fall back to original English
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
                    section.Title, language, false, cancellationToken);

                var sectionContentResult = await _translationService.TranslateTextAsync(
                    section.Content, language, true, cancellationToken); // Content may contain HTML

                // Only add to translated sections if BOTH title and content translated successfully
                // This ensures retrieval code falls back to English for untranslated sections
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

            // Translate questions
            var questionsTranslated = 0;
            var translatedQuestions = new List<TranslatedQuestionData>();

            foreach (var question in toolboxTalk.Questions)
            {
                var questionTextResult = await _translationService.TranslateTextAsync(
                    question.QuestionText, language, false, cancellationToken);

                if (!questionTextResult.Success)
                {
                    _logger.LogWarning(
                        "Question {QuestionId} text translation failed for {Language}: {Error}",
                        question.Id, language, questionTextResult.ErrorMessage);
                    continue; // Skip this question, retrieval will fall back to English
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
                                    option, language, false, cancellationToken);
                                if (optionResult.Success)
                                {
                                    translatedOptions.Add(optionResult.TranslatedContent);
                                }
                                else
                                {
                                    _logger.LogWarning(
                                        "Question {QuestionId} option translation failed for {Language}",
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

                // Only add if question text and all options translated successfully
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
                    _logger.LogWarning(
                        "Question {QuestionId} skipped due to option translation failures for {Language}",
                        question.Id, language);
                }
            }

            translation.TranslatedQuestions = JsonSerializer.Serialize(translatedQuestions);

            // Update metadata
            translation.TranslatedAt = DateTime.UtcNow;
            translation.TranslationProvider = "Claude";

            // Generate translated email templates (basic)
            var emailSubjectResult = await _translationService.TranslateTextAsync(
                $"Action Required: Complete Toolbox Talk - {toolboxTalk.Title}",
                language, false, cancellationToken);
            translation.EmailSubject = emailSubjectResult.Success
                ? emailSubjectResult.TranslatedContent
                : $"Action Required: Complete Toolbox Talk - {translation.TranslatedTitle}";

            var emailBodyResult = await _translationService.TranslateTextAsync(
                $"You have been assigned a new toolbox talk: {toolboxTalk.Title}. Please complete it by the due date.",
                language, false, cancellationToken);
            translation.EmailBody = emailBodyResult.Success
                ? emailBodyResult.TranslatedContent
                : $"You have been assigned a new toolbox talk: {translation.TranslatedTitle}. Please complete it by the due date.";

            _logger.LogInformation(
                "Successfully translated ToolboxTalk {ToolboxTalkId} to {Language}: {Sections} sections, {Questions} questions",
                toolboxTalk.Id, language, sectionsTranslated, questionsTranslated);

            return new LanguageTranslationResult
            {
                Language = language,
                LanguageCode = languageCode,
                Success = true,
                SectionsTranslated = sectionsTranslated,
                QuestionsTranslated = questionsTranslated
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
