using MediatR;
using Rascor.Modules.ToolboxTalks.Application.Features.Certificates.DTOs;

namespace Rascor.Modules.ToolboxTalks.Application.Features.Certificates.Queries;

public record GetAdminCertificateDownloadQuery : IRequest<CertificateDownloadDto?>
{
    public Guid TenantId { get; init; }
    public Guid CertificateId { get; init; }
}
