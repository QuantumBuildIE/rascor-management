using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rascor.Modules.Rams.Application.Common.Interfaces;
using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Application.Services;
using Rascor.Modules.Rams.Domain.Entities;

namespace Rascor.Modules.Rams.Infrastructure.Services;

/// <summary>
/// Service implementation for AI-powered control measure suggestions
/// </summary>
public class RamsAiService : IRamsAiService
{
    private readonly IRamsDbContext _context;
    private readonly IRamsLibraryService _libraryService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RamsAiService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public RamsAiService(
        IRamsDbContext context,
        IRamsLibraryService libraryService,
        IConfiguration configuration,
        ILogger<RamsAiService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _libraryService = libraryService;
        _configuration = configuration;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ControlMeasureSuggestionResponseDto> GetControlMeasureSuggestionsAsync(
        ControlMeasureSuggestionRequestDto request,
        Guid? ramsDocumentId = null,
        Guid? riskAssessmentId = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var auditLog = new McpAuditLog
        {
            RamsDocumentId = ramsDocumentId,
            RiskAssessmentId = riskAssessmentId,
            RequestType = "ControlMeasureSuggestion",
            InputPrompt = $"Task: {request.TaskActivity}, Hazard: {request.HazardIdentified}",
            InputContext = JsonSerializer.Serialize(request),
            RequestedAt = DateTime.UtcNow
        };

        try
        {
            // Step 1: Search library for matching hazards
            var matchedHazards = await SearchHazardsAsync(request.TaskActivity, request.HazardIdentified, cancellationToken);

            // Step 2: Search library for matching controls
            var matchedControls = await SearchControlsAsync(request.TaskActivity, request.HazardIdentified, matchedHazards, cancellationToken);

            // Step 3: Search for relevant legislation
            var matchedLegislation = await SearchLegislationAsync(request.TaskActivity, request.HazardIdentified, cancellationToken);

            // Step 4: Search for relevant SOPs
            var matchedSops = await SearchSopsAsync(request.TaskActivity, cancellationToken);

            // Step 5: If library results are sparse, use AI to generate suggestions
            string? aiGeneratedControls = null;
            string? aiGeneratedLegislation = null;
            int? suggestedResidualL = null;
            int? suggestedResidualS = null;
            bool usedAi = false;

            var aiEnabled = _configuration.GetValue<bool>("Rams:AiEnabled", true);
            var shouldUseAi = aiEnabled && (matchedControls.Count < 3 || !string.IsNullOrEmpty(request.HazardIdentified));

            if (shouldUseAi)
            {
                var aiResult = await GenerateAiSuggestionsAsync(request, matchedHazards, matchedControls, matchedLegislation, matchedSops, cancellationToken);

                if (aiResult != null)
                {
                    aiGeneratedControls = aiResult.ControlMeasures;
                    aiGeneratedLegislation = aiResult.Legislation;
                    suggestedResidualL = aiResult.SuggestedResidualLikelihood;
                    suggestedResidualS = aiResult.SuggestedResidualSeverity;
                    usedAi = true;

                    auditLog.AiResponse = JsonSerializer.Serialize(aiResult);
                    auditLog.ExtractedContent = aiGeneratedControls;
                    auditLog.ModelUsed = "claude-3-sonnet";
                    auditLog.InputTokens = aiResult.InputTokens;
                    auditLog.OutputTokens = aiResult.OutputTokens;
                }
            }

            stopwatch.Stop();
            auditLog.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            auditLog.IsSuccess = true;

            await _context.RamsMcpAuditLogs.AddAsync(auditLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new ControlMeasureSuggestionResponseDto
            {
                Success = true,
                MatchedHazards = matchedHazards,
                SuggestedControls = matchedControls,
                RelevantLegislation = matchedLegislation,
                RelevantSops = matchedSops,
                AiGeneratedControlMeasures = aiGeneratedControls,
                AiGeneratedLegislation = aiGeneratedLegislation,
                SuggestedResidualLikelihood = suggestedResidualL,
                SuggestedResidualSeverity = suggestedResidualS,
                AuditLogId = auditLog.Id,
                UsedAi = usedAi
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get control measure suggestions");

            stopwatch.Stop();
            auditLog.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            auditLog.IsSuccess = false;
            auditLog.ErrorMessage = ex.Message;

            await _context.RamsMcpAuditLogs.AddAsync(auditLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new ControlMeasureSuggestionResponseDto
            {
                Success = false,
                ErrorMessage = "Failed to generate suggestions. Please try again.",
                AuditLogId = auditLog.Id
            };
        }
    }

    public async Task MarkSuggestionAcceptedAsync(Guid auditLogId, bool accepted, CancellationToken cancellationToken = default)
    {
        var auditLog = await _context.RamsMcpAuditLogs
            .FirstOrDefaultAsync(x => x.Id == auditLogId, cancellationToken);

        if (auditLog != null)
        {
            auditLog.WasAccepted = accepted;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<List<HazardMatchDto>> SearchHazardsAsync(string task, string hazard, CancellationToken cancellationToken)
    {
        var searchTerms = ExtractKeywords(task, hazard);
        var allHazards = await _libraryService.GetAllHazardsAsync(false, cancellationToken);

        var matches = allHazards
            .Select(h => new
            {
                Hazard = h,
                Score = CalculateMatchScore(searchTerms, h.Name, h.Keywords, h.Description)
            })
            .Where(x => x.Score > 0.1)
            .OrderByDescending(x => x.Score)
            .Take(5)
            .Select(x => new HazardMatchDto
            {
                Id = x.Hazard.Id,
                Code = x.Hazard.Code,
                Name = x.Hazard.Name,
                Description = x.Hazard.Description,
                Category = x.Hazard.Category.ToString(),
                DefaultLikelihood = x.Hazard.DefaultLikelihood,
                DefaultSeverity = x.Hazard.DefaultSeverity,
                MatchScore = x.Score
            })
            .ToList();

        return matches;
    }

    private async Task<List<ControlMatchDto>> SearchControlsAsync(
        string task,
        string hazard,
        List<HazardMatchDto> matchedHazards,
        CancellationToken cancellationToken)
    {
        var searchTerms = ExtractKeywords(task, hazard);
        var allControls = await _libraryService.GetAllControlsAsync(false, cancellationToken);

        // Also include category-specific controls from matched hazards
        var relevantCategories = matchedHazards
            .Select(h => h.Category)
            .Distinct()
            .ToList();

        var matches = allControls
            .Select(c => new
            {
                Control = c,
                Score = CalculateMatchScore(searchTerms, c.Name, c.Keywords, c.Description) +
                        (c.ApplicableToCategory != null && relevantCategories.Contains(c.ApplicableToCategory.Value.ToString()) ? 0.3 : 0)
            })
            .Where(x => x.Score > 0.1)
            .OrderByDescending(x => x.Control.Hierarchy) // Prioritize Elimination > Substitution > Engineering etc.
            .ThenByDescending(x => x.Score)
            .Take(10)
            .Select(x => new ControlMatchDto
            {
                Id = x.Control.Id,
                Code = x.Control.Code,
                Name = x.Control.Name,
                Description = x.Control.Description,
                Hierarchy = x.Control.Hierarchy.ToString(),
                LikelihoodReduction = x.Control.TypicalLikelihoodReduction,
                SeverityReduction = x.Control.TypicalSeverityReduction,
                MatchScore = x.Score
            })
            .ToList();

        return matches;
    }

    private async Task<List<LegislationMatchDto>> SearchLegislationAsync(string task, string hazard, CancellationToken cancellationToken)
    {
        var searchTerms = ExtractKeywords(task, hazard);
        var allLegislation = await _libraryService.GetAllLegislationAsync(false, cancellationToken);

        var matches = allLegislation
            .Select(l => new
            {
                Legislation = l,
                Score = CalculateMatchScore(searchTerms, l.Name, l.Keywords, l.Description, l.ApplicableCategories)
            })
            .Where(x => x.Score > 0.1)
            .OrderByDescending(x => x.Score)
            .Take(5)
            .Select(x => new LegislationMatchDto
            {
                Id = x.Legislation.Id,
                Code = x.Legislation.Code,
                Name = x.Legislation.Name,
                ShortName = x.Legislation.ShortName,
                MatchScore = x.Score
            })
            .ToList();

        return matches;
    }

    private async Task<List<SopMatchDto>> SearchSopsAsync(string task, CancellationToken cancellationToken)
    {
        var searchTerms = ExtractKeywords(task);
        var allSops = await _libraryService.GetAllSopsAsync(false, cancellationToken);

        var matches = allSops
            .Select(s => new
            {
                Sop = s,
                Score = CalculateMatchScore(searchTerms, s.Topic, s.TaskKeywords, s.Description)
            })
            .Where(x => x.Score > 0.1)
            .OrderByDescending(x => x.Score)
            .Take(3)
            .Select(x => new SopMatchDto
            {
                Id = x.Sop.Id,
                SopId = x.Sop.SopId,
                Topic = x.Sop.Topic,
                PolicySnippet = x.Sop.PolicySnippet,
                MatchScore = x.Score
            })
            .ToList();

        return matches;
    }

    private static List<string> ExtractKeywords(params string?[] inputs)
    {
        var stopWords = new HashSet<string>
        {
            "the", "a", "an", "and", "or", "of", "to", "in", "for", "on", "with",
            "at", "by", "from", "is", "are", "was", "were", "be", "been", "being",
            "has", "have", "had", "do", "does", "did", "will", "would", "could",
            "should", "may", "might", "must", "shall", "can", "that", "which",
            "who", "whom", "this", "these", "those", "it", "its"
        };

        return inputs
            .Where(i => !string.IsNullOrEmpty(i))
            .SelectMany(i => i!.ToLower().Split(new[] { ' ', ',', '.', '-', '_', '/', '(', ')' }, StringSplitOptions.RemoveEmptyEntries))
            .Where(w => w.Length > 2 && !stopWords.Contains(w))
            .Distinct()
            .ToList();
    }

    private static double CalculateMatchScore(List<string> keywords, params string?[] fields)
    {
        if (keywords.Count == 0) return 0;

        var fieldText = string.Join(" ", fields.Where(f => !string.IsNullOrEmpty(f))).ToLower();
        if (string.IsNullOrEmpty(fieldText)) return 0;

        var matchCount = keywords.Count(k => fieldText.Contains(k));
        return (double)matchCount / keywords.Count;
    }

    private async Task<AiSuggestionResult?> GenerateAiSuggestionsAsync(
        ControlMeasureSuggestionRequestDto request,
        List<HazardMatchDto> hazards,
        List<ControlMatchDto> controls,
        List<LegislationMatchDto> legislation,
        List<SopMatchDto> sops,
        CancellationToken cancellationToken)
    {
        var apiKey = _configuration["Anthropic:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Anthropic API key not configured, skipping AI suggestions");
            return null;
        }

        try
        {
            var prompt = BuildPrompt(request, hazards, controls, legislation, sops);

            var requestBody = new
            {
                model = "claude-3-sonnet-20240229",
                max_tokens = 1024,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var httpClient = _httpClientFactory.CreateClient("ClaudeApi");
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.Add("x-api-key", apiKey);
            httpRequest.Headers.Add("anthropic-version", "2023-06-01");

            var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Claude API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                return null;
            }

            var result = JsonSerializer.Deserialize<ClaudeResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var aiText = result?.Content?.FirstOrDefault()?.Text;
            if (string.IsNullOrEmpty(aiText))
                return null;

            // Parse the AI response
            return ParseAiResponse(aiText, result?.Usage?.InputTokens ?? 0, result?.Usage?.OutputTokens ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call Claude API");
            return null;
        }
    }

    private static string BuildPrompt(
        ControlMeasureSuggestionRequestDto request,
        List<HazardMatchDto> hazards,
        List<ControlMatchDto> controls,
        List<LegislationMatchDto> legislation,
        List<SopMatchDto> sops)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are a health and safety expert helping to complete a Risk Assessment and Method Statement (RAMS) for construction work.");
        sb.AppendLine();
        sb.AppendLine("## Task Details:");
        sb.AppendLine($"- Task/Activity: {request.TaskActivity}");
        sb.AppendLine($"- Hazard Identified: {request.HazardIdentified}");
        if (!string.IsNullOrEmpty(request.LocationArea))
            sb.AppendLine($"- Location: {request.LocationArea}");
        if (!string.IsNullOrEmpty(request.WhoAtRisk))
            sb.AppendLine($"- Who is at risk: {request.WhoAtRisk}");
        if (!string.IsNullOrEmpty(request.ProjectType))
            sb.AppendLine($"- Project Type: {request.ProjectType}");
        if (request.InitialLikelihood.HasValue && request.InitialSeverity.HasValue)
            sb.AppendLine($"- Initial Risk: L{request.InitialLikelihood} × S{request.InitialSeverity} = {request.InitialLikelihood * request.InitialSeverity}");
        sb.AppendLine();

        if (controls.Count > 0)
        {
            sb.AppendLine("## Existing Control Measures from Library (consider these):");
            foreach (var c in controls.Take(5))
            {
                sb.AppendLine($"- [{c.Hierarchy}] {c.Name}: {c.Description}");
            }
            sb.AppendLine();
        }

        if (legislation.Count > 0)
        {
            sb.AppendLine("## Relevant Legislation:");
            foreach (var l in legislation.Take(3))
            {
                sb.AppendLine($"- {l.Code}: {l.Name}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("## Your Task:");
        sb.AppendLine("Provide specific, practical control measures following the hierarchy of controls (Elimination → Substitution → Engineering → Administrative → PPE).");
        sb.AppendLine();
        sb.AppendLine("Respond in this exact format:");
        sb.AppendLine("CONTROL_MEASURES:");
        sb.AppendLine("[List specific control measures, one per line, starting with bullet points]");
        sb.AppendLine();
        sb.AppendLine("LEGISLATION:");
        sb.AppendLine("[List relevant UK/Ireland legislation references]");
        sb.AppendLine();
        sb.AppendLine("RESIDUAL_RISK:");
        sb.AppendLine("Likelihood: [1-5]");
        sb.AppendLine("Severity: [1-5]");

        return sb.ToString();
    }

    private static AiSuggestionResult ParseAiResponse(string aiText, int inputTokens, int outputTokens)
    {
        var result = new AiSuggestionResult
        {
            InputTokens = inputTokens,
            OutputTokens = outputTokens
        };

        // Parse CONTROL_MEASURES section
        var controlsMatch = Regex.Match(
            aiText,
            @"CONTROL_MEASURES:\s*(.*?)(?=LEGISLATION:|RESIDUAL_RISK:|$)",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        if (controlsMatch.Success)
        {
            result.ControlMeasures = controlsMatch.Groups[1].Value.Trim();
        }

        // Parse LEGISLATION section
        var legislationMatch = Regex.Match(
            aiText,
            @"LEGISLATION:\s*(.*?)(?=RESIDUAL_RISK:|$)",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        if (legislationMatch.Success)
        {
            result.Legislation = legislationMatch.Groups[1].Value.Trim();
        }

        // Parse RESIDUAL_RISK section
        var likelihoodMatch = Regex.Match(aiText, @"Likelihood:\s*(\d)", RegexOptions.IgnoreCase);
        var severityMatch = Regex.Match(aiText, @"Severity:\s*(\d)", RegexOptions.IgnoreCase);

        if (likelihoodMatch.Success && int.TryParse(likelihoodMatch.Groups[1].Value, out var likelihood))
        {
            result.SuggestedResidualLikelihood = Math.Clamp(likelihood, 1, 5);
        }

        if (severityMatch.Success && int.TryParse(severityMatch.Groups[1].Value, out var severity))
        {
            result.SuggestedResidualSeverity = Math.Clamp(severity, 1, 5);
        }

        return result;
    }

    private class AiSuggestionResult
    {
        public string? ControlMeasures { get; set; }
        public string? Legislation { get; set; }
        public int? SuggestedResidualLikelihood { get; set; }
        public int? SuggestedResidualSeverity { get; set; }
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
    }

    private class ClaudeResponse
    {
        public List<ClaudeContent>? Content { get; set; }
        public ClaudeUsage? Usage { get; set; }
    }

    private class ClaudeContent
    {
        public string? Text { get; set; }
    }

    private class ClaudeUsage
    {
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
    }
}
