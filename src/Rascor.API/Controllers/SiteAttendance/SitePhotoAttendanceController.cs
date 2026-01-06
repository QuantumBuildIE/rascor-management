using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
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

    public SitePhotoAttendanceController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
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
            return BadRequest("No file uploaded");

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest("Invalid file type. Only JPEG, PNG, and WebP images are allowed.");

        // Validate file size (max 10MB)
        if (file.Length > 10 * 1024 * 1024)
            return BadRequest("File size exceeds 10MB limit.");

        // For now, we'll store the image in a local folder
        // In production, this should be uploaded to Azure Blob Storage or similar
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "spa");
        Directory.CreateDirectory(uploadsFolder);

        var fileExtension = Path.GetExtension(file.FileName);
        var fileName = $"{id}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Update the SPA record with the image URL
        var imageUrl = $"/uploads/spa/{fileName}";
        var command = new UpdateSitePhotoAttendanceCommand
        {
            Id = id,
            TenantId = _currentUserService.TenantId,
            ImageUrl = imageUrl
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
