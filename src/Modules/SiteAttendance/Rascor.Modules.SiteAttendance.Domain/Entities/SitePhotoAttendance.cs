using Rascor.Core.Domain.Common;
using Rascor.Core.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Domain.Entities;

/// <summary>
/// SPA/RAMS compliance records - photo proof of site attendance
/// </summary>
public class SitePhotoAttendance : TenantEntity
{
    public Guid EmployeeId { get; private set; }
    public Guid SiteId { get; private set; }
    public DateOnly EventDate { get; private set; }
    public string? WeatherConditions { get; private set; }
    public string? ImageUrl { get; private set; }
    public decimal? DistanceToSite { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public string? Notes { get; private set; }

    // Navigation properties
    public virtual Employee Employee { get; private set; } = null!;
    public virtual Site Site { get; private set; } = null!;

    private SitePhotoAttendance() { } // EF Core

    public static SitePhotoAttendance Create(
        Guid tenantId,
        Guid employeeId,
        Guid siteId,
        DateOnly eventDate,
        string? weatherConditions = null,
        string? imageUrl = null,
        decimal? latitude = null,
        decimal? longitude = null,
        string? notes = null)
    {
        return new SitePhotoAttendance
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = employeeId,
            SiteId = siteId,
            EventDate = eventDate,
            WeatherConditions = weatherConditions,
            ImageUrl = imageUrl,
            Latitude = latitude,
            Longitude = longitude,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateImage(string imageUrl)
    {
        ImageUrl = imageUrl;
    }

    public void SetDistanceToSite(decimal distance)
    {
        DistanceToSite = distance;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
    }

    public void UpdateWeatherConditions(string? weatherConditions)
    {
        WeatherConditions = weatherConditions;
    }
}
