using System.Text.Json.Serialization;

namespace Rascor.Modules.Proposals.Application.DTOs;

/// <summary>
/// DTO for converting a proposal to stock orders
/// </summary>
public record ConvertToStockOrderDto
{
    public Guid ProposalId { get; init; }
    public Guid SiteId { get; init; }  // Which site to deliver to
    public Guid SourceLocationId { get; init; }  // Which warehouse to pick from
    public DateTime? RequiredDate { get; init; }
    public string? Notes { get; init; }
    public ConversionMode Mode { get; init; } = ConversionMode.AllItems;
    public List<Guid>? SelectedSectionIds { get; init; }  // If Mode = SelectedSections
    public List<Guid>? SelectedLineItemIds { get; init; }  // If Mode = SelectedItems
}

/// <summary>
/// Mode for proposal to stock order conversion
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConversionMode
{
    AllItems,           // Convert entire proposal
    SelectedSections,   // Convert only selected sections
    SelectedItems       // Convert only selected line items
}
