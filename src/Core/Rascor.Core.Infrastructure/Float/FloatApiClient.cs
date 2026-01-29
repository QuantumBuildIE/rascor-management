using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rascor.Core.Infrastructure.Float.Models;

namespace Rascor.Core.Infrastructure.Float;

/// <summary>
/// HTTP client implementation for the Float.com scheduling API.
/// Handles authentication, pagination, and error handling.
/// </summary>
public class FloatApiClient : IFloatApiClient
{
    private readonly HttpClient _httpClient;
    private readonly FloatSettings _settings;
    private readonly ILogger<FloatApiClient> _logger;

    /// <summary>
    /// Maximum number of items per page (Float API limit).
    /// </summary>
    private const int MaxPerPage = 200;

    public FloatApiClient(
        HttpClient httpClient,
        IOptions<FloatSettings> settings,
        ILogger<FloatApiClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        ConfigureHttpClient();
    }

    /// <inheritdoc />
    public bool IsConfigured =>
        _settings.Enabled && !string.IsNullOrEmpty(_settings.ApiKey);

    /// <inheritdoc />
    public async Task<List<FloatPerson>> GetPeopleAsync(CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("Float API is not configured. Returning empty people list");
            return new List<FloatPerson>();
        }

        try
        {
            return await GetAllPagesAsync<FloatPerson>("people", ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch people from Float API");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<FloatProject>> GetProjectsAsync(CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("Float API is not configured. Returning empty projects list");
            return new List<FloatProject>();
        }

        try
        {
            return await GetAllPagesAsync<FloatProject>("projects", ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch projects from Float API");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<FloatTask>> GetTasksForDateAsync(DateOnly date, CancellationToken ct = default)
    {
        return await GetTasksForDateRangeAsync(date, date, ct);
    }

    /// <inheritdoc />
    public async Task<List<FloatTask>> GetTasksForDateRangeAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("Float API is not configured. Returning empty tasks list");
            return new List<FloatTask>();
        }

        try
        {
            var endpoint = $"tasks?start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}";
            return await GetAllPagesAsync<FloatTask>(endpoint, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch tasks from Float API for date range {StartDate} to {EndDate}",
                startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// Configures the HTTP client with authentication and headers.
    /// </summary>
    private void ConfigureHttpClient()
    {
        if (!string.IsNullOrEmpty(_settings.BaseUrl))
        {
            // Ensure trailing slash for proper path resolution with HttpClient
            var baseUrl = _settings.BaseUrl.TrimEnd('/') + "/";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        }

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrEmpty(_settings.UserAgent))
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_settings.UserAgent);
        }
    }

    /// <summary>
    /// Fetches all pages of data from a paginated Float API endpoint.
    /// </summary>
    /// <typeparam name="T">The type of items to fetch</typeparam>
    /// <param name="endpoint">The API endpoint (may include query parameters)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Combined list of all items from all pages</returns>
    private async Task<List<T>> GetAllPagesAsync<T>(string endpoint, CancellationToken ct)
    {
        var allItems = new List<T>();
        var page = 1;

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            var url = BuildPaginatedUrl(endpoint, page);
            _logger.LogDebug("Fetching Float API page {Page} from {Url}", page, url);

            var response = await _httpClient.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError(
                    "Float API request failed with status {StatusCode}: {Error}",
                    response.StatusCode, errorContent);
                response.EnsureSuccessStatusCode(); // Throw for non-success status
            }

            var items = await response.Content.ReadFromJsonAsync<List<T>>(ct);

            if (items == null || items.Count == 0)
            {
                _logger.LogDebug("No more items on page {Page}, stopping pagination", page);
                break;
            }

            allItems.AddRange(items);
            _logger.LogDebug("Fetched {Count} items on page {Page}, total so far: {Total}",
                items.Count, page, allItems.Count);

            // If we got fewer items than the max, we've reached the last page
            if (items.Count < MaxPerPage)
            {
                break;
            }

            page++;
        }

        _logger.LogInformation("Fetched total of {Count} items from Float API endpoint {Endpoint}",
            allItems.Count, endpoint);

        return allItems;
    }

    /// <summary>
    /// Builds a URL with pagination parameters.
    /// </summary>
    private static string BuildPaginatedUrl(string endpoint, int page)
    {
        var separator = endpoint.Contains('?') ? '&' : '?';
        return $"{endpoint}{separator}page={page}&per-page={MaxPerPage}";
    }
}
