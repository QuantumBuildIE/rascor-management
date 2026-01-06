namespace Rascor.Modules.StockManagement.Application.Features.StockOrders.DTOs;

public record StockOrderDto(
    Guid Id,
    string OrderNumber,
    Guid SiteId,
    string SiteName,
    DateTime OrderDate,
    DateTime? RequiredDate,
    string Status,
    decimal OrderTotal,
    string RequestedBy,
    string? ApprovedBy,
    DateTime? ApprovedDate,
    DateTime? CollectedDate,
    string? Notes,
    Guid SourceLocationId,
    string SourceLocationName,
    List<StockOrderLineDto> Lines,
    Guid? SourceProposalId = null,
    string? SourceProposalNumber = null
);
