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
    public int ProjectId { get; set; }

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
    public int? ClientId { get; set; }

    /// <summary>
    /// Tags associated with the project.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Whether the project is active (1) or inactive (0).
    /// </summary>
    [JsonPropertyName("active")]
    public int Active { get; set; }

    /// <summary>
    /// Convenience property to check if the project is active.
    /// </summary>
    [JsonIgnore]
    public bool IsActive => Active == 1;
}
