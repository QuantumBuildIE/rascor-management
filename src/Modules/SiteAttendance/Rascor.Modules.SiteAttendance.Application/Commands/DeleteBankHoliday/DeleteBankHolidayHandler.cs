using MediatR;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Commands.DeleteBankHoliday;

public class DeleteBankHolidayHandler : IRequestHandler<DeleteBankHolidayCommand, bool>
{
    private readonly IBankHolidayRepository _bankHolidayRepository;

    public DeleteBankHolidayHandler(IBankHolidayRepository bankHolidayRepository)
    {
        _bankHolidayRepository = bankHolidayRepository;
    }

    public async Task<bool> Handle(DeleteBankHolidayCommand request, CancellationToken cancellationToken)
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

        await _bankHolidayRepository.DeleteAsync(request.Id, cancellationToken);

        return true;
    }
}
