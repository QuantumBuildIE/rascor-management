using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rascor.Modules.ToolboxTalks.Application.Abstractions.Translations;
using Rascor.Modules.ToolboxTalks.Infrastructure.Configuration;

namespace Rascor.Modules.ToolboxTalks.Infrastructure.Services.Translations;

/// <summary>
/// Content translation service implementation using Claude (Anthropic) API.
/// Translates text and HTML content to different languages.
/// </summary>
public class ContentTranslationService : IContentTranslationService
{
    private readonly HttpClient _httpClient;
    private readonly SubtitleProcessingSettings _settings;
    private readonly ILogger<ContentTranslationService> _logger;

    public ContentTranslationService(
        HttpClient httpClient,
        IOptions<SubtitleProcessingSettings> settings,
        ILogger<ContentTranslationService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ContentTranslationResult> TranslateTextAsync(
        string text,
        string targetLanguage,
        bool isHtml = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return ContentTranslationResult.SuccessResult(string.Empty);
        }

        try
        {
            _logger.LogInformation("Translating content to {Language}, HTML: {IsHtml}", targetLanguage, isHtml);

            var prompt = BuildTranslationPrompt(text, targetLanguage, isHtml);
            var translatedText = await CallClaudeApiAsync(prompt, cancellationToken);

            if (string.IsNullOrWhiteSpace(translatedText))
            {
                _logger.LogWarning("Translation returned empty content for {Language}", targetLanguage);
                return ContentTranslationResult.FailureResult($"Translation to {targetLanguage} returned empty content");
            }

            _logger.LogInformation("Translation to {Language} completed successfully", targetLanguage);
            return ContentTranslationResult.SuccessResult(translatedText);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed during translation to {Language}", targetLanguage);
            return ContentTranslationResult.FailureResult($"HTTP request failed: {ex.Message}");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse translation response for {Language}", targetLanguage);
            return ContentTranslationResult.FailureResult($"Failed to parse translation response: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Translation to {Language} failed", targetLanguage);
            return ContentTranslationResult.FailureResult($"Translation failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<BatchTranslationResult> TranslateBatchAsync(
        IEnumerable<TranslationItem> items,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        var itemsList = items.ToList();
        if (itemsList.Count == 0)
        {
            return BatchTranslationResult.SuccessResult(new Dictionary<string, ContentTranslationResult>());
        }

        try
        {
            _logger.LogInformation("Translating batch of {Count} items to {Language}", itemsList.Count, targetLanguage);

            var prompt = BuildBatchTranslationPrompt(itemsList, targetLanguage);
            var responseText = await CallClaudeApiAsync(prompt, cancellationToken);

            var results = ParseBatchResponse(responseText, itemsList);

            _logger.LogInformation("Batch translation to {Language} completed", targetLanguage);
            return BatchTranslationResult.SuccessResult(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch translation to {Language} failed", targetLanguage);
            return BatchTranslationResult.FailureResult($"Batch translation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Calls the Claude API with the given prompt.
    /// </summary>
    private async Task<string> CallClaudeApiAsync(string prompt, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            model = _settings.Claude.Model,
            max_tokens = _settings.Claude.MaxTokens,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.Claude.BaseUrl}/messages");
        request.Headers.Add("x-api-key", _settings.Claude.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Claude API error: {StatusCode} - {Response}", response.StatusCode, responseBody);
            throw new HttpRequestException($"Claude API error: {response.StatusCode}");
        }

        return ParseClaudeResponse(responseBody);
    }

    /// <summary>
    /// Builds the translation prompt for plain text or HTML content.
    /// </summary>
    private static string BuildTranslationPrompt(string text, string targetLanguage, bool isHtml)
    {
        if (isHtml)
        {
            return $@"Translate the following HTML content to {targetLanguage}.
IMPORTANT: Keep all HTML tags exactly as they are. Only translate the text content between tags.
Return only the translated HTML, nothing else.

{text}";
        }

        return $@"Translate the following text to {targetLanguage}.
Return only the translated text, nothing else.

{text}";
    }

    /// <summary>
    /// Builds a batch translation prompt for multiple items.
    /// </summary>
    private static string BuildBatchTranslationPrompt(List<TranslationItem> items, string targetLanguage)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Translate the following items to {targetLanguage}.");
        sb.AppendLine("Return the translations as a JSON array with the same order as the input.");
        sb.AppendLine("Each element should be the translated text only.");
        sb.AppendLine("For HTML content (marked with [HTML]), preserve all HTML tags and only translate the text.");
        sb.AppendLine();
        sb.AppendLine("Items to translate:");
        sb.AppendLine("```");

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var prefix = item.IsHtml ? "[HTML] " : "";
            var context = !string.IsNullOrEmpty(item.Context) ? $" ({item.Context})" : "";
            sb.AppendLine($"{i + 1}. {prefix}{item.Text}{context}");
        }

        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("Return only a valid JSON array of translated strings, like:");
        sb.AppendLine("[\"translated text 1\", \"translated text 2\", ...]");

        return sb.ToString();
    }

    /// <summary>
    /// Parses the Claude API response to extract the translated text.
    /// </summary>
    private static string ParseClaudeResponse(string responseBody)
    {
        using var jsonDoc = JsonDocument.Parse(responseBody);

        if (!jsonDoc.RootElement.TryGetProperty("content", out var contentArray))
            return string.Empty;

        foreach (var item in contentArray.EnumerateArray())
        {
            if (item.TryGetProperty("text", out var textEl))
            {
                return textEl.GetString() ?? string.Empty;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Parses a batch translation response into individual results.
    /// </summary>
    private Dictionary<string, ContentTranslationResult> ParseBatchResponse(
        string responseText,
        List<TranslationItem> items)
    {
        var results = new Dictionary<string, ContentTranslationResult>();

        try
        {
            // Try to extract JSON array from the response
            var jsonStart = responseText.IndexOf('[');
            var jsonEnd = responseText.LastIndexOf(']');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonText = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var translations = JsonSerializer.Deserialize<List<string>>(jsonText);

                if (translations != null)
                {
                    for (int i = 0; i < items.Count && i < translations.Count; i++)
                    {
                        results[items[i].Key] = ContentTranslationResult.SuccessResult(translations[i]);
                    }

                    // Handle any items that didn't get a translation
                    for (int i = translations.Count; i < items.Count; i++)
                    {
                        results[items[i].Key] = ContentTranslationResult.FailureResult("No translation returned");
                    }

                    return results;
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse batch response as JSON array");
        }

        // Fallback: return failures - don't mask translation errors with original text
        foreach (var item in items)
        {
            results[item.Key] = ContentTranslationResult.FailureResult("Failed to parse translation response");
        }

        return results;
    }
}
