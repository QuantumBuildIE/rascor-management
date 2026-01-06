using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Core.Application.Models;
using Rascor.Modules.Proposals.Application.DTOs;
using Rascor.Modules.Proposals.Application.Services;

namespace Rascor.API.Controllers;

[ApiController]
[Route("api/proposals")]
[Authorize(Policy = "Proposals.View")]
public class ProposalsController : ControllerBase
{
    private readonly IProposalService _proposalService;
    private readonly IProposalWorkflowService _workflowService;
    private readonly IProposalCalculationService _calculationService;
    private readonly IProposalPdfService _pdfService;
    private readonly IProposalReportsService _reportsService;
    private readonly IProposalConversionService _conversionService;
    private readonly ILogger<ProposalsController> _logger;

    public ProposalsController(
        IProposalService proposalService,
        IProposalWorkflowService workflowService,
        IProposalCalculationService calculationService,
        IProposalPdfService pdfService,
        IProposalReportsService reportsService,
        IProposalConversionService conversionService,
        ILogger<ProposalsController> logger)
    {
        _proposalService = proposalService;
        _workflowService = workflowService;
        _calculationService = calculationService;
        _pdfService = pdfService;
        _reportsService = reportsService;
        _conversionService = conversionService;
        _logger = logger;
    }

    /// <summary>
    /// Get proposals with pagination, sorting, and filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProposals(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            var result = await _proposalService.GetProposalsAsync(
                search, status, companyId, pageNumber, pageSize, sortColumn, sortDirection);
            return Ok(Result.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving proposals");
            return StatusCode(500, Result.Fail("Error retrieving proposals"));
        }
    }

    /// <summary>
    /// Get proposals summary (total count, pipeline value, won this month, conversion rate)
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            var summary = await _proposalService.GetSummaryAsync();
            return Ok(Result.Ok(summary));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving proposals summary");
            return StatusCode(500, Result.Fail("Error retrieving proposals summary"));
        }
    }

    /// <summary>
    /// Get a proposal by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProposalById(Guid id, [FromQuery] bool includeCosting = false)
    {
        try
        {
            var proposal = await _proposalService.GetProposalByIdAsync(id, includeCosting);
            if (proposal == null)
                return NotFound(new { message = "Proposal not found" });

            return Ok(proposal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving proposal {ProposalId}", id);
            return StatusCode(500, new { message = "Error retrieving proposal" });
        }
    }

    /// <summary>
    /// Create a new proposal
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Proposals.Create")]
    public async Task<IActionResult> CreateProposal([FromBody] CreateProposalDto dto)
    {
        try
        {
            var proposal = await _proposalService.CreateProposalAsync(dto);
            return CreatedAtAction(nameof(GetProposalById), new { id = proposal.Id }, proposal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating proposal");
            return StatusCode(500, new { message = "Error creating proposal" });
        }
    }

    /// <summary>
    /// Update an existing proposal
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Proposals.Edit")]
    public async Task<IActionResult> UpdateProposal(Guid id, [FromBody] UpdateProposalDto dto)
    {
        try
        {
            var proposal = await _proposalService.UpdateProposalAsync(id, dto);
            return Ok(proposal);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating proposal {ProposalId}", id);
            return StatusCode(500, new { message = "Error updating proposal" });
        }
    }

    /// <summary>
    /// Delete a proposal (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Proposals.Delete")]
    public async Task<IActionResult> DeleteProposal(Guid id)
    {
        try
        {
            await _proposalService.DeleteProposalAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting proposal {ProposalId}", id);
            return StatusCode(500, new { message = "Error deleting proposal" });
        }
    }

    #region Sections

    /// <summary>
    /// Add a section to a proposal
    /// </summary>
    [HttpPost("{id:guid}/sections")]
    [Authorize(Policy = "Proposals.Create")]
    public async Task<IActionResult> AddSection(Guid id, [FromBody] CreateProposalSectionDto dto)
    {
        try
        {
            // Ensure the proposal ID in the route matches the DTO
            if (id != dto.ProposalId)
                return BadRequest(new { message = "Proposal ID mismatch" });

            var section = await _proposalService.AddSectionAsync(dto);
            return Ok(section);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding section to proposal {ProposalId}", id);
            return StatusCode(500, new { message = "Error adding section" });
        }
    }

    /// <summary>
    /// Update a proposal section
    /// </summary>
    [HttpPut("sections/{sectionId:guid}")]
    [Authorize(Policy = "Proposals.Edit")]
    public async Task<IActionResult> UpdateSection(Guid sectionId, [FromBody] UpdateProposalSectionDto dto)
    {
        try
        {
            var section = await _proposalService.UpdateSectionAsync(sectionId, dto);
            return Ok(section);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating section {SectionId}", sectionId);
            return StatusCode(500, new { message = "Error updating section" });
        }
    }

    /// <summary>
    /// Delete a proposal section
    /// </summary>
    [HttpDelete("sections/{sectionId:guid}")]
    [Authorize(Policy = "Proposals.Delete")]
    public async Task<IActionResult> DeleteSection(Guid sectionId)
    {
        try
        {
            await _proposalService.DeleteSectionAsync(sectionId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting section {SectionId}", sectionId);
            return StatusCode(500, new { message = "Error deleting section" });
        }
    }

    #endregion

    #region Line Items

    /// <summary>
    /// Add a line item to a section
    /// </summary>
    [HttpPost("sections/{sectionId:guid}/items")]
    [Authorize(Policy = "Proposals.Create")]
    public async Task<IActionResult> AddLineItem(Guid sectionId, [FromBody] CreateProposalLineItemDto dto)
    {
        try
        {
            // Ensure the section ID in the route matches the DTO
            if (sectionId != dto.ProposalSectionId)
                return BadRequest(new { message = "Section ID mismatch" });

            var lineItem = await _proposalService.AddLineItemAsync(dto);
            return Ok(lineItem);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding line item to section {SectionId}", sectionId);
            return StatusCode(500, new { message = "Error adding line item" });
        }
    }

    /// <summary>
    /// Update a line item
    /// </summary>
    [HttpPut("items/{itemId:guid}")]
    [Authorize(Policy = "Proposals.Edit")]
    public async Task<IActionResult> UpdateLineItem(Guid itemId, [FromBody] UpdateProposalLineItemDto dto)
    {
        try
        {
            var lineItem = await _proposalService.UpdateLineItemAsync(itemId, dto);
            return Ok(lineItem);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating line item {ItemId}", itemId);
            return StatusCode(500, new { message = "Error updating line item" });
        }
    }

    /// <summary>
    /// Delete a line item
    /// </summary>
    [HttpDelete("items/{itemId:guid}")]
    [Authorize(Policy = "Proposals.Delete")]
    public async Task<IActionResult> DeleteLineItem(Guid itemId)
    {
        try
        {
            await _proposalService.DeleteLineItemAsync(itemId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting line item {ItemId}", itemId);
            return StatusCode(500, new { message = "Error deleting line item" });
        }
    }

    #endregion

    #region Contacts

    /// <summary>
    /// Add a contact to a proposal
    /// </summary>
    [HttpPost("{id:guid}/contacts")]
    [Authorize(Policy = "Proposals.Create")]
    public async Task<IActionResult> AddContact(Guid id, [FromBody] CreateProposalContactDto dto)
    {
        try
        {
            // Ensure the proposal ID in the route matches the DTO
            if (id != dto.ProposalId)
                return BadRequest(new { message = "Proposal ID mismatch" });

            var contact = await _proposalService.AddContactAsync(dto);
            return Ok(contact);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding contact to proposal {ProposalId}", id);
            return StatusCode(500, new { message = "Error adding contact" });
        }
    }

    /// <summary>
    /// Update a proposal contact
    /// </summary>
    [HttpPut("contacts/{contactId:guid}")]
    [Authorize(Policy = "Proposals.Edit")]
    public async Task<IActionResult> UpdateContact(Guid contactId, [FromBody] UpdateProposalContactDto dto)
    {
        try
        {
            var contact = await _proposalService.UpdateContactAsync(contactId, dto);
            return Ok(contact);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contact {ContactId}", contactId);
            return StatusCode(500, new { message = "Error updating contact" });
        }
    }

    /// <summary>
    /// Delete a proposal contact
    /// </summary>
    [HttpDelete("contacts/{contactId:guid}")]
    [Authorize(Policy = "Proposals.Delete")]
    public async Task<IActionResult> DeleteContact(Guid contactId)
    {
        try
        {
            await _proposalService.DeleteContactAsync(contactId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contact {ContactId}", contactId);
            return StatusCode(500, new { message = "Error deleting contact" });
        }
    }

    #endregion

    #region Calculations

    /// <summary>
    /// Manually recalculate all totals for a proposal (useful for fixing data inconsistencies)
    /// </summary>
    [HttpPost("{id:guid}/recalculate")]
    [Authorize(Policy = "Proposals.Edit")]
    public async Task<IActionResult> RecalculateProposal(Guid id)
    {
        try
        {
            await _calculationService.RecalculateAllAsync(id);
            var updatedProposal = await _proposalService.GetProposalByIdAsync(id, true);

            if (updatedProposal == null)
                return NotFound(new { message = "Proposal not found" });

            return Ok(new
            {
                message = "Proposal recalculated successfully",
                proposal = updatedProposal
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating proposal {ProposalId}", id);
            return StatusCode(500, new { message = "Error recalculating proposal" });
        }
    }

    #endregion

    #region Workflow

    /// <summary>
    /// Submit a proposal for approval
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = "Proposals.Submit")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitProposalDto dto)
    {
        try
        {
            var result = await _workflowService.SubmitAsync(id, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting proposal {ProposalId}", id);
            return StatusCode(500, new { message = "Error submitting proposal" });
        }
    }

    /// <summary>
    /// Approve a proposal
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "Proposals.Approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveProposalDto dto)
    {
        try
        {
            var result = await _workflowService.ApproveAsync(id, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving proposal {ProposalId}", id);
            return StatusCode(500, new { message = "Error approving proposal" });
        }
    }

    /// <summary>
    /// Reject a proposal
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "Proposals.Approve")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectProposalDto dto)
    {
        try
        {
            var result = await _workflowService.RejectAsync(id, dto);
            return Ok(result);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting proposal {ProposalId}", id);
            return StatusCode(500, new { message = "Error rejecting proposal" });
        }
    }

    /// <summary>
    /// Mark a proposal as won
    /// </summary>
    [HttpPost("{id:guid}/win")]
    [Authorize(Policy = "Proposals.Edit")]
    public async Task<IActionResult> Win(Guid id, [FromBody] WinProposalDto dto)
    {
        try
        {
            var result = await _workflowService.MarkWonAsync(id, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking proposal {ProposalId} as won", id);
            return StatusCode(500, new { message = "Error marking proposal as won" });
        }
    }

    /// <summary>
    /// Mark a proposal as lost
    /// </summary>
    [HttpPost("{id:guid}/lose")]
    [Authorize(Policy = "Proposals.Edit")]
    public async Task<IActionResult> Lose(Guid id, [FromBody] LoseProposalDto dto)
    {
        try
        {
            var result = await _workflowService.MarkLostAsync(id, dto);
            return Ok(result);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking proposal {ProposalId} as lost", id);
            return StatusCode(500, new { message = "Error marking proposal as lost" });
        }
    }

    /// <summary>
    /// Cancel a proposal
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "Proposals.Edit")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            var result = await _workflowService.CancelAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling proposal {ProposalId}", id);
            return StatusCode(500, new { message = "Error cancelling proposal" });
        }
    }

    /// <summary>
    /// Create a revision of a proposal
    /// </summary>
    [HttpPost("{id:guid}/revise")]
    [Authorize(Policy = "Proposals.Create")]
    public async Task<IActionResult> CreateRevision(Guid id, [FromBody] CreateRevisionDto dto)
    {
        try
        {
            var result = await _workflowService.CreateRevisionAsync(id, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating revision for proposal {ProposalId}", id);
            return StatusCode(500, new { message = "Error creating revision" });
        }
    }

    /// <summary>
    /// Get all revisions for a proposal
    /// </summary>
    [HttpGet("{id:guid}/revisions")]
    [Authorize(Policy = "Proposals.View")]
    public async Task<IActionResult> GetRevisions(Guid id)
    {
        try
        {
            var revisions = await _workflowService.GetRevisionsAsync(id);
            return Ok(revisions);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revisions for proposal {ProposalId}", id);
            return StatusCode(500, new { message = "Error getting revisions" });
        }
    }

    #endregion

    #region PDF Generation

    /// <summary>
    /// Generate a PDF document for a proposal
    /// </summary>
    [HttpGet("{id:guid}/pdf")]
    [Authorize(Policy = "Proposals.View")]
    public async Task<IActionResult> GeneratePdf(Guid id, [FromQuery] bool includeCosting = false)
    {
        try
        {
            // Check ViewCostings permission if includeCosting requested
            if (includeCosting)
            {
                var hasViewCostings = User.HasClaim("Permission", "Proposals.ViewCostings") ||
                                      User.HasClaim("Permission", "Proposals.Admin");
                if (!hasViewCostings)
                {
                    includeCosting = false;  // Silently disable if no permission
                }
            }

            var pdf = await _pdfService.GeneratePdfAsync(id, includeCosting);

            var proposal = await _proposalService.GetProposalByIdAsync(id, false);
            if (proposal == null)
            {
                return NotFound(new { message = "Proposal not found" });
            }

            var fileName = $"{proposal.ProposalNumber}_v{proposal.Version}.pdf";

            return File(pdf, "application/pdf", fileName);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for proposal {ProposalId}", id);
            return StatusCode(500, new { message = "Error generating PDF" });
        }
    }

    #endregion

    #region Reports

    /// <summary>
    /// Get pipeline report showing proposals by stage with values
    /// </summary>
    [HttpGet("reports/pipeline")]
    [Authorize(Policy = "Proposals.View")]
    public async Task<IActionResult> GetPipelineReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        try
        {
            var report = await _reportsService.GetPipelineReportAsync(fromDate, toDate);
            return Ok(Result.Ok(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating pipeline report");
            return StatusCode(500, Result.Fail("Error generating pipeline report"));
        }
    }

    /// <summary>
    /// Get conversion report with win/loss metrics
    /// </summary>
    [HttpGet("reports/conversion")]
    [Authorize(Policy = "Proposals.View")]
    public async Task<IActionResult> GetConversionReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        try
        {
            var report = await _reportsService.GetConversionReportAsync(fromDate, toDate);
            return Ok(Result.Ok(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating conversion report");
            return StatusCode(500, Result.Fail("Error generating conversion report"));
        }
    }

    /// <summary>
    /// Get proposals breakdown by status
    /// </summary>
    [HttpGet("reports/by-status")]
    [Authorize(Policy = "Proposals.View")]
    public async Task<IActionResult> GetByStatusReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        try
        {
            var report = await _reportsService.GetByStatusReportAsync(fromDate, toDate);
            return Ok(Result.Ok(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating by-status report");
            return StatusCode(500, Result.Fail("Error generating by-status report"));
        }
    }

    /// <summary>
    /// Get proposals breakdown by company
    /// </summary>
    [HttpGet("reports/by-company")]
    [Authorize(Policy = "Proposals.View")]
    public async Task<IActionResult> GetByCompanyReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int top = 10)
    {
        try
        {
            var report = await _reportsService.GetByCompanyReportAsync(fromDate, toDate, top);
            return Ok(Result.Ok(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating by-company report");
            return StatusCode(500, Result.Fail("Error generating by-company report"));
        }
    }

    /// <summary>
    /// Get win/loss analysis with reasons
    /// </summary>
    [HttpGet("reports/win-loss")]
    [Authorize(Policy = "Proposals.View")]
    public async Task<IActionResult> GetWinLossAnalysis(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        try
        {
            var report = await _reportsService.GetWinLossAnalysisAsync(fromDate, toDate);
            return Ok(Result.Ok(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating win/loss analysis");
            return StatusCode(500, Result.Fail("Error generating win/loss analysis"));
        }
    }

    /// <summary>
    /// Get monthly trends over specified number of months
    /// </summary>
    [HttpGet("reports/monthly-trends")]
    [Authorize(Policy = "Proposals.View")]
    public async Task<IActionResult> GetMonthlyTrends([FromQuery] int months = 12)
    {
        try
        {
            var report = await _reportsService.GetMonthlyTrendsAsync(months);
            return Ok(Result.Ok(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly trends report");
            return StatusCode(500, Result.Fail("Error generating monthly trends report"));
        }
    }

    #endregion

    #region Stock Order Conversion

    /// <summary>
    /// Check if a proposal can be converted to stock orders
    /// </summary>
    [HttpGet("{id:guid}/can-convert")]
    [Authorize(Policy = "Proposals.View")]
    public async Task<IActionResult> CanConvert(Guid id)
    {
        try
        {
            var canConvert = await _conversionService.CanConvertAsync(id);
            return Ok(canConvert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if proposal {ProposalId} can be converted", id);
            return StatusCode(500, new { message = "Error checking conversion eligibility" });
        }
    }

    /// <summary>
    /// Preview conversion of a proposal to stock orders
    /// </summary>
    [HttpPost("{id:guid}/preview-conversion")]
    [Authorize(Policy = "Proposals.Edit")]
    public async Task<IActionResult> PreviewConversion(Guid id, [FromBody] ConvertToStockOrderDto dto)
    {
        try
        {
            // Ensure the proposal ID in the route matches the DTO
            dto = dto with { ProposalId = id };
            var preview = await _conversionService.PreviewConversionAsync(dto);
            return Ok(preview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing conversion for proposal {ProposalId}", id);
            return StatusCode(500, new { message = "Error previewing conversion" });
        }
    }

    /// <summary>
    /// Convert a proposal to stock orders
    /// </summary>
    [HttpPost("{id:guid}/convert-to-orders")]
    [Authorize(Policy = "Proposals.Edit")]
    public async Task<IActionResult> ConvertToStockOrders(Guid id, [FromBody] ConvertToStockOrderDto dto)
    {
        try
        {
            // Ensure the proposal ID in the route matches the DTO
            dto = dto with { ProposalId = id };

            // Get the current user's name for the requestedBy field
            var requestedBy = User.FindFirst("given_name")?.Value ??
                             User.FindFirst("name")?.Value ??
                             User.Identity?.Name ??
                             "Unknown";

            var result = await _conversionService.ConvertToStockOrdersAsync(dto, requestedBy);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting proposal {ProposalId} to stock orders", id);
            return StatusCode(500, new { message = "Error converting to stock orders" });
        }
    }

    #endregion
}
