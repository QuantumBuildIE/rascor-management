namespace Rascor.Modules.Proposals.Application.DTOs;

/// <summary>
/// Result of a proposal to stock order conversion
/// </summary>
public record ConversionResultDto
{
    public bool Success { get; init; }
    public List<CreatedStockOrderDto> CreatedOrders { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Information about a stock order created from a proposal
/// </summary>
public record CreatedStockOrderDto
{
    public Guid StockOrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public int ItemCount { get; init; }
    public decimal TotalValue { get; init; }
}
