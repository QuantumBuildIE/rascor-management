using AutoMapper;
using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Commands.UpdateBankHoliday;

public class UpdateBankHolidayHandler : IRequestHandler<UpdateBankHolidayCommand, BankHolidayDto>
{
    private readonly IBankHolidayRepository _bankHolidayRepository;
    private readonly IMapper _mapper;

    public UpdateBankHolidayHandler(
        IBankHolidayRepository bankHolidayRepository,
        IMapper mapper)
    {
        _bankHolidayRepository = bankHolidayRepository;
        _mapper = mapper;
    }

    public async Task<BankHolidayDto> Handle(UpdateBankHolidayCommand request, CancellationToken cancellationToken)
    {
        var bankHoliday = await _bankHolidayRepository.GetByIdAsync(request.Id, cancellationToken);

        if (bankHoliday == null)
        {
            throw new KeyNotFoundException($"Bank holiday with ID {request.Id} not found.");
        }

        if (bankHoliday.TenantId != request.TenantId)
        {
            throw new UnauthorizedAccessException("Access denied to this bank holiday.");
        }

        // Check if another bank holiday already exists for the new date
        if (bankHoliday.Date != request.Date)
        {
            var exists = await _bankHolidayRepository.IsBankHolidayAsync(
                request.TenantId,
                request.Date,
                cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException($"A bank holiday already exists for {request.Date}.");
            }
        }

        bankHoliday.Update(request.Date, request.Name);

        await _bankHolidayRepository.UpdateAsync(bankHoliday, cancellationToken);

        return _mapper.Map<BankHolidayDto>(bankHoliday);
    }
}
