using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rascor.Core.Infrastructure.Float.Models;

/// <summary>
/// Represents a project in Float.
/// </summary>
public class FloatProject
{
    /// <summary>
    /// Unique identifier for the project.
    /// </summary>
    [JsonPropertyName("project_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? ProjectId { get; set; }

    /// <summary>
    /// Name of the project.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Project code (optional identifier used by the organization).
    /// </summary>
    [JsonPropertyName("project_code")]
    public string? ProjectCode { get; set; }

    /// <summary>
    /// Client ID associated with this project.
    /// </summary>
    [JsonPropertyName("client_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? ClientId { get; set; }

    /// <summary>
    /// Tags associated with the project.
    /// Handles both string arrays and object arrays from Float API.
    /// </summary>
    [JsonPropertyName("tags")]
    [JsonConverter(typeof(FloatTagListConverter))]
    public List<FloatTag> Tags { get; set; } = new();

    /// <summary>
    /// Whether the project is active (1) or inactive (0).
    /// </summary>
    [JsonPropertyName("active")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? Active { get; set; }

    /// <summary>
    /// Convenience property to check if the project is active.
    /// Defaults to false if Active is null.
    /// </summary>
    [JsonIgnore]
    public bool IsActive => Active == 1;

    /// <summary>
    /// Captures any additional properties from the API that aren't explicitly mapped.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
