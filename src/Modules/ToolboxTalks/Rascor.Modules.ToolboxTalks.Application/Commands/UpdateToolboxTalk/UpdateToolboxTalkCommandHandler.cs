using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Rascor.Modules.ToolboxTalks.Application.Common.Interfaces;
using Rascor.Modules.ToolboxTalks.Application.DTOs;
using Rascor.Modules.ToolboxTalks.Domain.Entities;
using Rascor.Modules.ToolboxTalks.Domain.Enums;

namespace Rascor.Modules.ToolboxTalks.Application.Commands.UpdateToolboxTalk;

public class UpdateToolboxTalkCommandHandler : IRequestHandler<UpdateToolboxTalkCommand, ToolboxTalkDto>
{
    private readonly IToolboxTalksDbContext _dbContext;

    public UpdateToolboxTalkCommandHandler(IToolboxTalksDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ToolboxTalkDto> Handle(UpdateToolboxTalkCommand request, CancellationToken cancellationToken)
    {
        // Find the toolbox talk with sections and questions
        var toolboxTalk = await _dbContext.ToolboxTalks
            .Include(t => t.Sections)
            .Include(t => t.Questions)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (toolboxTalk == null)
        {
            throw new KeyNotFoundException($"Toolbox talk with ID {request.Id} not found.");
        }

        // Validate tenant ownership
        if (toolboxTalk.TenantId != request.TenantId)
        {
            throw new UnauthorizedAccessException("Access denied to this toolbox talk.");
        }

        // Validate title uniqueness (excluding current talk)
        if (toolboxTalk.Title != request.Title)
        {
            var titleExists = await _dbContext.ToolboxTalks
                .AnyAsync(t => t.TenantId == request.TenantId &&
                              t.Title == request.Title &&
                              t.Id != request.Id, cancellationToken);

            if (titleExists)
            {
                throw new InvalidOperationException($"A toolbox talk with title '{request.Title}' already exists.");
            }
        }

        // Update basic properties
        toolboxTalk.Title = request.Title;
        toolboxTalk.Description = request.Description;
        toolboxTalk.Frequency = request.Frequency;
        toolboxTalk.VideoUrl = request.VideoUrl;
        toolboxTalk.VideoSource = request.VideoSource;
        toolboxTalk.AttachmentUrl = request.AttachmentUrl;
        toolboxTalk.MinimumVideoWatchPercent = request.MinimumVideoWatchPercent;
        toolboxTalk.RequiresQuiz = request.RequiresQuiz;
        toolboxTalk.PassingScore = request.RequiresQuiz ? request.PassingScore : null;
        toolboxTalk.IsActive = request.IsActive;

        // Handle sections update
        UpdateSections(toolboxTalk, request.Sections);

        // Handle questions update
        UpdateQuestions(toolboxTalk, request.Questions);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Reload to ensure clean state
        await _dbContext.ToolboxTalks
            .Entry(toolboxTalk)
            .Collection(t => t.Sections)
            .LoadAsync(cancellationToken);
        await _dbContext.ToolboxTalks
            .Entry(toolboxTalk)
            .Collection(t => t.Questions)
            .LoadAsync(cancellationToken);

        return MapToDto(toolboxTalk);
    }

    private void UpdateSections(ToolboxTalk toolboxTalk, List<UpdateToolboxTalkSectionDto> sectionDtos)
    {
        var existingSections = toolboxTalk.Sections.Where(s => !s.IsDeleted).ToList();
        var incomingSectionIds = sectionDtos.Where(s => s.Id.HasValue).Select(s => s.Id!.Value).ToHashSet();

        // Soft delete sections not in the request
        foreach (var section in existingSections)
        {
            if (!incomingSectionIds.Contains(section.Id))
            {
                section.IsDeleted = true;
            }
        }

        // Update existing and add new sections
        foreach (var sectionDto in sectionDtos)
        {
            if (sectionDto.Id.HasValue)
            {
                // Update existing section
                var existingSection = existingSections.FirstOrDefault(s => s.Id == sectionDto.Id.Value);
                if (existingSection != null)
                {
                    existingSection.SectionNumber = sectionDto.SectionNumber;
                    existingSection.Title = sectionDto.Title;
                    existingSection.Content = sectionDto.Content;
                    existingSection.RequiresAcknowledgment = sectionDto.RequiresAcknowledgment;
                    existingSection.IsDeleted = false; // In case it was marked for deletion
                }
            }
            else
            {
                // Create new section
                var newSection = new ToolboxTalkSection
                {
                    Id = Guid.NewGuid(),
                    ToolboxTalkId = toolboxTalk.Id,
                    SectionNumber = sectionDto.SectionNumber,
                    Title = sectionDto.Title,
                    Content = sectionDto.Content,
                    RequiresAcknowledgment = sectionDto.RequiresAcknowledgment
                };
                toolboxTalk.Sections.Add(newSection);
            }
        }
    }

    private void UpdateQuestions(ToolboxTalk toolboxTalk, List<UpdateToolboxTalkQuestionDto> questionDtos)
    {
        var existingQuestions = toolboxTalk.Questions.Where(q => !q.IsDeleted).ToList();
        var incomingQuestionIds = questionDtos.Where(q => q.Id.HasValue).Select(q => q.Id!.Value).ToHashSet();

        // Soft delete questions not in the request
        foreach (var question in existingQuestions)
        {
            if (!incomingQuestionIds.Contains(question.Id))
            {
                question.IsDeleted = true;
            }
        }

        // Update existing and add new questions
        foreach (var questionDto in questionDtos)
        {
            if (questionDto.Id.HasValue)
            {
                // Update existing question
                var existingQuestion = existingQuestions.FirstOrDefault(q => q.Id == questionDto.Id.Value);
                if (existingQuestion != null)
                {
                    existingQuestion.QuestionNumber = questionDto.QuestionNumber;
                    existingQuestion.QuestionText = questionDto.QuestionText;
                    existingQuestion.QuestionType = questionDto.QuestionType;
                    existingQuestion.Options = questionDto.Options != null ? JsonSerializer.Serialize(questionDto.Options) : null;
                    existingQuestion.CorrectAnswer = questionDto.CorrectAnswer;
                    existingQuestion.Points = questionDto.Points;
                    existingQuestion.IsDeleted = false; // In case it was marked for deletion
                }
            }
            else
            {
                // Create new question
                var newQuestion = new ToolboxTalkQuestion
                {
                    Id = Guid.NewGuid(),
                    ToolboxTalkId = toolboxTalk.Id,
                    QuestionNumber = questionDto.QuestionNumber,
                    QuestionText = questionDto.QuestionText,
                    QuestionType = questionDto.QuestionType,
                    Options = questionDto.Options != null ? JsonSerializer.Serialize(questionDto.Options) : null,
                    CorrectAnswer = questionDto.CorrectAnswer,
                    Points = questionDto.Points
                };
                toolboxTalk.Questions.Add(newQuestion);
            }
        }
    }

    private static ToolboxTalkDto MapToDto(ToolboxTalk entity)
    {
        return new ToolboxTalkDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Frequency = entity.Frequency,
            FrequencyDisplay = GetFrequencyDisplay(entity.Frequency),
            VideoUrl = entity.VideoUrl,
            VideoSource = entity.VideoSource,
            VideoSourceDisplay = GetVideoSourceDisplay(entity.VideoSource),
            AttachmentUrl = entity.AttachmentUrl,
            MinimumVideoWatchPercent = entity.MinimumVideoWatchPercent,
            RequiresQuiz = entity.RequiresQuiz,
            PassingScore = entity.PassingScore,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Sections = entity.Sections
                .Where(s => !s.IsDeleted)
                .Select(s => new ToolboxTalkSectionDto
                {
                    Id = s.Id,
                    ToolboxTalkId = s.ToolboxTalkId,
                    SectionNumber = s.SectionNumber,
                    Title = s.Title,
                    Content = s.Content,
                    RequiresAcknowledgment = s.RequiresAcknowledgment
                }).OrderBy(s => s.SectionNumber).ToList(),
            Questions = entity.Questions
                .Where(q => !q.IsDeleted)
                .Select(q => new ToolboxTalkQuestionDto
                {
                    Id = q.Id,
                    ToolboxTalkId = q.ToolboxTalkId,
                    QuestionNumber = q.QuestionNumber,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    QuestionTypeDisplay = GetQuestionTypeDisplay(q.QuestionType),
                    Options = !string.IsNullOrEmpty(q.Options) ? JsonSerializer.Deserialize<List<string>>(q.Options) : null,
                    CorrectAnswer = q.CorrectAnswer,
                    Points = q.Points
                }).OrderBy(q => q.QuestionNumber).ToList()
        };
    }

    private static string GetFrequencyDisplay(ToolboxTalkFrequency frequency) => frequency switch
    {
        ToolboxTalkFrequency.Once => "Once",
        ToolboxTalkFrequency.Weekly => "Weekly",
        ToolboxTalkFrequency.Monthly => "Monthly",
        ToolboxTalkFrequency.Annually => "Annually",
        _ => frequency.ToString()
    };

    private static string GetVideoSourceDisplay(VideoSource source) => source switch
    {
        VideoSource.None => "None",
        VideoSource.YouTube => "YouTube",
        VideoSource.GoogleDrive => "Google Drive",
        VideoSource.Vimeo => "Vimeo",
        VideoSource.DirectUrl => "Direct URL",
        _ => source.ToString()
    };

    private static string GetQuestionTypeDisplay(QuestionType type) => type switch
    {
        QuestionType.MultipleChoice => "Multiple Choice",
        QuestionType.TrueFalse => "True/False",
        QuestionType.ShortAnswer => "Short Answer",
        _ => type.ToString()
    };
}
