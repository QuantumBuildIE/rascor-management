using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.Rams.Application.Common.Interfaces;
using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Application.Services;
using Rascor.Modules.Rams.Domain.Entities;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Infrastructure.Services;

/// <summary>
/// Service implementation for RAMS document operations
/// </summary>
public class RamsDocumentService : IRamsDocumentService
{
    private readonly IRamsDbContext _context;
    private readonly ICoreDbContext _coreContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<RamsDocumentService> _logger;

    public RamsDocumentService(
        IRamsDbContext context,
        ICoreDbContext coreContext,
        ICurrentUserService currentUserService,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<RamsDocumentService> logger)
    {
        _context = context;
        _coreContext = coreContext;
        _currentUserService = currentUserService;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task<PaginatedList<RamsDocumentListDto>> GetDocumentsAsync(
        string? search = null,
        RamsStatus? status = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortColumn = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.RamsDocuments
            .Include(d => d.RiskAssessments)
            .Include(d => d.MethodSteps)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(d =>
                d.ProjectName.ToLower().Contains(searchLower) ||
                d.ProjectReference.ToLower().Contains(searchLower) ||
                (d.ClientName != null && d.ClientName.ToLower().Contains(searchLower)));
        }

        // Apply status filter
        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        // Get site names for lookup
        var siteIds = await query.Where(d => d.SiteId.HasValue)
            .Select(d => d.SiteId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var siteNames = await _coreContext.Sites
            .Where(s => siteIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.SiteName, cancellationToken);

        // Apply sorting
        query = ApplySorting(query, sortColumn, sortDirection);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Get page of items
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new RamsDocumentListDto
            {
                Id = d.Id,
                ProjectName = d.ProjectName,
                ProjectReference = d.ProjectReference,
                ProjectType = d.ProjectType,
                ClientName = d.ClientName,
                Status = d.Status,
                ProposedStartDate = d.ProposedStartDate,
                RiskAssessmentCount = d.RiskAssessments.Count,
                MethodStepCount = d.MethodSteps.Count,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Populate site names
        foreach (var item in items)
        {
            // This is a workaround since we can't include SiteId in the anonymous type
            // We'll need to do a second query or modify the approach
        }

        return new PaginatedList<RamsDocumentListDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<RamsDocumentDto?> GetDocumentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _context.RamsDocuments
            .Include(d => d.RiskAssessments.OrderBy(r => r.SortOrder))
            .Include(d => d.MethodSteps.OrderBy(m => m.StepNumber))
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (document == null)
            return null;

        // Get site name if applicable
        string? siteName = null;
        if (document.SiteId.HasValue)
        {
            siteName = await _coreContext.Sites
                .Where(s => s.Id == document.SiteId.Value)
                .Select(s => s.SiteName)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // Get safety officer name if applicable
        string? safetyOfficerName = null;
        if (document.SafetyOfficerId.HasValue)
        {
            var employee = await _coreContext.Employees
                .Where(e => e.Id == document.SafetyOfficerId.Value)
                .Select(e => new { e.FirstName, e.LastName })
                .FirstOrDefaultAsync(cancellationToken);

            if (employee != null)
                safetyOfficerName = $"{employee.FirstName} {employee.LastName}";
        }

        return MapToDto(document, siteName, safetyOfficerName);
    }

    public async Task<RamsDocumentDto> CreateDocumentAsync(CreateRamsDocumentDto dto, CancellationToken cancellationToken = default)
    {
        // Check for duplicate project reference
        if (await ProjectReferenceExistsAsync(dto.ProjectReference, null, cancellationToken))
        {
            throw new InvalidOperationException("Project reference already exists");
        }

        var document = new RamsDocument
        {
            ProjectName = dto.ProjectName,
            ProjectReference = dto.ProjectReference,
            ProjectType = dto.ProjectType,
            ClientName = dto.ClientName,
            SiteAddress = dto.SiteAddress,
            AreaOfActivity = dto.AreaOfActivity,
            ProposedStartDate = dto.ProposedStartDate,
            ProposedEndDate = dto.ProposedEndDate,
            SafetyOfficerId = dto.SafetyOfficerId,
            MethodStatementBody = dto.MethodStatementBody,
            ProposalId = dto.ProposalId,
            SiteId = dto.SiteId,
            Status = RamsStatus.Draft
        };

        _context.RamsDocuments.Add(document);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created RAMS document {Id} with reference {Reference}",
            document.Id, document.ProjectReference);

        return await GetDocumentByIdAsync(document.Id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve created document");
    }

    public async Task<RamsDocumentDto> UpdateDocumentAsync(Guid id, UpdateRamsDocumentDto dto, CancellationToken cancellationToken = default)
    {
        var document = await _context.RamsDocuments
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (document == null)
            throw new InvalidOperationException("Document not found");

        // Can only edit Draft or Rejected documents
        if (document.Status != RamsStatus.Draft && document.Status != RamsStatus.Rejected)
            throw new InvalidOperationException("Can only edit documents in Draft or Rejected status");

        // Check for duplicate project reference (excluding current document)
        if (await ProjectReferenceExistsAsync(dto.ProjectReference, id, cancellationToken))
            throw new InvalidOperationException("Project reference already exists");

        document.ProjectName = dto.ProjectName;
        document.ProjectReference = dto.ProjectReference;
        document.ProjectType = dto.ProjectType;
        document.ClientName = dto.ClientName;
        document.SiteAddress = dto.SiteAddress;
        document.AreaOfActivity = dto.AreaOfActivity;
        document.ProposedStartDate = dto.ProposedStartDate;
        document.ProposedEndDate = dto.ProposedEndDate;
        document.SafetyOfficerId = dto.SafetyOfficerId;
        document.MethodStatementBody = dto.MethodStatementBody;
        document.SiteId = dto.SiteId;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated RAMS document {Id}", id);

        return await GetDocumentByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve updated document");
    }

    public async Task DeleteDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _context.RamsDocuments
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (document == null)
            throw new InvalidOperationException("Document not found");

        // Can only delete Draft documents
        if (document.Status != RamsStatus.Draft)
            throw new InvalidOperationException("Can only delete documents in Draft status");

        _context.RamsDocuments.Remove(document);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted RAMS document {Id}", id);
    }

    public async Task<RamsDocumentDto> SubmitForReviewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _context.RamsDocuments
            .Include(d => d.RiskAssessments)
            .Include(d => d.MethodSteps)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (document == null)
            throw new InvalidOperationException("Document not found");

        if (document.Status != RamsStatus.Draft && document.Status != RamsStatus.Rejected)
            throw new InvalidOperationException("Can only submit documents in Draft or Rejected status");

        // Validate document has required content
        if (!document.RiskAssessments.Any())
            throw new InvalidOperationException("Document must have at least one risk assessment");

        if (!document.MethodSteps.Any())
            throw new InvalidOperationException("Document must have at least one method step");

        document.Status = RamsStatus.PendingReview;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("RAMS document {Id} submitted for review", id);

        // Send notification (fire and forget - don't block the workflow)
        // Use a new scope to avoid disposed context issues
        var userId = _currentUserService.UserId ?? "unknown";
        var userName = await GetCurrentUserNameAsync(cancellationToken);
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<IRamsNotificationService>();
                await notificationService.SendSubmitNotificationAsync(id, userId, userName, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send submit notification for document {DocumentId}", id);
            }
        });

        return await GetDocumentByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve document");
    }

    public async Task<RamsDocumentDto> ApproveAsync(Guid id, ApprovalDto? dto, CancellationToken cancellationToken = default)
    {
        var document = await _context.RamsDocuments
            .Include(d => d.RiskAssessments)
            .Include(d => d.MethodSteps)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (document == null)
            throw new InvalidOperationException("Document not found");

        if (document.Status != RamsStatus.PendingReview)
            throw new InvalidOperationException("Can only approve documents in Pending Review status");

        document.Status = RamsStatus.Approved;
        document.DateApproved = DateTime.UtcNow;
        document.ApprovedById = Guid.TryParse(_currentUserService.UserId, out var userId) ? userId : null;
        document.ApprovalComments = dto?.Comments;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("RAMS document {Id} approved", id);

        // Send notification (fire and forget - don't block the workflow)
        // Use a new scope to avoid disposed context issues
        var userIdStr = _currentUserService.UserId ?? "unknown";
        var userName = await GetCurrentUserNameAsync(cancellationToken);
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<IRamsNotificationService>();
                await notificationService.SendApprovalNotificationAsync(id, userIdStr, userName, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send approval notification for document {DocumentId}", id);
            }
        });

        return await GetDocumentByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve document");
    }

    public async Task<RamsDocumentDto> RejectAsync(Guid id, ApprovalDto dto, CancellationToken cancellationToken = default)
    {
        var document = await _context.RamsDocuments
            .Include(d => d.RiskAssessments)
            .Include(d => d.MethodSteps)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (document == null)
            throw new InvalidOperationException("Document not found");

        if (document.Status != RamsStatus.PendingReview)
            throw new InvalidOperationException("Can only reject documents in Pending Review status");

        if (string.IsNullOrWhiteSpace(dto.Comments))
            throw new InvalidOperationException("Rejection comments are required");

        document.Status = RamsStatus.Rejected;
        document.ApprovalComments = dto.Comments;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("RAMS document {Id} rejected", id);

        // Send notification (fire and forget - don't block the workflow)
        // Use a new scope to avoid disposed context issues
        var userId = _currentUserService.UserId ?? "unknown";
        var userName = await GetCurrentUserNameAsync(cancellationToken);
        var rejectionComments = dto.Comments;
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<IRamsNotificationService>();
                await notificationService.SendRejectionNotificationAsync(id, userId, userName, rejectionComments, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send rejection notification for document {DocumentId}", id);
            }
        });

        return await GetDocumentByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve document");
    }

    public async Task<bool> ProjectReferenceExistsAsync(string projectReference, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.RamsDocuments.Where(d => d.ProjectReference == projectReference);

        if (excludeId.HasValue)
            query = query.Where(d => d.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    private static IQueryable<RamsDocument> ApplySorting(
        IQueryable<RamsDocument> query,
        string? sortColumn,
        string? sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        return sortColumn?.ToLower() switch
        {
            "projectname" => isDescending
                ? query.OrderByDescending(d => d.ProjectName)
                : query.OrderBy(d => d.ProjectName),
            "projectreference" => isDescending
                ? query.OrderByDescending(d => d.ProjectReference)
                : query.OrderBy(d => d.ProjectReference),
            "status" => isDescending
                ? query.OrderByDescending(d => d.Status)
                : query.OrderBy(d => d.Status),
            "proposedstartdate" => isDescending
                ? query.OrderByDescending(d => d.ProposedStartDate)
                : query.OrderBy(d => d.ProposedStartDate),
            "createdat" => isDescending
                ? query.OrderByDescending(d => d.CreatedAt)
                : query.OrderBy(d => d.CreatedAt),
            _ => query.OrderByDescending(d => d.CreatedAt)
        };
    }

    private static RamsDocumentDto MapToDto(RamsDocument document, string? siteName = null, string? safetyOfficerName = null)
    {
        return new RamsDocumentDto
        {
            Id = document.Id,
            ProjectName = document.ProjectName,
            ProjectReference = document.ProjectReference,
            ProjectType = document.ProjectType,
            ClientName = document.ClientName,
            SiteAddress = document.SiteAddress,
            AreaOfActivity = document.AreaOfActivity,
            ProposedStartDate = document.ProposedStartDate,
            ProposedEndDate = document.ProposedEndDate,
            SafetyOfficerId = document.SafetyOfficerId,
            SafetyOfficerName = safetyOfficerName,
            Status = document.Status,
            DateApproved = document.DateApproved,
            ApprovedById = document.ApprovedById,
            ApprovalComments = document.ApprovalComments,
            MethodStatementBody = document.MethodStatementBody,
            GeneratedPdfUrl = document.GeneratedPdfUrl,
            ProposalId = document.ProposalId,
            SiteId = document.SiteId,
            SiteName = siteName,
            RiskAssessmentCount = document.RiskAssessments.Count,
            MethodStepCount = document.MethodSteps.Count,
            CreatedAt = document.CreatedAt,
            ModifiedAt = document.UpdatedAt
        };
    }

    private async Task<string> GetCurrentUserNameAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return "Unknown User";

        var user = await _coreContext.Users
            .Where(u => u.Id == userGuid)
            .Select(u => new { u.FirstName, u.LastName })
            .FirstOrDefaultAsync(cancellationToken);

        return user != null
            ? $"{user.FirstName} {user.LastName}"
            : "Unknown User";
    }
}
