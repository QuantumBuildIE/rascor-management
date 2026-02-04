using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rascor.Core.Infrastructure.Float.Models;

/// <summary>
/// Represents a tag in Float.
/// Tags can be returned as simple strings or full objects depending on the endpoint.
/// </summary>
[JsonConverter(typeof(FloatTagConverter))]
public class FloatTag
{
    /// <summary>
    /// Unique identifier for the tag.
    /// May be null when tag is returned as a simple string.
    /// </summary>
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    /// <summary>
    /// Name of the tag.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Captures any additional properties from the API that aren't explicitly mapped.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
