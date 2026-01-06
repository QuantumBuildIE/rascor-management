using Microsoft.EntityFrameworkCore;
using Rascor.Modules.Proposals.Application.Common.Interfaces;
using Rascor.Modules.Proposals.Domain.Entities;

namespace Rascor.Modules.Proposals.Application.Services;

public class ProposalCalculationService : IProposalCalculationService
{
    private readonly IProposalsDbContext _dbContext;

    public ProposalCalculationService(IProposalsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Calculates line item totals (LineTotal, LineCost, LineMargin, MarginPercent)
    /// </summary>
    public ProposalLineItem CalculateLineItem(ProposalLineItem lineItem)
    {
        // Line totals
        lineItem.LineTotal = lineItem.Quantity * lineItem.UnitPrice;
        lineItem.LineCost = lineItem.Quantity * lineItem.UnitCost;
        lineItem.LineMargin = lineItem.LineTotal - lineItem.LineCost;

        // Margin percent (avoid divide by zero)
        lineItem.MarginPercent = lineItem.LineTotal > 0
            ? Math.Round((lineItem.LineMargin / lineItem.LineTotal) * 100, 2)
            : 0;

        return lineItem;
    }

    /// <summary>
    /// Calculates section totals from its line items (SectionTotal, SectionCost, SectionMargin)
    /// </summary>
    public async Task<ProposalSection> CalculateSectionTotalsAsync(Guid sectionId)
    {
        var section = await _dbContext.ProposalSections
            .Include(s => s.LineItems)
            .FirstOrDefaultAsync(s => s.Id == sectionId)
            ?? throw new InvalidOperationException($"Section {sectionId} not found");

        // Sum all line totals, costs, margins
        section.SectionTotal = section.LineItems.Sum(l => l.LineTotal);
        section.SectionCost = section.LineItems.Sum(l => l.LineCost);
        section.SectionMargin = section.SectionTotal - section.SectionCost;

        await _dbContext.SaveChangesAsync();
        return section;
    }

    /// <summary>
    /// Calculates proposal totals from its sections (Subtotal, DiscountAmount, NetTotal, VatAmount, GrandTotal, TotalCost, TotalMargin, MarginPercent)
    /// </summary>
    public async Task<Proposal> CalculateProposalTotalsAsync(Guid proposalId)
    {
        var proposal = await _dbContext.Proposals
            .Include(p => p.Sections)
            .FirstOrDefaultAsync(p => p.Id == proposalId)
            ?? throw new InvalidOperationException($"Proposal {proposalId} not found");

        // Subtotal = sum of all section totals
        proposal.Subtotal = proposal.Sections.Sum(s => s.SectionTotal);

        // Total cost = sum of all section costs
        proposal.TotalCost = proposal.Sections.Sum(s => s.SectionCost);

        // Discount calculation
        if (proposal.DiscountPercent > 0)
        {
            proposal.DiscountAmount = Math.Round(proposal.Subtotal * (proposal.DiscountPercent / 100), 2);
        }
        else
        {
            proposal.DiscountAmount = 0;
        }

        // Net total = subtotal - discount
        proposal.NetTotal = proposal.Subtotal - proposal.DiscountAmount;

        // VAT calculation
        proposal.VatAmount = Math.Round(proposal.NetTotal * (proposal.VatRate / 100), 2);

        // Grand total = net total + VAT
        proposal.GrandTotal = proposal.NetTotal + proposal.VatAmount;

        // Margin calculation (based on NetTotal, not GrandTotal)
        proposal.TotalMargin = proposal.NetTotal - proposal.TotalCost;
        proposal.MarginPercent = proposal.NetTotal > 0
            ? Math.Round((proposal.TotalMargin / proposal.NetTotal) * 100, 2)
            : 0;

        await _dbContext.SaveChangesAsync();
        return proposal;
    }

    /// <summary>
    /// Full bottom-up recalculation: all line items → all sections → proposal
    /// </summary>
    public async Task RecalculateAllAsync(Guid proposalId)
    {
        // Get proposal with all sections and line items
        var proposal = await _dbContext.Proposals
            .Include(p => p.Sections)
            .ThenInclude(s => s.LineItems)
            .FirstOrDefaultAsync(p => p.Id == proposalId)
            ?? throw new InvalidOperationException($"Proposal {proposalId} not found");

        // 1. Calculate all line items
        foreach (var section in proposal.Sections)
        {
            foreach (var lineItem in section.LineItems)
            {
                CalculateLineItem(lineItem);
            }
        }

        // 2. Calculate all section totals
        foreach (var section in proposal.Sections)
        {
            section.SectionTotal = section.LineItems.Sum(l => l.LineTotal);
            section.SectionCost = section.LineItems.Sum(l => l.LineCost);
            section.SectionMargin = section.SectionTotal - section.SectionCost;
        }

        // 3. Calculate proposal totals
        proposal.Subtotal = proposal.Sections.Sum(s => s.SectionTotal);
        proposal.TotalCost = proposal.Sections.Sum(s => s.SectionCost);

        if (proposal.DiscountPercent > 0)
        {
            proposal.DiscountAmount = Math.Round(proposal.Subtotal * (proposal.DiscountPercent / 100), 2);
        }
        else
        {
            proposal.DiscountAmount = 0;
        }

        proposal.NetTotal = proposal.Subtotal - proposal.DiscountAmount;
        proposal.VatAmount = Math.Round(proposal.NetTotal * (proposal.VatRate / 100), 2);
        proposal.GrandTotal = proposal.NetTotal + proposal.VatAmount;
        proposal.TotalMargin = proposal.NetTotal - proposal.TotalCost;
        proposal.MarginPercent = proposal.NetTotal > 0
            ? Math.Round((proposal.TotalMargin / proposal.NetTotal) * 100, 2)
            : 0;

        // 4. Save all changes
        await _dbContext.SaveChangesAsync();
    }
}
