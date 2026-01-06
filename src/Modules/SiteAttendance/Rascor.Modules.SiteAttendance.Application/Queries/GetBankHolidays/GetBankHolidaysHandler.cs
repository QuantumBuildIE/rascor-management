using AutoMapper;
using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetBankHolidays;

public class GetBankHolidaysHandler : IRequestHandler<GetBankHolidaysQuery, IEnumerable<BankHolidayDto>>
{
    private readonly IBankHolidayRepository _bankHolidayRepository;
    private readonly IMapper _mapper;

    public GetBankHolidaysHandler(
        IBankHolidayRepository bankHolidayRepository,
        IMapper mapper)
    {
        _bankHolidayRepository = bankHolidayRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BankHolidayDto>> Handle(GetBankHolidaysQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.BankHoliday> bankHolidays;

        if (request.Year.HasValue)
        {
            bankHolidays = await _bankHolidayRepository.GetByYearAsync(
                request.TenantId,
                request.Year.Value,
                cancellationToken);
        }
        else
        {
            bankHolidays = await _bankHolidayRepository.GetAllAsync(
                request.TenantId,
                cancellationToken);
        }

        return _mapper.Map<IEnumerable<BankHolidayDto>>(bankHolidays.OrderBy(h => h.Date));
    }
}
