using System.Text.Json.Serialization;

namespace Rascor.Core.Infrastructure.Float.Models;

/// <summary>
/// Represents a department in Float.
/// </summary>
public class FloatDepartment
{
    /// <summary>
    /// Unique identifier for the department.
    /// </summary>
    [JsonPropertyName("department_id")]
    public int DepartmentId { get; set; }

    /// <summary>
    /// Name of the department.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
