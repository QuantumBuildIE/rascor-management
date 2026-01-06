namespace Rascor.Modules.Proposals.Domain.Entities;

public enum ProposalStatus
{
    Draft = 0,
    Submitted = 1,
    UnderReview = 2,
    Approved = 3,
    Rejected = 4,
    Won = 5,
    Lost = 6,
    Expired = 7,
    Cancelled = 8
}
