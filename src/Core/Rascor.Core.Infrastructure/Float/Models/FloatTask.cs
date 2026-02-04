using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rascor.Core.Infrastructure.Float.Models;

/// <summary>
/// Represents a scheduled task (allocation) in Float.
/// Tasks represent scheduled work for a person on a project.
/// </summary>
public class FloatTask
{
    /// <summary>
    /// Unique identifier for the task.
    /// </summary>
    [JsonPropertyName("task_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? TaskId { get; set; }

    /// <summary>
    /// ID of the project this task belongs to.
    /// </summary>
    [JsonPropertyName("project_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? ProjectId { get; set; }

    /// <summary>
    /// ID of the person assigned to this task (singular assignment).
    /// </summary>
    [JsonPropertyName("people_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? PeopleId { get; set; }

    /// <summary>
    /// IDs of people assigned to this task (multiple assignment).
    /// Float uses this when a task is assigned to multiple people.
    /// </summary>
    [JsonPropertyName("people_ids")]
    public List<int>? PeopleIds { get; set; }

    /// <summary>
    /// Start date of the task (YYYY-MM-DD format).
    /// </summary>
    [JsonPropertyName("start_date")]
    public string? StartDate { get; set; }

    /// <summary>
    /// End date of the task (YYYY-MM-DD format).
    /// </summary>
    [JsonPropertyName("end_date")]
    public string? EndDate { get; set; }

    /// <summary>
    /// Scheduled hours for the task.
    /// </summary>
    [JsonPropertyName("hours")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public double? Hours { get; set; }

    /// <summary>
    /// Name/title of the task (optional).
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Notes/description for the task (optional).
    /// </summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    /// <summary>
    /// Phase ID if the task is part of a project phase.
    /// </summary>
    [JsonPropertyName("phase_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? PhaseId { get; set; }

    /// <summary>
    /// Parses the StartDate string to a DateOnly value.
    /// </summary>
    [JsonIgnore]
    public DateOnly? StartDateParsed =>
        !string.IsNullOrEmpty(StartDate) && DateOnly.TryParse(StartDate, out var date) ? date : null;

    /// <summary>
    /// Parses the EndDate string to a DateOnly value.
    /// </summary>
    [JsonIgnore]
    public DateOnly? EndDateParsed =>
        !string.IsNullOrEmpty(EndDate) && DateOnly.TryParse(EndDate, out var date) ? date : null;

    /// <summary>
    /// Gets the hours as a decimal value (for backward compatibility).
    /// </summary>
    [JsonIgnore]
    public decimal? HoursParsed => Hours.HasValue ? (decimal)Hours.Value : null;

    /// <summary>
    /// Captures any additional properties from the API that aren't explicitly mapped.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
