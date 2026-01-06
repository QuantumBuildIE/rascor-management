using Rascor.Modules.Proposals.Domain.Entities;

namespace Rascor.Tests.Common.Builders;

/// <summary>
/// Fluent builder for creating Proposal entities in tests.
/// </summary>
public class ProposalBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _tenantId = TestTenant.TestTenantConstants.TenantId;
    private string _proposalNumber = $"PRO-TEST-{Guid.NewGuid().ToString()[..8]}";
    private int _version = 1;
    private Guid? _parentProposalId = null;
    private Guid _companyId;
    private string _companyName = "Test Company";
    private string _projectName = "Test Project";
    private ProposalStatus _status = ProposalStatus.Draft;
    private decimal _vatRate = 23.0m;
    private DateTime _proposalDate = DateTime.UtcNow;
    private DateTime? _validUntilDate = null;
    private string? _notes = null;
    private DateTime? _wonDate = null;
    private DateTime? _lostDate = null;
    private string? _wonLostReason = null;
    private readonly List<ProposalSection> _sections = new();

    public ProposalBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ProposalBuilder WithTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public ProposalBuilder WithProposalNumber(string proposalNumber)
    {
        _proposalNumber = proposalNumber;
        return this;
    }

    public ProposalBuilder AsVersion(int version, Guid? parentProposalId = null)
    {
        _version = version;
        _parentProposalId = parentProposalId;
        return this;
    }

    public ProposalBuilder ForCompany(Guid companyId, string companyName = "Test Company")
    {
        _companyId = companyId;
        _companyName = companyName;
        return this;
    }

    public ProposalBuilder WithProjectName(string projectName)
    {
        _projectName = projectName;
        return this;
    }

    public ProposalBuilder WithVatRate(decimal vatRate)
    {
        _vatRate = vatRate;
        return this;
    }

    public ProposalBuilder WithProposalDate(DateTime proposalDate)
    {
        _proposalDate = proposalDate;
        return this;
    }

    public ProposalBuilder ValidUntil(DateTime validUntilDate)
    {
        _validUntilDate = validUntilDate;
        return this;
    }

    public ProposalBuilder WithNotes(string notes)
    {
        _notes = notes;
        return this;
    }

    public ProposalBuilder AsDraft()
    {
        _status = ProposalStatus.Draft;
        return this;
    }

    public ProposalBuilder AsSubmitted()
    {
        _status = ProposalStatus.Submitted;
        return this;
    }

    public ProposalBuilder AsApproved()
    {
        _status = ProposalStatus.Approved;
        return this;
    }

    public ProposalBuilder AsRejected()
    {
        _status = ProposalStatus.Rejected;
        return this;
    }

    public ProposalBuilder AsWon(DateTime? wonDate = null)
    {
        _status = ProposalStatus.Won;
        _wonDate = wonDate ?? DateTime.UtcNow;
        return this;
    }

    public ProposalBuilder AsLost(string reason, DateTime? lostDate = null)
    {
        _status = ProposalStatus.Lost;
        _lostDate = lostDate ?? DateTime.UtcNow;
        _wonLostReason = reason;
        return this;
    }

    public ProposalBuilder AsCancelled()
    {
        _status = ProposalStatus.Cancelled;
        return this;
    }

    public ProposalBuilder WithSection(string name, string? description = null, Action<ProposalSectionBuilder>? configure = null)
    {
        var sectionBuilder = new ProposalSectionBuilder()
            .WithProposalId(_id)
            .WithTenantId(_tenantId)
            .WithSectionName(name)
            .WithDescription(description)
            .WithSortOrder(_sections.Count + 1);

        configure?.Invoke(sectionBuilder);

        _sections.Add(sectionBuilder.Build());
        return this;
    }

    public Proposal Build()
    {
        var proposal = new Proposal
        {
            Id = _id,
            TenantId = _tenantId,
            ProposalNumber = _proposalNumber,
            Version = _version,
            ParentProposalId = _parentProposalId,
            CompanyId = _companyId,
            CompanyName = _companyName,
            ProjectName = _projectName,
            Status = _status,
            VatRate = _vatRate,
            ProposalDate = _proposalDate,
            ValidUntilDate = _validUntilDate ?? DateTime.UtcNow.AddDays(30),
            Notes = _notes,
            WonDate = _wonDate,
            LostDate = _lostDate,
            WonLostReason = _wonLostReason,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-builder"
        };

        foreach (var section in _sections)
        {
            proposal.Sections.Add(section);
        }

        // Calculate totals
        proposal.Subtotal = proposal.Sections.SelectMany(s => s.LineItems).Sum(li => li.LineTotal);
        proposal.VatAmount = proposal.Subtotal * (proposal.VatRate / 100);
        proposal.GrandTotal = proposal.Subtotal + proposal.VatAmount;
        proposal.TotalCost = proposal.Sections.SelectMany(s => s.LineItems).Sum(li => li.LineCost);
        if (proposal.Subtotal > 0)
        {
            proposal.TotalMargin = proposal.Subtotal - proposal.TotalCost;
            proposal.MarginPercent = (proposal.TotalMargin / proposal.Subtotal) * 100;
        }

        return proposal;
    }

    /// <summary>
    /// Creates a draft proposal.
    /// </summary>
    public static Proposal CreateDraft(Guid companyId, string companyName, string projectName = "Test Project", Guid? id = null)
    {
        return new ProposalBuilder()
            .WithId(id ?? Guid.NewGuid())
            .ForCompany(companyId, companyName)
            .WithProjectName(projectName)
            .AsDraft()
            .Build();
    }

    /// <summary>
    /// Creates an approved proposal.
    /// </summary>
    public static Proposal CreateApproved(Guid companyId, string companyName, string projectName = "Test Project", Guid? id = null)
    {
        return new ProposalBuilder()
            .WithId(id ?? Guid.NewGuid())
            .ForCompany(companyId, companyName)
            .WithProjectName(projectName)
            .AsApproved()
            .Build();
    }

    /// <summary>
    /// Creates a won proposal.
    /// </summary>
    public static Proposal CreateWon(Guid companyId, string companyName, string projectName = "Test Project", Guid? id = null)
    {
        return new ProposalBuilder()
            .WithId(id ?? Guid.NewGuid())
            .ForCompany(companyId, companyName)
            .WithProjectName(projectName)
            .AsWon()
            .Build();
    }

    /// <summary>
    /// Creates a lost proposal with reason.
    /// </summary>
    public static Proposal CreateLost(Guid companyId, string companyName, string lostReason = "Price too high", string projectName = "Test Project", Guid? id = null)
    {
        return new ProposalBuilder()
            .WithId(id ?? Guid.NewGuid())
            .ForCompany(companyId, companyName)
            .WithProjectName(projectName)
            .AsLost(lostReason)
            .Build();
    }
}

/// <summary>
/// Fluent builder for ProposalSection entities.
/// </summary>
public class ProposalSectionBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _tenantId = TestTenant.TestTenantConstants.TenantId;
    private Guid _proposalId;
    private string _sectionName = "Test Section";
    private string? _description = null;
    private int _sortOrder = 1;
    private Guid? _sourceKitId = null;
    private readonly List<ProposalLineItem> _lineItems = new();

    public ProposalSectionBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ProposalSectionBuilder WithTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public ProposalSectionBuilder WithProposalId(Guid proposalId)
    {
        _proposalId = proposalId;
        return this;
    }

    public ProposalSectionBuilder WithSectionName(string sectionName)
    {
        _sectionName = sectionName;
        return this;
    }

    public ProposalSectionBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public ProposalSectionBuilder WithSortOrder(int sortOrder)
    {
        _sortOrder = sortOrder;
        return this;
    }

    public ProposalSectionBuilder FromSourceKit(Guid sourceKitId)
    {
        _sourceKitId = sourceKitId;
        return this;
    }

    public ProposalSectionBuilder WithLineItem(Guid? productId, string description, decimal quantity, decimal unitPrice, decimal unitCost)
    {
        var lineTotal = quantity * unitPrice;
        var lineCost = quantity * unitCost;
        var lineMargin = lineTotal - lineCost;
        var marginPercent = lineTotal > 0 ? (lineMargin / lineTotal) * 100 : 0;

        var lineItem = new ProposalLineItem
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            ProposalSectionId = _id,
            ProductId = productId,
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice,
            UnitCost = unitCost,
            LineTotal = lineTotal,
            LineCost = lineCost,
            LineMargin = lineMargin,
            MarginPercent = marginPercent,
            SortOrder = _lineItems.Count + 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-builder"
        };

        _lineItems.Add(lineItem);
        return this;
    }

    public ProposalSection Build()
    {
        var section = new ProposalSection
        {
            Id = _id,
            TenantId = _tenantId,
            ProposalId = _proposalId,
            SectionName = _sectionName,
            Description = _description,
            SortOrder = _sortOrder,
            SourceKitId = _sourceKitId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-builder"
        };

        foreach (var lineItem in _lineItems)
        {
            section.LineItems.Add(lineItem);
        }

        // Calculate section totals
        section.SectionTotal = section.LineItems.Sum(li => li.LineTotal);
        section.SectionCost = section.LineItems.Sum(li => li.LineCost);
        section.SectionMargin = section.SectionTotal - section.SectionCost;

        return section;
    }
}
