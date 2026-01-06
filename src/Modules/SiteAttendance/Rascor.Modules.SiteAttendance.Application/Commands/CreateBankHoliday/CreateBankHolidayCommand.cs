using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Commands.CreateBankHoliday;

public record CreateBankHolidayCommand : IRequest<BankHolidayDto>
{
    public Guid TenantId { get; init; }
    public DateOnly Date { get; init; }
    public string? Name { get; init; }
}
