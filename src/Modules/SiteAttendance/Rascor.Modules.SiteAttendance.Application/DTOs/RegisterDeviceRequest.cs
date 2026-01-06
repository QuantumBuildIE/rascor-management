namespace Rascor.Modules.SiteAttendance.Application.DTOs;

public record RegisterDeviceRequest
{
    public string DeviceIdentifier { get; init; } = null!;
    public string? DeviceName { get; init; }
    public string? Platform { get; init; } // iOS, Android
    public string? PushToken { get; init; }
    public Guid? EmployeeId { get; init; }
}
