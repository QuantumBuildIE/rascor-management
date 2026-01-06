using Rascor.Core.Application.Features.Contacts.DTOs;
using Rascor.Core.Application.Models;

namespace Rascor.Core.Application.Features.Contacts;

public interface IContactService
{
    Task<Result<List<ContactDto>>> GetAllAsync();
    Task<Result<List<ContactDto>>> GetByCompanyIdAsync(Guid companyId);
    Task<Result<PaginatedList<ContactDto>>> GetPaginatedAsync(GetContactsQueryDto query);
    Task<Result<ContactDto>> GetByIdAsync(Guid id);
    Task<Result<ContactDto>> CreateAsync(CreateContactDto dto);
    Task<Result<ContactDto>> UpdateAsync(Guid id, UpdateContactDto dto);
    Task<Result> DeleteAsync(Guid id);
}
