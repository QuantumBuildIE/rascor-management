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
    public int TaskId { get; set; }

    /// <summary>
    /// ID of the project this task belongs to.
    /// </summary>
    [JsonPropertyName("project_id")]
    public int ProjectId { get; set; }

    /// <summary>
    /// ID of the person assigned to this task.
    /// </summary>
    [JsonPropertyName("people_id")]
    public int PeopleId { get; set; }

    /// <summary>
    /// Start date of the task (YYYY-MM-DD format).
    /// </summary>
    [JsonPropertyName("start_date")]
    public string StartDate { get; set; } = string.Empty;

    /// <summary>
    /// End date of the task (YYYY-MM-DD format).
    /// </summary>
    [JsonPropertyName("end_date")]
    public string EndDate { get; set; } = string.Empty;

    /// <summary>
    /// Scheduled hours for the task.
    /// </summary>
    [JsonPropertyName("hours")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public double Hours { get; set; }

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
    public int? PhaseId { get; set; }

    /// <summary>
    /// Parses the StartDate string to a DateOnly value.
    /// </summary>
    [JsonIgnore]
    public DateOnly? StartDateParsed =>
        DateOnly.TryParse(StartDate, out var date) ? date : null;

    /// <summary>
    /// Parses the EndDate string to a DateOnly value.
    /// </summary>
    [JsonIgnore]
    public DateOnly? EndDateParsed =>
        DateOnly.TryParse(EndDate, out var date) ? date : null;

    /// <summary>
    /// Gets the hours as a decimal value (for backward compatibility).
    /// </summary>
    [JsonIgnore]
    public decimal HoursParsed => (decimal)Hours;
}
