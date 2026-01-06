using Rascor.Modules.Proposals.Domain.Entities;

namespace Rascor.Modules.Proposals.Application.Services;

public interface IProposalCalculationService
{
    /// <summary>
    /// Calculates line item totals (LineTotal, LineCost, LineMargin, MarginPercent)
    /// </summary>
    ProposalLineItem CalculateLineItem(ProposalLineItem lineItem);

    /// <summary>
    /// Calculates section totals from its line items (SectionTotal, SectionCost, SectionMargin)
    /// </summary>
    Task<ProposalSection> CalculateSectionTotalsAsync(Guid sectionId);

    /// <summary>
    /// Calculates proposal totals from its sections (Subtotal, DiscountAmount, NetTotal, VatAmount, GrandTotal, TotalCost, TotalMargin, MarginPercent)
    /// </summary>
    Task<Proposal> CalculateProposalTotalsAsync(Guid proposalId);

    /// <summary>
    /// Full bottom-up recalculation: all line items → all sections → proposal
    /// Useful for fixing data or after bulk updates
    /// </summary>
    Task RecalculateAllAsync(Guid proposalId);
}
