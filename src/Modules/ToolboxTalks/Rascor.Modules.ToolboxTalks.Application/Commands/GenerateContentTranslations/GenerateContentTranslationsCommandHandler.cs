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
            "Generating content translations for ToolboxTalk {ToolboxTalkId} in {LanguageCount} languages",
            request.ToolboxTalkId, request.TargetLanguages.Count);

        // Load the toolbox talk with sections and questions
        var toolboxTalk = await _context.ToolboxTalks
            .Include(t => t.Sections.OrderBy(s => s.SectionNumber))
            .Include(t => t.Questions.OrderBy(q => q.QuestionNumber))
            .Include(t => t.Translations)
            .FirstOrDefaultAsync(t => t.Id == request.ToolboxTalkId && t.TenantId == request.TenantId, cancellationToken);

        if (toolboxTalk == null)
        {
            return GenerateContentTranslationsResult.FailureResult("Toolbox talk not found");
        }

        var results = new List<LanguageTranslationResult>();

        foreach (var language in request.TargetLanguages)
        {
            var result = await TranslateForLanguageAsync(toolboxTalk, language, cancellationToken);
            results.Add(result);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
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

                translation.TranslatedDescription = descResult.Success
                    ? descResult.TranslatedContent
                    : toolboxTalk.Description; // Fallback to original
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

                translatedSections.Add(new TranslatedSectionData
                {
                    SectionId = section.Id,
                    Title = sectionTitleResult.Success ? sectionTitleResult.TranslatedContent : section.Title,
                    Content = sectionContentResult.Success ? sectionContentResult.TranslatedContent : section.Content
                });

                if (sectionTitleResult.Success && sectionContentResult.Success)
                {
                    sectionsTranslated++;
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

                List<string>? translatedOptions = null;

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
                                translatedOptions.Add(optionResult.Success ? optionResult.TranslatedContent : option);
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // Keep original options if parsing fails
                    }
                }

                translatedQuestions.Add(new TranslatedQuestionData
                {
                    QuestionId = question.Id,
                    QuestionText = questionTextResult.Success ? questionTextResult.TranslatedContent : question.QuestionText,
                    Options = translatedOptions
                });

                if (questionTextResult.Success)
                {
                    questionsTranslated++;
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
