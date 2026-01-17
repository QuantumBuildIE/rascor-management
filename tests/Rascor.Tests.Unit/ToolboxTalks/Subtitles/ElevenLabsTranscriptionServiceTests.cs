using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Rascor.Modules.ToolboxTalks.Infrastructure.Configuration;
using Rascor.Modules.ToolboxTalks.Infrastructure.Services.Subtitles;

namespace Rascor.Tests.Unit.ToolboxTalks.Subtitles;

/// <summary>
/// Unit tests for ElevenLabsTranscriptionService
/// </summary>
public class ElevenLabsTranscriptionServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<ElevenLabsTranscriptionService>> _loggerMock;
    private readonly IOptions<SubtitleProcessingSettings> _settings;

    public ElevenLabsTranscriptionServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _loggerMock = new Mock<ILogger<ElevenLabsTranscriptionService>>();

        var settings = new SubtitleProcessingSettings
        {
            ElevenLabs = new ElevenLabsSettings
            {
                ApiKey = "test-api-key",
                Model = "scribe_v1",
                BaseUrl = "https://api.elevenlabs.io/v1"
            }
        };
        _settings = Options.Create(settings);
    }

    [Fact]
    public async Task TranscribeAsync_WithSuccessfulResponse_ReturnsWords()
    {
        // Arrange
        var responseJson = new
        {
            words = new[]
            {
                new { text = "Hello", type = "word", start = 0.0, end = 0.5 },
                new { text = "World", type = "word", start = 0.6, end = 1.0 }
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(responseJson));

        var sut = CreateService();

        // Act
        var result = await sut.TranscribeAsync("https://example.com/video.mp4");

        // Assert
        result.Success.Should().BeTrue();
        result.Words.Should().HaveCount(2);
        result.Words[0].Text.Should().Be("Hello");
        result.Words[0].Start.Should().Be(0.0m);
        result.Words[0].End.Should().Be(0.5m);
        result.Words[1].Text.Should().Be("World");
    }

    [Fact]
    public async Task TranscribeAsync_WithEmptyWordsArray_ReturnsEmptyList()
    {
        // Arrange
        var responseJson = new { words = Array.Empty<object>() };

        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(responseJson));

        var sut = CreateService();

        // Act
        var result = await sut.TranscribeAsync("https://example.com/video.mp4");

        // Assert
        result.Success.Should().BeTrue();
        result.Words.Should().BeEmpty();
    }

    [Fact]
    public async Task TranscribeAsync_WithMissingWordsProperty_ReturnsEmptyList()
    {
        // Arrange
        var responseJson = new { text = "Hello World" }; // No 'words' property

        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(responseJson));

        var sut = CreateService();

        // Act
        var result = await sut.TranscribeAsync("https://example.com/video.mp4");

        // Assert
        result.Success.Should().BeTrue();
        result.Words.Should().BeEmpty();
    }

    [Fact]
    public async Task TranscribeAsync_WithApiError_ReturnsFailure()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadRequest, "Invalid request");

        var sut = CreateService();

        // Act
        var result = await sut.TranscribeAsync("https://example.com/video.mp4");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("ElevenLabs API error");
        result.ErrorMessage.Should().Contain("BadRequest");
    }

    [Fact]
    public async Task TranscribeAsync_WithRateLimitError_ReturnsFailure()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.TooManyRequests, "Rate limit exceeded");

        var sut = CreateService();

        // Act
        var result = await sut.TranscribeAsync("https://example.com/video.mp4");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("ElevenLabs API error");
    }

    [Fact]
    public async Task TranscribeAsync_WithUnauthorizedError_ReturnsFailure()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Unauthorized, "Invalid API key");

        var sut = CreateService();

        // Act
        var result = await sut.TranscribeAsync("https://example.com/video.mp4");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("ElevenLabs API error");
    }

    [Fact]
    public async Task TranscribeAsync_WithServerError_ReturnsFailure()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.InternalServerError, "Server error");

        var sut = CreateService();

        // Act
        var result = await sut.TranscribeAsync("https://example.com/video.mp4");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("ElevenLabs API error");
    }

    [Fact]
    public async Task TranscribeAsync_WithHttpException_ReturnsFailure()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var sut = CreateService();

        // Act
        var result = await sut.TranscribeAsync("https://example.com/video.mp4");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("HTTP request failed");
    }

    [Fact]
    public async Task TranscribeAsync_WithInvalidJson_ReturnsFailure()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "not valid json {{{");

        var sut = CreateService();

        // Act
        var result = await sut.TranscribeAsync("https://example.com/video.mp4");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("parse");
    }

    [Fact]
    public async Task TranscribeAsync_IncludesRawResponseOnSuccess()
    {
        // Arrange
        var responseJson = new { words = new[] { new { text = "Test", type = "word", start = 0.0, end = 0.5 } } };
        var responseString = JsonSerializer.Serialize(responseJson);

        SetupHttpResponse(HttpStatusCode.OK, responseString);

        var sut = CreateService();

        // Act
        var result = await sut.TranscribeAsync("https://example.com/video.mp4");

        // Assert
        result.RawResponse.Should().Be(responseString);
    }

    [Fact]
    public async Task TranscribeAsync_HandlesSpacingType()
    {
        // Arrange
        var responseJson = new
        {
            words = new[]
            {
                new { text = "Hello", type = "word", start = 0.0, end = 0.5 },
                new { text = " ", type = "spacing", start = 0.5, end = 0.6 },
                new { text = "World", type = "word", start = 0.6, end = 1.0 }
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(responseJson));

        var sut = CreateService();

        // Act
        var result = await sut.TranscribeAsync("https://example.com/video.mp4");

        // Assert
        result.Success.Should().BeTrue();
        result.Words.Should().HaveCount(3);
        result.Words[1].Type.Should().Be("spacing");
    }

    [Fact]
    public async Task TranscribeAsync_HandlesPunctuationType()
    {
        // Arrange
        var responseJson = new
        {
            words = new[]
            {
                new { text = "Hello", type = "word", start = 0.0, end = 0.5 },
                new { text = ".", type = "punctuation", start = 0.5, end = 0.5 }
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(responseJson));

        var sut = CreateService();

        // Act
        var result = await sut.TranscribeAsync("https://example.com/video.mp4");

        // Assert
        result.Success.Should().BeTrue();
        result.Words.Should().HaveCount(2);
        result.Words[1].Type.Should().Be("punctuation");
    }

    [Fact]
    public async Task TranscribeAsync_HandlesAudioEventType()
    {
        // Arrange
        var responseJson = new
        {
            words = new[]
            {
                new { text = "(music)", type = "audio_event", start = 0.0, end = 2.0 },
                new { text = "Hello", type = "word", start = 2.5, end = 3.0 }
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(responseJson));

        var sut = CreateService();

        // Act
        var result = await sut.TranscribeAsync("https://example.com/video.mp4");

        // Assert
        result.Success.Should().BeTrue();
        result.Words.Should().HaveCount(2);
        result.Words[0].Type.Should().Be("audio_event");
    }

    [Fact]
    public async Task TranscribeAsync_HandlesMissingOptionalFields()
    {
        // Arrange
        var responseJson = """
        {
            "words": [
                { "text": "Hello" }
            ]
        }
        """;

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        var sut = CreateService();

        // Act
        var result = await sut.TranscribeAsync("https://example.com/video.mp4");

        // Assert
        result.Success.Should().BeTrue();
        result.Words.Should().HaveCount(1);
        result.Words[0].Text.Should().Be("Hello");
        result.Words[0].Type.Should().Be("word"); // Default value
        result.Words[0].Start.Should().Be(0);
        result.Words[0].End.Should().Be(0);
    }

    [Fact]
    public async Task TranscribeAsync_WithCancelledToken_ReturnsFailure()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException("Operation was cancelled"));

        var sut = CreateService();

        // Act
        var result = await sut.TranscribeAsync("https://example.com/video.mp4");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("failed");
    }

    [Fact]
    public async Task TranscribeAsync_SendsCorrectHeaders()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"words":[]}""")
            });

        var sut = CreateService();

        // Act
        await sut.TranscribeAsync("https://example.com/video.mp4");

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Should().Contain(h => h.Key == "xi-api-key");
    }

    private ElevenLabsTranscriptionService CreateService()
    {
        return new ElevenLabsTranscriptionService(_httpClient, _settings, _loggerMock.Object);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content)
            });
    }
}
