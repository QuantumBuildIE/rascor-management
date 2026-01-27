using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.Abstractions.Storage;
using Rascor.Modules.SiteAttendance.Application.Commands.CreateSitePhotoAttendance;
using Rascor.Modules.SiteAttendance.Application.Commands.UpdateSitePhotoAttendance;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Queries.GetSitePhotoAttendanceById;
using Rascor.Modules.SiteAttendance.Application.Queries.GetSitePhotoAttendances;

namespace Rascor.API.Controllers.SiteAttendance;

[ApiController]
[Route("api/site-attendance/spa")]
[Authorize]
public class SitePhotoAttendanceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISpaStorageService _spaStorageService;
    private readonly ILogger<SitePhotoAttendanceController> _logger;

    public SitePhotoAttendanceController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ISpaStorageService spaStorageService,
        ILogger<SitePhotoAttendanceController> logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _spaStorageService = spaStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Create a Site Photo Attendance record
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "SiteAttendance.MarkAttendance")]
    [ProducesResponseType(typeof(SitePhotoAttendanceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSpaRequest request)
    {
        var command = new CreateSitePhotoAttendanceCommand
        {
            TenantId = _currentUserService.TenantId,
            EmployeeId = request.EmployeeId,
            SiteId = request.SiteId,
            EventDate = request.EventDate,
            WeatherConditions = request.WeatherConditions,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Upload image for a Site Photo Attendance record
    /// </summary>
    [HttpPost("{id:guid}/image")]
    [Authorize(Policy = "SiteAttendance.MarkAttendance")]
    [ProducesResponseType(typeof(SitePhotoAttendanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded" });

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { error = "Invalid file type. Only JPEG, PNG, and WebP images are allowed." });

        // Validate file size (max 10MB)
        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { error = "File size exceeds 10MB limit." });

        // Verify SPA record exists and belongs to tenant
        var existingQuery = new GetSitePhotoAttendanceByIdQuery
        {
            Id = id,
            TenantId = _currentUserService.TenantId
        };
        var existingSpa = await _mediator.Send(existingQuery);
        if (existingSpa == null)
            return NotFound(new { error = "Site Photo Attendance record not found" });

        // Upload to R2 storage
        using var stream = file.OpenReadStream();
        var uploadResult = await _spaStorageService.UploadImageAsync(
            _currentUserService.TenantId,
            id,
            stream,
            file.ContentType);

        if (!uploadResult.Success)
        {
            _logger.LogError("Failed to upload SPA image for {SpaId}: {Error}", id, uploadResult.ErrorMessage);
            return BadRequest(new { error = uploadResult.ErrorMessage });
        }

        // Update the SPA record with the image URL
        var command = new UpdateSitePhotoAttendanceCommand
        {
            Id = id,
            TenantId = _currentUserService.TenantId,
            ImageUrl = uploadResult.PublicUrl
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Upload signature for a Site Photo Attendance record
    /// </summary>
    [HttpPost("{id:guid}/signature")]
    [Authorize(Policy = "SiteAttendance.MarkAttendance")]
    [ProducesResponseType(typeof(SitePhotoAttendanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadSignature(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded" });

        // Validate file type (signatures are PNG from canvas)
        if (file.ContentType.ToLower() != "image/png")
            return BadRequest(new { error = "Invalid file type. Signature must be a PNG image." });

        // Validate file size (max 1MB for signatures)
        if (file.Length > 1 * 1024 * 1024)
            return BadRequest(new { error = "Signature file size exceeds 1MB limit." });

        // Verify SPA record exists and belongs to tenant
        var existingQuery = new GetSitePhotoAttendanceByIdQuery
        {
            Id = id,
            TenantId = _currentUserService.TenantId
        };
        var existingSpa = await _mediator.Send(existingQuery);
        if (existingSpa == null)
            return NotFound(new { error = "Site Photo Attendance record not found" });

        // Upload to R2 storage
        using var stream = file.OpenReadStream();
        var uploadResult = await _spaStorageService.UploadSignatureAsync(
            _currentUserService.TenantId,
            id,
            stream);

        if (!uploadResult.Success)
        {
            _logger.LogError("Failed to upload SPA signature for {SpaId}: {Error}", id, uploadResult.ErrorMessage);
            return BadRequest(new { error = uploadResult.ErrorMessage });
        }

        // Update the SPA record with the signature URL
        var command = new UpdateSitePhotoAttendanceCommand
        {
            Id = id,
            TenantId = _currentUserService.TenantId,
            SignatureUrl = uploadResult.PublicUrl
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Update a Site Photo Attendance record
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "SiteAttendance.MarkAttendance")]
    [ProducesResponseType(typeof(SitePhotoAttendanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSpaRequest request)
    {
        var command = new UpdateSitePhotoAttendanceCommand
        {
            Id = id,
            TenantId = _currentUserService.TenantId,
            WeatherConditions = request.WeatherConditions,
            ImageUrl = request.ImageUrl,
            SignatureUrl = request.SignatureUrl,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get Site Photo Attendance records with filters
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "SiteAttendance.View")]
    [ProducesResponseType(typeof(PaginatedList<SitePhotoAttendanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? siteId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetSitePhotoAttendancesQuery
        {
            TenantId = _currentUserService.TenantId,
            EmployeeId = employeeId,
            SiteId = siteId,
            FromDate = fromDate,
            ToDate = toDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get Site Photo Attendance by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "SiteAttendance.View")]
    [ProducesResponseType(typeof(SitePhotoAttendanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetSitePhotoAttendanceByIdQuery
        {
            Id = id,
            TenantId = _currentUserService.TenantId
        };

        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
