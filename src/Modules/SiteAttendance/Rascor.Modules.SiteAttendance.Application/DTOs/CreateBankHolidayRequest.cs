namespace Rascor.Modules.SiteAttendance.Application.DTOs;

public record CreateBankHolidayRequest
{
    public DateOnly Date { get; init; }
    public string? Name { get; init; }
}
