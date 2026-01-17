using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rascor.Modules.ToolboxTalks.Application.Abstractions.Subtitles;
using Rascor.Modules.ToolboxTalks.Infrastructure.Configuration;

namespace Rascor.Modules.ToolboxTalks.Infrastructure.Services.Subtitles;

/// <summary>
/// Transcription service implementation using ElevenLabs Speech-to-Text API.
/// Converts video audio to word-level transcription with timing data.
/// </summary>
public class ElevenLabsTranscriptionService : ITranscriptionService
{
    private readonly HttpClient _httpClient;
    private readonly SubtitleProcessingSettings _settings;
    private readonly ILogger<ElevenLabsTranscriptionService> _logger;

    public ElevenLabsTranscriptionService(
        HttpClient httpClient,
        IOptions<SubtitleProcessingSettings> settings,
        ILogger<ElevenLabsTranscriptionService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Transcribes audio from a video URL using ElevenLabs API.
    /// </summary>
    public async Task<TranscriptionResult> TranscribeAsync(string videoUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting transcription for video: {VideoUrl}", videoUrl);

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.ElevenLabs.BaseUrl}/speech-to-text");
            request.Headers.Add("xi-api-key", _settings.ElevenLabs.ApiKey);

            var formContent = new MultipartFormDataContent
            {
                { new StringContent(videoUrl), "cloud_storage_url" },
                { new StringContent(_settings.ElevenLabs.Model), "model_id" }
            };

            request.Content = formContent;

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("ElevenLabs API error: {StatusCode} - {Response}", response.StatusCode, responseBody);
                return TranscriptionResult.FailureResult($"ElevenLabs API error: {response.StatusCode} - {responseBody}");
            }

            var words = ParseTranscriptionResponse(responseBody);

            _logger.LogInformation("Transcription completed. Words extracted: {WordCount}", words.Count);

            return TranscriptionResult.SuccessResult(words, responseBody);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed during transcription for video: {VideoUrl}", videoUrl);
            return TranscriptionResult.FailureResult($"HTTP request failed: {ex.Message}");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse transcription response for video: {VideoUrl}", videoUrl);
            return TranscriptionResult.FailureResult($"Failed to parse transcription response: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transcription failed for video: {VideoUrl}", videoUrl);
            return TranscriptionResult.FailureResult($"Transcription failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Parses the ElevenLabs API response into a list of transcript words.
    /// </summary>
    private static List<TranscriptWord> ParseTranscriptionResponse(string responseBody)
    {
        var words = new List<TranscriptWord>();

        using var jsonDoc = JsonDocument.Parse(responseBody);

        if (!jsonDoc.RootElement.TryGetProperty("words", out var wordsElement))
            return words;

        foreach (var wordElement in wordsElement.EnumerateArray())
        {
            words.Add(new TranscriptWord
            {
                Text = wordElement.TryGetProperty("text", out var textEl)
                    ? textEl.GetString() ?? string.Empty
                    : string.Empty,
                Type = wordElement.TryGetProperty("type", out var typeEl)
                    ? typeEl.GetString() ?? "word"
                    : "word",
                Start = wordElement.TryGetProperty("start", out var startEl)
                    ? startEl.GetDecimal()
                    : 0,
                End = wordElement.TryGetProperty("end", out var endEl)
                    ? endEl.GetDecimal()
                    : 0
            });
        }

        return words;
    }
}
