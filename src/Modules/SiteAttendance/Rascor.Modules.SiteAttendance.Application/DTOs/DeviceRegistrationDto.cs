namespace Rascor.Modules.SiteAttendance.Application.DTOs;

public record DeviceRegistrationDto
{
    public Guid Id { get; init; }
    public Guid? EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public string DeviceIdentifier { get; init; } = null!;
    public string? DeviceName { get; init; }
    public string? Platform { get; init; }
    public DateTime RegisteredAt { get; init; }
    public DateTime? LastActiveAt { get; init; }
    public bool IsActive { get; init; }
}
