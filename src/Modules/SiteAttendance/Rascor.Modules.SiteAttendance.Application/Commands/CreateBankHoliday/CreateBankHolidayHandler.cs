using AutoMapper;
using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Commands.CreateBankHoliday;

public class CreateBankHolidayHandler : IRequestHandler<CreateBankHolidayCommand, BankHolidayDto>
{
    private readonly IBankHolidayRepository _bankHolidayRepository;
    private readonly IMapper _mapper;

    public CreateBankHolidayHandler(
        IBankHolidayRepository bankHolidayRepository,
        IMapper mapper)
    {
        _bankHolidayRepository = bankHolidayRepository;
        _mapper = mapper;
    }

    public async Task<BankHolidayDto> Handle(CreateBankHolidayCommand request, CancellationToken cancellationToken)
    {
        // Check if bank holiday already exists for this date
        var exists = await _bankHolidayRepository.IsBankHolidayAsync(
            request.TenantId,
            request.Date,
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"A bank holiday already exists for {request.Date}.");
        }

        var bankHoliday = BankHoliday.Create(
            request.TenantId,
            request.Date,
            request.Name);

        await _bankHolidayRepository.AddAsync(bankHoliday, cancellationToken);

        return _mapper.Map<BankHolidayDto>(bankHoliday);
    }
}
