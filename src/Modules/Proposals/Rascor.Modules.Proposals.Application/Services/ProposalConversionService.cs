using Microsoft.EntityFrameworkCore;
using Rascor.Core.Application.Interfaces;
using Rascor.Modules.Proposals.Application.Common.Interfaces;
using Rascor.Modules.Proposals.Application.DTOs;
using Rascor.Modules.Proposals.Domain.Entities;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Modules.StockManagement.Application.Features.StockOrders;
using Rascor.Modules.StockManagement.Application.Features.StockOrders.DTOs;

namespace Rascor.Modules.Proposals.Application.Services;

/// <summary>
/// Service for converting proposals to stock orders
/// </summary>
public class ProposalConversionService : IProposalConversionService
{
    private readonly IProposalsDbContext _proposalsDb;
    private readonly IStockManagementDbContext _stockDb;
    private readonly ICoreDbContext _coreDb;
    private readonly IStockOrderService _stockOrderService;

    public ProposalConversionService(
        IProposalsDbContext proposalsDb,
        IStockManagementDbContext stockDb,
        ICoreDbContext coreDb,
        IStockOrderService stockOrderService)
    {
        _proposalsDb = proposalsDb;
        _stockDb = stockDb;
        _coreDb = coreDb;
        _stockOrderService = stockOrderService;
    }

    public async Task<bool> CanConvertAsync(Guid proposalId)
    {
        var proposal = await _proposalsDb.Proposals.FindAsync(proposalId);
        if (proposal == null) return false;

        // Can only convert Won proposals
        return proposal.Status == ProposalStatus.Won;
    }

    public async Task<ConversionPreviewDto> PreviewConversionAsync(ConvertToStockOrderDto dto)
    {
        var proposal = await GetProposalWithDetailsAsync(dto.ProposalId);
        if (proposal == null)
        {
            return new ConversionPreviewDto
            {
                TotalItems = 0,
                TotalQuantity = 0,
                TotalValue = 0,
                HasStockWarnings = false,
                HasAdHocItems = false,
                Items = new List<ConversionPreviewItemDto>()
            };
        }

        var lineItems = GetLineItemsForConversion(proposal, dto);

        var preview = new ConversionPreviewDto
        {
            TotalItems = lineItems.Count,
            TotalQuantity = lineItems.Sum(i => i.Quantity),
            TotalValue = lineItems.Sum(i => i.LineTotal),
            Items = new List<ConversionPreviewItemDto>()
        };

        var previewItems = new List<ConversionPreviewItemDto>();

        foreach (var item in lineItems)
        {
            if (item.ProductId.HasValue)
            {
                var stockLevel = await _stockDb.StockLevels
                    .FirstOrDefaultAsync(s =>
                        s.ProductId == item.ProductId &&
                        s.LocationId == dto.SourceLocationId);

                var availableStock = stockLevel?.QuantityOnHand - (stockLevel?.QuantityReserved ?? 0) ?? 0;

                previewItems.Add(new ConversionPreviewItemDto
                {
                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    AvailableStock = availableStock,
                    HasSufficientStock = availableStock >= item.Quantity,
                    IsAdHocItem = false,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.LineTotal
                });
            }
            else
            {
                // Ad-hoc items (no product) - warn but allow
                previewItems.Add(new ConversionPreviewItemDto
                {
                    ProductId = null,
                    ProductCode = null,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    AvailableStock = 0,
                    HasSufficientStock = false,
                    IsAdHocItem = true,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.LineTotal
                });
            }
        }

        return preview with
        {
            Items = previewItems,
            HasStockWarnings = previewItems.Any(i => !i.HasSufficientStock && !i.IsAdHocItem),
            HasAdHocItems = previewItems.Any(i => i.IsAdHocItem)
        };
    }

    public async Task<ConversionResultDto> ConvertToStockOrdersAsync(ConvertToStockOrderDto dto, string requestedBy)
    {
        // Validate proposal status
        if (!await CanConvertAsync(dto.ProposalId))
        {
            return new ConversionResultDto
            {
                Success = false,
                ErrorMessage = "Only won proposals can be converted to stock orders"
            };
        }

        var proposal = await GetProposalWithDetailsAsync(dto.ProposalId);
        if (proposal == null)
        {
            return new ConversionResultDto
            {
                Success = false,
                ErrorMessage = "Proposal not found"
            };
        }

        // Get the site name
        var site = await _coreDb.Sites.FirstOrDefaultAsync(s => s.Id == dto.SiteId);
        if (site == null)
        {
            return new ConversionResultDto
            {
                Success = false,
                ErrorMessage = "Site not found"
            };
        }

        var lineItems = GetLineItemsForConversion(proposal, dto);

        // Filter to only items with ProductId (can't order ad-hoc items)
        var orderableItems = lineItems.Where(i => i.ProductId.HasValue).ToList();
        var adHocItems = lineItems.Where(i => !i.ProductId.HasValue).ToList();

        var warnings = new List<string>();

        if (adHocItems.Any())
        {
            warnings.Add($"{adHocItems.Count} ad-hoc item(s) were skipped (no product linked)");
        }

        if (!orderableItems.Any())
        {
            return new ConversionResultDto
            {
                Success = false,
                ErrorMessage = "No orderable items found (all items are ad-hoc)",
                Warnings = warnings
            };
        }

        // Build notes
        var notes = $"Created from Proposal {proposal.ProposalNumber}";
        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            notes += $"\n{dto.Notes}";
        }

        // Create stock order DTO
        var stockOrderDto = new CreateStockOrderDto(
            SiteId: dto.SiteId,
            SiteName: site.SiteName,
            OrderDate: DateTime.UtcNow,
            RequiredDate: dto.RequiredDate.HasValue
                ? DateTime.SpecifyKind(dto.RequiredDate.Value, DateTimeKind.Utc)
                : null,
            RequestedBy: requestedBy,
            Notes: notes,
            SourceLocationId: dto.SourceLocationId,
            Lines: orderableItems.Select(item => new CreateStockOrderLineDto(
                ProductId: item.ProductId!.Value,
                QuantityRequested: (int)Math.Ceiling(item.Quantity)  // Round up
            )).ToList(),
            SourceProposalId: proposal.Id,
            SourceProposalNumber: proposal.ProposalNumber
        );

        var result = await _stockOrderService.CreateAsync(stockOrderDto);

        if (!result.Success)
        {
            return new ConversionResultDto
            {
                Success = false,
                ErrorMessage = result.Message ?? "Failed to create stock order",
                Warnings = warnings
            };
        }

        var stockOrder = result.Data!;

        // Update proposal notes to track the conversion
        proposal.Notes = AppendNote(proposal.Notes,
            $"Stock Order {stockOrder.OrderNumber} created for {orderableItems.Count} items on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
        await _proposalsDb.SaveChangesAsync();

        return new ConversionResultDto
        {
            Success = true,
            CreatedOrders = new List<CreatedStockOrderDto>
            {
                new CreatedStockOrderDto
                {
                    StockOrderId = stockOrder.Id,
                    OrderNumber = stockOrder.OrderNumber,
                    ItemCount = orderableItems.Count,
                    TotalValue = stockOrder.OrderTotal
                }
            },
            Warnings = warnings
        };
    }

    private async Task<Proposal?> GetProposalWithDetailsAsync(Guid proposalId)
    {
        return await _proposalsDb.Proposals
            .Include(p => p.Sections)
                .ThenInclude(s => s.LineItems)
            .FirstOrDefaultAsync(p => p.Id == proposalId);
    }

    private static List<ProposalLineItem> GetLineItemsForConversion(Proposal proposal, ConvertToStockOrderDto dto)
    {
        var allItems = proposal.Sections.SelectMany(s => s.LineItems).ToList();

        return dto.Mode switch
        {
            ConversionMode.AllItems => allItems,
            ConversionMode.SelectedSections => allItems
                .Where(i => dto.SelectedSectionIds?.Contains(i.ProposalSectionId) == true)
                .ToList(),
            ConversionMode.SelectedItems => allItems
                .Where(i => dto.SelectedLineItemIds?.Contains(i.Id) == true)
                .ToList(),
            _ => allItems
        };
    }

    private static string? AppendNote(string? existingNotes, string newNote)
    {
        if (string.IsNullOrWhiteSpace(existingNotes))
        {
            return newNote;
        }
        return $"{existingNotes}\n\n{newNote}";
    }
}
