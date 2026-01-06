using Microsoft.EntityFrameworkCore;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.Proposals.Application.Common.Interfaces;
using Rascor.Modules.Proposals.Application.DTOs;
using Rascor.Modules.Proposals.Domain.Entities;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;

namespace Rascor.Modules.Proposals.Application.Services;

public class ProposalService : IProposalService
{
    private readonly IProposalsDbContext _context;
    private readonly IStockManagementDbContext _stockContext;
    private readonly IProposalCalculationService _calculationService;
    private readonly ICurrentUserService _currentUserService;

    public ProposalService(
        IProposalsDbContext context,
        IStockManagementDbContext stockContext,
        IProposalCalculationService calculationService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _stockContext = stockContext;
        _calculationService = calculationService;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<ProposalListDto>> GetProposalsAsync(
        string? search, string? status, Guid? companyId,
        int pageNumber, int pageSize, string? sortColumn, string? sortDirection)
    {
        var query = _context.Proposals.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p =>
                p.ProposalNumber.ToLower().Contains(searchLower) ||
                p.ProjectName.ToLower().Contains(searchLower) ||
                p.CompanyName.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ProposalStatus>(status, true, out var statusEnum))
        {
            query = query.Where(p => p.Status == statusEnum);
        }

        if (companyId.HasValue)
        {
            query = query.Where(p => p.CompanyId == companyId.Value);
        }

        // Apply sorting
        query = ApplySorting(query, sortColumn, sortDirection);

        // Get total count
        var totalCount = await query.CountAsync();

        // Don't include costing in list view - only in detail view
        var hasViewCostings = false;

        // Apply pagination and project to DTO
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProposalListDto
            {
                Id = p.Id,
                ProposalNumber = p.ProposalNumber,
                Version = p.Version,
                ProjectName = p.ProjectName,
                CompanyName = p.CompanyName,
                ProposalDate = p.ProposalDate,
                ValidUntilDate = p.ValidUntilDate,
                Status = p.Status.ToString(),
                GrandTotal = p.GrandTotal,
                Currency = p.Currency,
                MarginPercent = hasViewCostings ? p.MarginPercent : null,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return new PaginatedList<ProposalListDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<ProposalSummaryDto> GetSummaryAsync()
    {
        // Get all proposals for statistics
        var allProposals = await _context.Proposals.ToListAsync();

        // Total count
        var totalCount = allProposals.Count;

        // Pipeline value - active proposals (Draft, Submitted, UnderReview, Approved)
        var pipelineStatuses = new[]
        {
            ProposalStatus.Draft,
            ProposalStatus.Submitted,
            ProposalStatus.UnderReview,
            ProposalStatus.Approved
        };
        var pipelineValue = allProposals
            .Where(p => pipelineStatuses.Contains(p.Status))
            .Sum(p => p.GrandTotal);

        // Won this month
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var wonThisMonth = allProposals
            .Where(p => p.Status == ProposalStatus.Won && p.WonDate >= startOfMonth)
            .ToList();
        var wonThisMonthCount = wonThisMonth.Count;
        var wonThisMonthValue = wonThisMonth.Sum(p => p.GrandTotal);

        // Conversion rate (Won / (Won + Lost))
        var wonCount = allProposals.Count(p => p.Status == ProposalStatus.Won);
        var lostCount = allProposals.Count(p => p.Status == ProposalStatus.Lost);
        var conversionRate = (wonCount + lostCount) > 0
            ? Math.Round((decimal)wonCount / (wonCount + lostCount) * 100, 1)
            : 0;

        return new ProposalSummaryDto
        {
            TotalCount = totalCount,
            PipelineValue = pipelineValue,
            WonThisMonthCount = wonThisMonthCount,
            WonThisMonthValue = wonThisMonthValue,
            ConversionRate = conversionRate
        };
    }

    public async Task<ProposalDto?> GetProposalByIdAsync(Guid id, bool includeCosting = false)
    {
        var proposal = await _context.Proposals
            .Include(p => p.Sections)
                .ThenInclude(s => s.LineItems)
            .Include(p => p.Contacts)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (proposal == null)
            return null;

        // Permission check will be done at the controller/authorization level
        return MapToProposalDto(proposal, includeCosting);
    }

    public async Task<ProposalDto> CreateProposalAsync(CreateProposalDto dto)
    {
        // Validation
        if (dto.ValidUntilDate < dto.ProposalDate)
            throw new InvalidOperationException("Valid Until Date must be on or after Proposal Date");

        // Generate proposal number
        var proposalNumber = await GenerateProposalNumberAsync();

        // Get company name from Core module (simplified - in production, you'd call a company service)
        var companyName = await GetCompanyNameAsync(dto.CompanyId);
        var primaryContactName = dto.PrimaryContactId.HasValue
            ? await GetContactNameAsync(dto.PrimaryContactId.Value)
            : null;

        var proposal = new Proposal
        {
            ProposalNumber = proposalNumber,
            Version = 1,
            CompanyId = dto.CompanyId,
            CompanyName = companyName,
            PrimaryContactId = dto.PrimaryContactId,
            PrimaryContactName = primaryContactName,
            ProjectName = dto.ProjectName,
            ProjectAddress = dto.ProjectAddress,
            ProjectDescription = dto.ProjectDescription,
            ProposalDate = EnsureUtc(dto.ProposalDate),
            ValidUntilDate = dto.ValidUntilDate.HasValue ? EnsureUtc(dto.ValidUntilDate.Value) : null,
            Currency = dto.Currency,
            VatRate = dto.VatRate,
            DiscountPercent = dto.DiscountPercent,
            PaymentTerms = dto.PaymentTerms,
            TermsAndConditions = dto.TermsAndConditions,
            Notes = dto.Notes,
            Status = ProposalStatus.Draft
        };

        _context.Proposals.Add(proposal);
        await _context.SaveChangesAsync();

        // Recalculate totals using calculation service
        await _calculationService.CalculateProposalTotalsAsync(proposal.Id);

        return (await GetProposalByIdAsync(proposal.Id, true))!;
    }

    public async Task<ProposalDto> UpdateProposalAsync(Guid id, UpdateProposalDto dto)
    {
        var proposal = await _context.Proposals.FindAsync(id);
        if (proposal == null)
            throw new InvalidOperationException("Proposal not found");

        // Business rule: Can only edit proposals in Draft status
        if (proposal.Status != ProposalStatus.Draft)
        {
            throw new InvalidOperationException($"Cannot edit proposal in {proposal.Status} status. Only Draft proposals can be edited.");
        }

        // Validation
        if (dto.ValidUntilDate < dto.ProposalDate)
            throw new InvalidOperationException("Valid Until Date must be on or after Proposal Date");

        // Get updated company/contact names
        var companyName = await GetCompanyNameAsync(dto.CompanyId);
        var primaryContactName = dto.PrimaryContactId.HasValue
            ? await GetContactNameAsync(dto.PrimaryContactId.Value)
            : null;

        var discountChanged = proposal.DiscountPercent != dto.DiscountPercent;
        var vatRateChanged = proposal.VatRate != dto.VatRate;

        proposal.CompanyId = dto.CompanyId;
        proposal.CompanyName = companyName;
        proposal.PrimaryContactId = dto.PrimaryContactId;
        proposal.PrimaryContactName = primaryContactName;
        proposal.ProjectName = dto.ProjectName;
        proposal.ProjectAddress = dto.ProjectAddress;
        proposal.ProjectDescription = dto.ProjectDescription;
        proposal.ProposalDate = EnsureUtc(dto.ProposalDate);
        proposal.ValidUntilDate = dto.ValidUntilDate.HasValue ? EnsureUtc(dto.ValidUntilDate.Value) : null;
        proposal.Currency = dto.Currency;
        proposal.VatRate = dto.VatRate;
        proposal.DiscountPercent = dto.DiscountPercent;
        proposal.PaymentTerms = dto.PaymentTerms;
        proposal.TermsAndConditions = dto.TermsAndConditions;
        proposal.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        // Recalculate totals if discount or VAT rate changed
        if (discountChanged || vatRateChanged)
        {
            await _calculationService.CalculateProposalTotalsAsync(id);
        }

        return (await GetProposalByIdAsync(id, true))!;
    }

    public async Task DeleteProposalAsync(Guid id)
    {
        var proposal = await _context.Proposals.FindAsync(id);
        if (proposal == null)
            throw new InvalidOperationException("Proposal not found");

        // Only allow deletion if Draft
        if (proposal.Status != ProposalStatus.Draft)
        {
            throw new InvalidOperationException($"Cannot delete proposal in {proposal.Status} status");
        }

        proposal.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task<string> GenerateProposalNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"PROP-{year}-";

        var lastProposal = await _context.Proposals
            .Where(p => p.ProposalNumber.StartsWith(prefix))
            .OrderByDescending(p => p.ProposalNumber)
            .FirstOrDefaultAsync();

        int nextSequence = 1;
        if (lastProposal != null)
        {
            var lastSequence = lastProposal.ProposalNumber.Replace(prefix, "");
            if (int.TryParse(lastSequence, out var parsed))
            {
                nextSequence = parsed + 1;
            }
        }

        return $"{prefix}{nextSequence:D4}";
    }

    #region Sections

    public async Task<ProposalSectionDto> AddSectionAsync(CreateProposalSectionDto dto)
    {
        var proposal = await _context.Proposals.FindAsync(dto.ProposalId);
        if (proposal == null)
            throw new InvalidOperationException("Proposal not found");

        // Business rule: Cannot add sections if Won, Lost, or Cancelled
        if (proposal.Status == ProposalStatus.Won ||
            proposal.Status == ProposalStatus.Lost ||
            proposal.Status == ProposalStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot add sections to proposal in {proposal.Status} status");
        }

        var section = new ProposalSection
        {
            ProposalId = dto.ProposalId,
            SourceKitId = dto.SourceKitId,
            SectionName = dto.SectionName,
            Description = dto.Description,
            SortOrder = dto.SortOrder
        };

        // If source kit provided, expand kit into line items
        if (dto.SourceKitId.HasValue)
        {
            var kit = await _stockContext.ProductKits
                .Include(k => k.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(k => k.Id == dto.SourceKitId.Value);

            if (kit != null)
            {
                // Use kit name/description if not provided
                if (string.IsNullOrEmpty(section.SectionName))
                    section.SectionName = kit.KitName;
                if (string.IsNullOrEmpty(section.Description))
                    section.Description = kit.Description;

                // Add section first to get ID
                _context.ProposalSections.Add(section);
                await _context.SaveChangesAsync();

                // Create line items from kit items
                foreach (var kitItem in kit.Items.OrderBy(i => i.SortOrder))
                {
                    var lineItem = new ProposalLineItem
                    {
                        ProposalSectionId = section.Id,
                        ProductId = kitItem.ProductId,
                        ProductCode = kitItem.Product.ProductCode,
                        Description = kitItem.Product.ProductName,
                        Quantity = kitItem.DefaultQuantity,
                        Unit = kitItem.Product.UnitType ?? "Each",
                        UnitCost = kitItem.Product.CostPrice ?? kitItem.Product.BaseRate,
                        UnitPrice = kitItem.Product.SellPrice ?? kitItem.Product.BaseRate,
                        SortOrder = kitItem.SortOrder,
                        Notes = kitItem.Notes
                    };

                    // Calculate line totals
                    _calculationService.CalculateLineItem(lineItem);
                    _context.ProposalLineItems.Add(lineItem);
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                _context.ProposalSections.Add(section);
                await _context.SaveChangesAsync();
            }
        }
        else
        {
            _context.ProposalSections.Add(section);
            await _context.SaveChangesAsync();
        }

        // Calculate section and proposal totals
        await _calculationService.CalculateSectionTotalsAsync(section.Id);
        await _calculationService.CalculateProposalTotalsAsync(dto.ProposalId);

        // Return with line items
        var sectionWithItems = await _context.ProposalSections
            .Include(s => s.LineItems)
            .FirstOrDefaultAsync(s => s.Id == section.Id);

        return MapToProposalSectionDto(sectionWithItems!, true);
    }

    public async Task<ProposalSectionDto> UpdateSectionAsync(Guid id, UpdateProposalSectionDto dto)
    {
        var section = await _context.ProposalSections
            .Include(s => s.LineItems)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (section == null)
            throw new InvalidOperationException("Section not found");

        section.SectionName = dto.SectionName;
        section.Description = dto.Description;
        section.SortOrder = dto.SortOrder;

        await _context.SaveChangesAsync();

        return MapToProposalSectionDto(section, true);
    }

    public async Task DeleteSectionAsync(Guid id)
    {
        var section = await _context.ProposalSections
            .Include(s => s.LineItems)
            .Include(s => s.Proposal)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (section == null)
            throw new InvalidOperationException("Section not found");

        // Business rule: Cannot delete sections if Won, Lost, or Cancelled
        if (section.Proposal.Status == ProposalStatus.Won ||
            section.Proposal.Status == ProposalStatus.Lost ||
            section.Proposal.Status == ProposalStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot delete sections from proposal in {section.Proposal.Status} status");
        }

        var proposalId = section.ProposalId;

        // Remove line items
        _context.ProposalLineItems.RemoveRange(section.LineItems);

        // Remove section
        _context.ProposalSections.Remove(section);

        await _context.SaveChangesAsync();

        // Recalculate proposal totals
        await _calculationService.CalculateProposalTotalsAsync(proposalId);
    }

    #endregion

    #region Line Items

    public async Task<ProposalLineItemDto> AddLineItemAsync(CreateProposalLineItemDto dto)
    {
        var section = await _context.ProposalSections
            .Include(s => s.Proposal)
            .FirstOrDefaultAsync(s => s.Id == dto.ProposalSectionId);

        if (section == null)
            throw new InvalidOperationException("Section not found");

        // Business rule: Cannot add line items if Won, Lost, or Cancelled
        if (section.Proposal.Status == ProposalStatus.Won ||
            section.Proposal.Status == ProposalStatus.Lost ||
            section.Proposal.Status == ProposalStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot add line items to proposal in {section.Proposal.Status} status");
        }

        // Validation
        if (dto.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than 0");
        if (dto.UnitPrice < 0)
            throw new InvalidOperationException("Unit Price cannot be negative");

        var lineItem = new ProposalLineItem
        {
            ProposalSectionId = dto.ProposalSectionId,
            ProductId = dto.ProductId,
            Description = dto.Description,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            UnitCost = dto.UnitCost,
            UnitPrice = dto.UnitPrice,
            SortOrder = dto.SortOrder,
            Notes = dto.Notes
        };

        // Auto-populate from Product if ProductId provided
        if (dto.ProductId.HasValue)
        {
            var product = await _stockContext.Products.FindAsync(dto.ProductId.Value);
            if (product != null)
            {
                lineItem.ProductCode = product.ProductCode;

                // Use provided values or fall back to product values
                if (string.IsNullOrEmpty(lineItem.Description))
                    lineItem.Description = product.ProductName;
                if (string.IsNullOrEmpty(lineItem.Unit))
                    lineItem.Unit = product.UnitType ?? "Each";
                if (lineItem.UnitCost == 0)
                    lineItem.UnitCost = product.CostPrice ?? product.BaseRate;
                if (lineItem.UnitPrice == 0)
                    lineItem.UnitPrice = product.SellPrice ?? product.BaseRate;
            }
        }

        // Calculate line totals using calculation service
        _calculationService.CalculateLineItem(lineItem);

        _context.ProposalLineItems.Add(lineItem);
        await _context.SaveChangesAsync();

        // Recalculate section and proposal totals
        await _calculationService.CalculateSectionTotalsAsync(section.Id);
        await _calculationService.CalculateProposalTotalsAsync(section.ProposalId);

        return MapToProposalLineItemDto(lineItem);
    }

    public async Task<ProposalLineItemDto> UpdateLineItemAsync(Guid id, UpdateProposalLineItemDto dto)
    {
        var lineItem = await _context.ProposalLineItems
            .Include(li => li.Section)
                .ThenInclude(s => s.Proposal)
            .FirstOrDefaultAsync(li => li.Id == id);

        if (lineItem == null)
            throw new InvalidOperationException("Line item not found");

        // Business rule: Cannot edit line items if Won, Lost, or Cancelled
        if (lineItem.Section.Proposal.Status == ProposalStatus.Won ||
            lineItem.Section.Proposal.Status == ProposalStatus.Lost ||
            lineItem.Section.Proposal.Status == ProposalStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot edit line items in proposal with {lineItem.Section.Proposal.Status} status");
        }

        // Validation
        if (dto.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than 0");
        if (dto.UnitPrice < 0)
            throw new InvalidOperationException("Unit Price cannot be negative");

        lineItem.ProductId = dto.ProductId;
        lineItem.Description = dto.Description;
        lineItem.Quantity = dto.Quantity;
        lineItem.Unit = dto.Unit;
        lineItem.UnitCost = dto.UnitCost;
        lineItem.UnitPrice = dto.UnitPrice;
        lineItem.SortOrder = dto.SortOrder;
        lineItem.Notes = dto.Notes;

        // Auto-populate from Product if ProductId provided and changed
        if (dto.ProductId.HasValue)
        {
            var product = await _stockContext.Products.FindAsync(dto.ProductId.Value);
            if (product != null)
            {
                lineItem.ProductCode = product.ProductCode;

                // Update values from product if they're at defaults
                if (string.IsNullOrEmpty(lineItem.Unit))
                    lineItem.Unit = product.UnitType ?? "Each";
            }
        }
        else
        {
            lineItem.ProductCode = null;
        }

        // Recalculate line totals using calculation service
        _calculationService.CalculateLineItem(lineItem);

        await _context.SaveChangesAsync();

        // Recalculate section and proposal totals
        await _calculationService.CalculateSectionTotalsAsync(lineItem.Section.Id);
        await _calculationService.CalculateProposalTotalsAsync(lineItem.Section.ProposalId);

        return MapToProposalLineItemDto(lineItem);
    }

    public async Task DeleteLineItemAsync(Guid id)
    {
        var lineItem = await _context.ProposalLineItems
            .Include(li => li.Section)
                .ThenInclude(s => s.Proposal)
            .FirstOrDefaultAsync(li => li.Id == id);

        if (lineItem == null)
            throw new InvalidOperationException("Line item not found");

        // Business rule: Cannot delete line items if Won, Lost, or Cancelled
        if (lineItem.Section.Proposal.Status == ProposalStatus.Won ||
            lineItem.Section.Proposal.Status == ProposalStatus.Lost ||
            lineItem.Section.Proposal.Status == ProposalStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot delete line items from proposal in {lineItem.Section.Proposal.Status} status");
        }

        var sectionId = lineItem.Section.Id;
        var proposalId = lineItem.Section.ProposalId;

        _context.ProposalLineItems.Remove(lineItem);
        await _context.SaveChangesAsync();

        // Recalculate section and proposal totals
        await _calculationService.CalculateSectionTotalsAsync(sectionId);
        await _calculationService.CalculateProposalTotalsAsync(proposalId);
    }

    #endregion

    #region Contacts

    public async Task<ProposalContactDto> AddContactAsync(CreateProposalContactDto dto)
    {
        var proposal = await _context.Proposals.FindAsync(dto.ProposalId);
        if (proposal == null)
            throw new InvalidOperationException("Proposal not found");

        // If setting as primary, unset other primary contacts
        if (dto.IsPrimary)
        {
            var existingPrimary = await _context.ProposalContacts
                .Where(c => c.ProposalId == dto.ProposalId && c.IsPrimary)
                .ToListAsync();

            foreach (var contact in existingPrimary)
            {
                contact.IsPrimary = false;
            }
        }

        var proposalContact = new ProposalContact
        {
            ProposalId = dto.ProposalId,
            ContactId = dto.ContactId,
            ContactName = dto.ContactName,
            Email = dto.Email,
            Phone = dto.Phone,
            Role = dto.Role,
            IsPrimary = dto.IsPrimary
        };

        _context.ProposalContacts.Add(proposalContact);
        await _context.SaveChangesAsync();

        return MapToProposalContactDto(proposalContact);
    }

    public async Task<ProposalContactDto> UpdateContactAsync(Guid id, UpdateProposalContactDto dto)
    {
        var contact = await _context.ProposalContacts.FindAsync(id);
        if (contact == null)
            throw new InvalidOperationException("Contact not found");

        // If setting as primary, unset other primary contacts
        if (dto.IsPrimary && !contact.IsPrimary)
        {
            var existingPrimary = await _context.ProposalContacts
                .Where(c => c.ProposalId == contact.ProposalId && c.IsPrimary && c.Id != id)
                .ToListAsync();

            foreach (var existingContact in existingPrimary)
            {
                existingContact.IsPrimary = false;
            }
        }

        contact.ContactId = dto.ContactId;
        contact.ContactName = dto.ContactName;
        contact.Email = dto.Email;
        contact.Phone = dto.Phone;
        contact.Role = dto.Role;
        contact.IsPrimary = dto.IsPrimary;

        await _context.SaveChangesAsync();

        return MapToProposalContactDto(contact);
    }

    public async Task DeleteContactAsync(Guid id)
    {
        var contact = await _context.ProposalContacts.FindAsync(id);
        if (contact == null)
            throw new InvalidOperationException("Contact not found");

        _context.ProposalContacts.Remove(contact);
        await _context.SaveChangesAsync();
    }

    #endregion


    #region Helper Methods

    private IQueryable<Proposal> ApplySorting(IQueryable<Proposal> query, string? sortColumn, string? sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        return sortColumn?.ToLower() switch
        {
            "proposalnumber" => isDescending ? query.OrderByDescending(p => p.ProposalNumber) : query.OrderBy(p => p.ProposalNumber),
            "projectname" => isDescending ? query.OrderByDescending(p => p.ProjectName) : query.OrderBy(p => p.ProjectName),
            "companyname" => isDescending ? query.OrderByDescending(p => p.CompanyName) : query.OrderBy(p => p.CompanyName),
            "proposaldate" => isDescending ? query.OrderByDescending(p => p.ProposalDate) : query.OrderBy(p => p.ProposalDate),
            "status" => isDescending ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
            "grandtotal" => isDescending ? query.OrderByDescending(p => p.GrandTotal) : query.OrderBy(p => p.GrandTotal),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };
    }

    private ProposalDto MapToProposalDto(Proposal proposal, bool includeCosting)
    {
        return new ProposalDto
        {
            Id = proposal.Id,
            ProposalNumber = proposal.ProposalNumber,
            Version = proposal.Version,
            ParentProposalId = proposal.ParentProposalId,
            CompanyId = proposal.CompanyId,
            CompanyName = proposal.CompanyName,
            PrimaryContactId = proposal.PrimaryContactId,
            PrimaryContactName = proposal.PrimaryContactName,
            ProjectName = proposal.ProjectName,
            ProjectAddress = proposal.ProjectAddress,
            ProjectDescription = proposal.ProjectDescription,
            ProposalDate = proposal.ProposalDate,
            ValidUntilDate = proposal.ValidUntilDate,
            SubmittedDate = proposal.SubmittedDate,
            ApprovedDate = proposal.ApprovedDate,
            ApprovedBy = proposal.ApprovedBy,
            WonDate = proposal.WonDate,
            LostDate = proposal.LostDate,
            Status = proposal.Status.ToString(),
            WonLostReason = proposal.WonLostReason,
            Currency = proposal.Currency,
            Subtotal = proposal.Subtotal,
            DiscountPercent = proposal.DiscountPercent,
            DiscountAmount = proposal.DiscountAmount,
            NetTotal = proposal.NetTotal,
            VatRate = proposal.VatRate,
            VatAmount = proposal.VatAmount,
            GrandTotal = proposal.GrandTotal,
            TotalCost = includeCosting ? proposal.TotalCost : null,
            TotalMargin = includeCosting ? proposal.TotalMargin : null,
            MarginPercent = includeCosting ? proposal.MarginPercent : null,
            PaymentTerms = proposal.PaymentTerms,
            TermsAndConditions = proposal.TermsAndConditions,
            Notes = proposal.Notes,
            DrawingFileName = proposal.DrawingFileName,
            DrawingUrl = proposal.DrawingUrl,
            Sections = proposal.Sections.OrderBy(s => s.SortOrder).Select(s => MapToProposalSectionDto(s, includeCosting)).ToList(),
            Contacts = proposal.Contacts.Select(MapToProposalContactDto).ToList(),
            CreatedAt = proposal.CreatedAt,
            CreatedBy = proposal.CreatedBy,
            UpdatedAt = proposal.UpdatedAt
        };
    }

    private ProposalSectionDto MapToProposalSectionDto(ProposalSection section, bool includeCosting)
    {
        return new ProposalSectionDto
        {
            Id = section.Id,
            ProposalId = section.ProposalId,
            SourceKitId = section.SourceKitId,
            SectionName = section.SectionName,
            Description = section.Description,
            SortOrder = section.SortOrder,
            SectionCost = includeCosting ? section.SectionCost : 0,
            SectionTotal = section.SectionTotal,
            SectionMargin = includeCosting ? section.SectionMargin : 0,
            LineItems = section.LineItems?.OrderBy(li => li.SortOrder).Select(MapToProposalLineItemDto).ToList() ?? new()
        };
    }

    private ProposalLineItemDto MapToProposalLineItemDto(ProposalLineItem lineItem)
    {
        return new ProposalLineItemDto
        {
            Id = lineItem.Id,
            ProposalSectionId = lineItem.ProposalSectionId,
            ProductId = lineItem.ProductId,
            ProductCode = lineItem.ProductCode,
            Description = lineItem.Description,
            Quantity = lineItem.Quantity,
            Unit = lineItem.Unit,
            UnitCost = lineItem.UnitCost,
            UnitPrice = lineItem.UnitPrice,
            LineTotal = lineItem.LineTotal,
            LineCost = lineItem.LineCost,
            LineMargin = lineItem.LineMargin,
            MarginPercent = lineItem.MarginPercent,
            SortOrder = lineItem.SortOrder,
            Notes = lineItem.Notes
        };
    }

    private ProposalContactDto MapToProposalContactDto(ProposalContact contact)
    {
        return new ProposalContactDto
        {
            Id = contact.Id,
            ProposalId = contact.ProposalId,
            ContactId = contact.ContactId,
            ContactName = contact.ContactName,
            Email = contact.Email,
            Phone = contact.Phone,
            Role = contact.Role,
            IsPrimary = contact.IsPrimary
        };
    }

    private async Task<string> GetCompanyNameAsync(Guid companyId)
    {
        // TODO: In production, call Core module's company service
        // For now, return placeholder
        return "Company Name"; // Replace with actual lookup
    }

    private async Task<string?> GetContactNameAsync(Guid contactId)
    {
        // TODO: In production, call Core module's contact service
        // For now, return placeholder
        return "Contact Name"; // Replace with actual lookup
    }

    /// <summary>
    /// Ensures a DateTime value has UTC kind for PostgreSQL compatibility.
    /// If the DateTime has Unspecified kind, it's treated as UTC.
    /// </summary>
    private static DateTime EnsureUtc(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };
    }

    #endregion
}
