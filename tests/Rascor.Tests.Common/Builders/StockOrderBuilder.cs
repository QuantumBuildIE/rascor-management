using Rascor.Modules.StockManagement.Domain.Entities;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Tests.Common.Builders;

/// <summary>
/// Fluent builder for creating StockOrder entities in tests.
/// </summary>
public class StockOrderBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _tenantId = TestTenant.TestTenantConstants.TenantId;
    private string _orderNumber = $"SO-TEST-{Guid.NewGuid().ToString()[..8]}";
    private Guid _siteId;
    private string _siteName = "Test Site";
    private Guid _sourceLocationId;
    private string _requestedBy = "test-user";
    private StockOrderStatus _status = StockOrderStatus.Draft;
    private DateTime _orderDate = DateTime.UtcNow;
    private DateTime? _requiredDate = DateTime.UtcNow.AddDays(3);
    private decimal _orderTotal = 0;
    private string? _approvedBy = null;
    private DateTime? _approvedDate = null;
    private DateTime? _collectedDate = null;
    private string? _notes = null;
    private Guid? _sourceProposalId = null;
    private string? _sourceProposalNumber = null;
    private readonly List<StockOrderLine> _lines = new();

    public StockOrderBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public StockOrderBuilder WithTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public StockOrderBuilder WithOrderNumber(string orderNumber)
    {
        _orderNumber = orderNumber;
        return this;
    }

    public StockOrderBuilder ForSite(Guid siteId, string siteName = "Test Site")
    {
        _siteId = siteId;
        _siteName = siteName;
        return this;
    }

    public StockOrderBuilder FromLocation(Guid sourceLocationId)
    {
        _sourceLocationId = sourceLocationId;
        return this;
    }

    public StockOrderBuilder RequestedBy(string requestedBy)
    {
        _requestedBy = requestedBy;
        return this;
    }

    public StockOrderBuilder WithOrderDate(DateTime orderDate)
    {
        _orderDate = orderDate;
        return this;
    }

    public StockOrderBuilder WithRequiredDate(DateTime requiredDate)
    {
        _requiredDate = requiredDate;
        return this;
    }

    public StockOrderBuilder WithNotes(string notes)
    {
        _notes = notes;
        return this;
    }

    public StockOrderBuilder FromProposal(Guid proposalId, string proposalNumber)
    {
        _sourceProposalId = proposalId;
        _sourceProposalNumber = proposalNumber;
        return this;
    }

    public StockOrderBuilder AsDraft()
    {
        _status = StockOrderStatus.Draft;
        return this;
    }

    public StockOrderBuilder AsPendingApproval()
    {
        _status = StockOrderStatus.PendingApproval;
        return this;
    }

    public StockOrderBuilder AsApproved(string approvedBy, DateTime? approvedDate = null)
    {
        _status = StockOrderStatus.Approved;
        _approvedBy = approvedBy;
        _approvedDate = approvedDate ?? DateTime.UtcNow;
        return this;
    }

    public StockOrderBuilder AsAwaitingPick(string approvedBy, DateTime? approvedDate = null)
    {
        _status = StockOrderStatus.AwaitingPick;
        _approvedBy = approvedBy;
        _approvedDate = approvedDate ?? DateTime.UtcNow.AddDays(-1);
        return this;
    }

    public StockOrderBuilder AsReadyForCollection(string approvedBy)
    {
        _status = StockOrderStatus.ReadyForCollection;
        _approvedBy = approvedBy;
        _approvedDate = _approvedDate ?? DateTime.UtcNow.AddDays(-1);
        return this;
    }

    public StockOrderBuilder AsCollected(string approvedBy, DateTime? collectedDate = null)
    {
        _status = StockOrderStatus.Collected;
        _approvedBy = approvedBy;
        _approvedDate = _approvedDate ?? DateTime.UtcNow.AddDays(-2);
        _collectedDate = collectedDate ?? DateTime.UtcNow;
        return this;
    }

    public StockOrderBuilder AsCancelled()
    {
        _status = StockOrderStatus.Cancelled;
        return this;
    }

    public StockOrderBuilder WithLine(Guid productId, int quantityRequested, decimal unitPrice, int quantityIssued = 0)
    {
        _lines.Add(new StockOrderLine
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            StockOrderId = _id,
            ProductId = productId,
            QuantityRequested = quantityRequested,
            QuantityIssued = quantityIssued,
            UnitPrice = unitPrice,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-builder"
        });
        return this;
    }

    public StockOrder Build()
    {
        var order = new StockOrder
        {
            Id = _id,
            TenantId = _tenantId,
            OrderNumber = _orderNumber,
            SiteId = _siteId,
            SiteName = _siteName,
            SourceLocationId = _sourceLocationId,
            RequestedBy = _requestedBy,
            Status = _status,
            OrderDate = _orderDate,
            RequiredDate = _requiredDate,
            OrderTotal = _orderTotal,
            ApprovedBy = _approvedBy,
            ApprovedDate = _approvedDate,
            CollectedDate = _collectedDate,
            Notes = _notes,
            SourceProposalId = _sourceProposalId,
            SourceProposalNumber = _sourceProposalNumber,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-builder"
        };

        foreach (var line in _lines)
        {
            line.StockOrderId = _id;
            order.Lines.Add(line);
        }

        // Calculate order total from lines
        order.OrderTotal = order.Lines.Sum(l => l.QuantityRequested * l.UnitPrice);

        return order;
    }

    /// <summary>
    /// Creates a draft stock order with default line items.
    /// </summary>
    public static StockOrder CreateDraft(Guid siteId, string siteName, Guid sourceLocationId, string requestedBy, Guid? id = null)
    {
        return new StockOrderBuilder()
            .WithId(id ?? Guid.NewGuid())
            .ForSite(siteId, siteName)
            .FromLocation(sourceLocationId)
            .RequestedBy(requestedBy)
            .AsDraft()
            .Build();
    }

    /// <summary>
    /// Creates a pending approval stock order.
    /// </summary>
    public static StockOrder CreatePendingApproval(Guid siteId, string siteName, Guid sourceLocationId, string requestedBy, Guid? id = null)
    {
        return new StockOrderBuilder()
            .WithId(id ?? Guid.NewGuid())
            .ForSite(siteId, siteName)
            .FromLocation(sourceLocationId)
            .RequestedBy(requestedBy)
            .AsPendingApproval()
            .Build();
    }

    /// <summary>
    /// Creates an approved stock order.
    /// </summary>
    public static StockOrder CreateApproved(Guid siteId, string siteName, Guid sourceLocationId, string requestedBy, string approvedBy, Guid? id = null)
    {
        return new StockOrderBuilder()
            .WithId(id ?? Guid.NewGuid())
            .ForSite(siteId, siteName)
            .FromLocation(sourceLocationId)
            .RequestedBy(requestedBy)
            .AsApproved(approvedBy)
            .Build();
    }

    /// <summary>
    /// Creates a completed (collected) stock order.
    /// </summary>
    public static StockOrder CreateCompleted(Guid siteId, string siteName, Guid sourceLocationId, string requestedBy, string approvedBy, Guid? id = null)
    {
        return new StockOrderBuilder()
            .WithId(id ?? Guid.NewGuid())
            .ForSite(siteId, siteName)
            .FromLocation(sourceLocationId)
            .RequestedBy(requestedBy)
            .AsCollected(approvedBy)
            .Build();
    }
}
