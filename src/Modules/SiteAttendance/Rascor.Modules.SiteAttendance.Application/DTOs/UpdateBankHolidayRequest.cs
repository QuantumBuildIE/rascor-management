namespace Rascor.Modules.SiteAttendance.Application.DTOs;

public record UpdateBankHolidayRequest
{
    public DateOnly Date { get; init; }
    public string? Name { get; init; }
}
