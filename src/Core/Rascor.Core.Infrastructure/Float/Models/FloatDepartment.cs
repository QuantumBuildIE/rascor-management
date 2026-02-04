using System.Text.Json;
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
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? DepartmentId { get; set; }

    /// <summary>
    /// Name of the department.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Captures any additional properties from the API that aren't explicitly mapped.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
