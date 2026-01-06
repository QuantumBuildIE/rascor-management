using MediatR;

namespace Rascor.Modules.SiteAttendance.Application.Commands.DeleteBankHoliday;

public record DeleteBankHolidayCommand : IRequest<bool>
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
}
