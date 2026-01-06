using Rascor.Modules.Proposals.Application.DTOs;

namespace Rascor.Modules.Proposals.Application.Services;

/// <summary>
/// Service for converting proposals to stock orders
/// </summary>
public interface IProposalConversionService
{
    /// <summary>
    /// Check if a proposal can be converted to stock orders
    /// </summary>
    Task<bool> CanConvertAsync(Guid proposalId);

    /// <summary>
    /// Preview the conversion without actually creating orders
    /// </summary>
    Task<ConversionPreviewDto> PreviewConversionAsync(ConvertToStockOrderDto dto);

    /// <summary>
    /// Convert a proposal to stock orders
    /// </summary>
    Task<ConversionResultDto> ConvertToStockOrdersAsync(ConvertToStockOrderDto dto, string requestedBy);
}
