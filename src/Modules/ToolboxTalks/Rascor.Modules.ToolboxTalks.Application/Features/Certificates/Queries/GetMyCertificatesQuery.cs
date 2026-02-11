using MediatR;
using Rascor.Modules.ToolboxTalks.Application.Features.Certificates.DTOs;

namespace Rascor.Modules.ToolboxTalks.Application.Features.Certificates.Queries;

public record GetMyCertificatesQuery : IRequest<List<CertificateDto>>
{
    public Guid TenantId { get; init; }
    public Guid EmployeeId { get; init; }
}
