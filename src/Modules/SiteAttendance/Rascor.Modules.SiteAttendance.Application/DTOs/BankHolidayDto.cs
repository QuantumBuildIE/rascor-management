namespace Rascor.Modules.SiteAttendance.Application.DTOs;

public record BankHolidayDto
{
    public Guid Id { get; init; }
    public DateOnly Date { get; init; }
    public string? Name { get; init; }
}
