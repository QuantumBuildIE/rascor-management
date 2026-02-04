using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rascor.Core.Infrastructure.Float.Models;

/// <summary>
/// Represents a person (team member) in Float.
/// </summary>
public class FloatPerson
{
    /// <summary>
    /// Unique identifier for the person in Float.
    /// </summary>
    [JsonPropertyName("people_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? PeopleId { get; set; }

    /// <summary>
    /// Full name of the person.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the person.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Job title of the person.
    /// </summary>
    [JsonPropertyName("job_title")]
    public string? JobTitle { get; set; }

    /// <summary>
    /// Department the person belongs to.
    /// </summary>
    [JsonPropertyName("department")]
    public FloatDepartment? Department { get; set; }

    /// <summary>
    /// Whether the person is active (1) or inactive (0).
    /// </summary>
    [JsonPropertyName("active")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? Active { get; set; }

    /// <summary>
    /// Tags associated with the person.
    /// Handles both string arrays and object arrays from Float API.
    /// </summary>
    [JsonPropertyName("tags")]
    [JsonConverter(typeof(FloatTagListConverter))]
    public List<FloatTag> Tags { get; set; } = new();

    /// <summary>
    /// Convenience property to check if the person is active.
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
