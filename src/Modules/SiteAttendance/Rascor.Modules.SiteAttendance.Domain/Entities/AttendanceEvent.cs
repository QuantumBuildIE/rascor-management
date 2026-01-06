using Rascor.Core.Domain.Common;
using Rascor.Core.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Enums;

namespace Rascor.Modules.SiteAttendance.Domain.Entities;

/// <summary>
/// Raw GPS entry/exit events from mobile devices
/// </summary>
public class AttendanceEvent : TenantEntity
{
    public Guid EmployeeId { get; private set; }
    public Guid SiteId { get; private set; }
    public EventType EventType { get; private set; }
    public DateTime Timestamp { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public TriggerMethod TriggerMethod { get; private set; }
    public Guid? DeviceRegistrationId { get; private set; }
    public bool IsNoise { get; private set; }
    public decimal? NoiseDistance { get; private set; }
    public bool Processed { get; private set; }

    // Navigation properties (Core entities)
    public virtual Employee Employee { get; private set; } = null!;
    public virtual Site Site { get; private set; } = null!;
    public virtual DeviceRegistration? DeviceRegistration { get; private set; }

    private AttendanceEvent() { } // EF Core

    public static AttendanceEvent Create(
        Guid tenantId,
        Guid employeeId,
        Guid siteId,
        EventType eventType,
        DateTime timestamp,
        decimal? latitude,
        decimal? longitude,
        TriggerMethod triggerMethod,
        Guid? deviceRegistrationId = null)
    {
        return new AttendanceEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = employeeId,
            SiteId = siteId,
            EventType = eventType,
            Timestamp = timestamp,
            Latitude = latitude,
            Longitude = longitude,
            TriggerMethod = triggerMethod,
            DeviceRegistrationId = deviceRegistrationId,
            IsNoise = false,
            Processed = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsNoise(decimal distance)
    {
        IsNoise = true;
        NoiseDistance = distance;
    }

    public void MarkAsProcessed()
    {
        Processed = true;
    }
}
