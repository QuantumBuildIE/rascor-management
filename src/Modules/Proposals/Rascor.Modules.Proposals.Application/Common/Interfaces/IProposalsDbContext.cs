using Microsoft.EntityFrameworkCore;
using Rascor.Modules.Proposals.Domain.Entities;

namespace Rascor.Modules.Proposals.Application.Common.Interfaces;

/// <summary>
/// Interface for the Proposals database context
/// </summary>
public interface IProposalsDbContext
{
    // DbSets
    DbSet<Proposal> Proposals { get; }
    DbSet<ProposalSection> ProposalSections { get; }
    DbSet<ProposalLineItem> ProposalLineItems { get; }
    DbSet<ProposalContact> ProposalContacts { get; }

    /// <summary>
    /// Save changes to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
