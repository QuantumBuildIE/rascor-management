using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Rascor.Core.Infrastructure.Float.Jobs;

namespace Rascor.Core.Infrastructure.Float;

/// <summary>
/// Extension methods for registering Float API services with dependency injection.
/// </summary>
public static class FloatServiceCollectionExtensions
{
    /// <summary>
    /// Adds Float API client services to the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddFloatApiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register settings
        services.Configure<FloatSettings>(
            configuration.GetSection(FloatSettings.SectionName));

        // Register HTTP client with retry policy
        services.AddHttpClient<IFloatApiClient, FloatApiClient>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy());

        // Register Float matching service
        services.AddScoped<IFloatMatchingService, FloatMatchingService>();

        // Register Float SPA check services
        services.AddScoped<IFloatSpaEmailTemplateService, FloatSpaEmailTemplateService>();
        services.AddScoped<IFloatSpaCheckService, FloatSpaCheckService>();

        // Register Float background job
        services.AddScoped<FloatSpaCheckJob>();

        return services;
    }

    /// <summary>
    /// Creates a Polly retry policy for handling transient HTTP errors.
    /// Uses exponential backoff with jitter.
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // Handles 5xx and 408 (Request Timeout)
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // Handle 429 (rate limiting)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                {
                    // Exponential backoff: 2^retryAttempt seconds (2s, 4s, 8s)
                    // Plus jitter to avoid thundering herd
                    var exponentialBackoff = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
                    return exponentialBackoff + jitter;
                },
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // Log retry attempts
                    // Note: Logging is handled by the FloatApiClient when the request ultimately fails
                });
    }
}
