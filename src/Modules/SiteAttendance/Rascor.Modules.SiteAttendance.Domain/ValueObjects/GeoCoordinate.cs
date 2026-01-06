namespace Rascor.Modules.SiteAttendance.Domain.ValueObjects;

public record GeoCoordinate
{
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }

    public GeoCoordinate(decimal latitude, decimal longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));
        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

        Latitude = latitude;
        Longitude = longitude;
    }

    // Haversine formula to calculate distance in meters
    public double DistanceTo(GeoCoordinate other)
    {
        const double EarthRadiusMeters = 6371000;

        var lat1Rad = (double)Latitude * Math.PI / 180.0;
        var lat2Rad = (double)other.Latitude * Math.PI / 180.0;
        var deltaLat = ((double)other.Latitude - (double)Latitude) * Math.PI / 180.0;
        var deltaLon = ((double)other.Longitude - (double)Longitude) * Math.PI / 180.0;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;
    }
}
