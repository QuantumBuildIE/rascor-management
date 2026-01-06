namespace Rascor.Modules.Proposals.Application.DTOs;

/// <summary>
/// Preview of a proposal to stock order conversion
/// </summary>
public record ConversionPreviewDto
{
    public int TotalItems { get; init; }
    public decimal TotalQuantity { get; init; }
    public decimal TotalValue { get; init; }
    public bool HasStockWarnings { get; init; }
    public bool HasAdHocItems { get; init; }
    public List<ConversionPreviewItemDto> Items { get; init; } = new();
}

/// <summary>
/// Individual item in a conversion preview
/// </summary>
public record ConversionPreviewItemDto
{
    public Guid? ProductId { get; init; }
    public string? ProductCode { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal AvailableStock { get; init; }
    public bool HasSufficientStock { get; init; }
    public bool IsAdHocItem { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}
