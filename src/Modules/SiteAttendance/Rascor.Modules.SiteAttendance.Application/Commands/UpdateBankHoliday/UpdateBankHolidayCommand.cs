using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Commands.UpdateBankHoliday;

public record UpdateBankHolidayCommand : IRequest<BankHolidayDto>
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public DateOnly Date { get; init; }
    public string? Name { get; init; }
}
