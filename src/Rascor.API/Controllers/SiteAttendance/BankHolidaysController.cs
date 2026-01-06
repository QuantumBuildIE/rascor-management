using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Core.Application.Interfaces;
using Rascor.Modules.SiteAttendance.Application.Commands.CreateBankHoliday;
using Rascor.Modules.SiteAttendance.Application.Commands.DeleteBankHoliday;
using Rascor.Modules.SiteAttendance.Application.Commands.UpdateBankHoliday;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Queries.GetBankHolidays;

namespace Rascor.API.Controllers.SiteAttendance;

[ApiController]
[Route("api/site-attendance/bank-holidays")]
[Authorize]
public class BankHolidaysController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public BankHolidaysController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get bank holidays for the tenant
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "SiteAttendance.View")]
    [ProducesResponseType(typeof(IEnumerable<BankHolidayDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] int? year)
    {
        var query = new GetBankHolidaysQuery
        {
            TenantId = _currentUserService.TenantId,
            Year = year
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a bank holiday
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "SiteAttendance.Admin")]
    [ProducesResponseType(typeof(BankHolidayDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBankHolidayRequest request)
    {
        var command = new CreateBankHolidayCommand
        {
            TenantId = _currentUserService.TenantId,
            Date = request.Date,
            Name = request.Name
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetList), result);
    }

    /// <summary>
    /// Update a bank holiday
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "SiteAttendance.Admin")]
    [ProducesResponseType(typeof(BankHolidayDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBankHolidayRequest request)
    {
        var command = new UpdateBankHolidayCommand
        {
            Id = id,
            TenantId = _currentUserService.TenantId,
            Date = request.Date,
            Name = request.Name
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete a bank holiday
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "SiteAttendance.Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteBankHolidayCommand
        {
            Id = id,
            TenantId = _currentUserService.TenantId
        };

        var result = await _mediator.Send(command);

        if (!result)
            return NotFound();

        return NoContent();
    }
}
