namespace Rascor.Modules.StockManagement.Application.Features.StockOrders.DTOs;

public record CreateStockOrderDto(
    Guid SiteId,
    string SiteName,
    DateTime OrderDate,
    DateTime? RequiredDate,
    string RequestedBy,
    string? Notes,
    Guid SourceLocationId,
    List<CreateStockOrderLineDto> Lines,
    Guid? SourceProposalId = null,
    string? SourceProposalNumber = null
);
