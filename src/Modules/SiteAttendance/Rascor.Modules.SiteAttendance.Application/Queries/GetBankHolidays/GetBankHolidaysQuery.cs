using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetBankHolidays;

public record GetBankHolidaysQuery : IRequest<IEnumerable<BankHolidayDto>>
{
    public Guid TenantId { get; init; }
    public int? Year { get; init; }
}
