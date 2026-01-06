namespace Rascor.Modules.SiteAttendance.Application.DTOs;

public record RecordAttendanceEventRequest
{
    public Guid EmployeeId { get; init; }
    public Guid SiteId { get; init; }
    public string EventType { get; init; } = null!; // "Enter" or "Exit"
    public DateTime Timestamp { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string TriggerMethod { get; init; } = "Automatic"; // "Automatic" or "Manual"
    public string? DeviceIdentifier { get; init; }
}
