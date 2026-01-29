using System.Text.Json.Serialization;

namespace Rascor.Core.Infrastructure.Float.Models;

/// <summary>
/// Represents a tag in Float.
/// </summary>
public class FloatTag
{
    /// <summary>
    /// Unique identifier for the tag.
    /// </summary>
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    /// <summary>
    /// Name of the tag.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
