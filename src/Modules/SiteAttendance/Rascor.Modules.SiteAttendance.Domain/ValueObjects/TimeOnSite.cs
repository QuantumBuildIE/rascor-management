namespace Rascor.Modules.SiteAttendance.Domain.ValueObjects;

public record TimeOnSite
{
    public int TotalMinutes { get; init; }

    public TimeOnSite(int totalMinutes)
    {
        if (totalMinutes < 0)
            throw new ArgumentException("Total minutes cannot be negative", nameof(totalMinutes));
        TotalMinutes = totalMinutes;
    }

    public decimal Hours => Math.Round(TotalMinutes / 60.0m, 2);
    public TimeSpan AsTimeSpan => TimeSpan.FromMinutes(TotalMinutes);

    public static TimeOnSite FromTimeSpan(TimeSpan timeSpan)
        => new((int)timeSpan.TotalMinutes);

    public static TimeOnSite Zero => new(0);
}
