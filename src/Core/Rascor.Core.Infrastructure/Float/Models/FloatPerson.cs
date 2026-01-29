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
    public int PeopleId { get; set; }

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
    public int Active { get; set; }

    /// <summary>
    /// Tags associated with the person.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Convenience property to check if the person is active.
    /// </summary>
    [JsonIgnore]
    public bool IsActive => Active == 1;
}
