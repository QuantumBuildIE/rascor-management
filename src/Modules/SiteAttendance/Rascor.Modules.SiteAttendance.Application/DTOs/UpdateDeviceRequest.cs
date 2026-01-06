namespace Rascor.Modules.SiteAttendance.Application.DTOs;

public record UpdateDeviceRequest
{
    public string? DeviceName { get; init; }
    public string? PushToken { get; init; }
    public Guid? EmployeeId { get; init; }
    public bool? IsActive { get; init; }
}
