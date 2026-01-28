using Microsoft.EntityFrameworkCore;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Services;
using Rascor.Modules.SiteAttendance.Infrastructure.Persistence;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Services;

/// <summary>
/// Admin device management service implementation.
/// Supports device-to-employee linking for Zoho migration workflow.
/// </summary>
public class AdminDeviceService : IAdminDeviceService
{
    private readonly SiteAttendanceDbContext _dbContext;

    public AdminDeviceService(SiteAttendanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedList<AdminDeviceListDto>> GetDevicesAsync(
        Guid tenantId,
        bool? isLinked = null,
        bool? isActive = null,
        string? search = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _dbContext.DeviceRegistrations
            .Include(d => d.Employee)
            .Where(d => d.TenantId == tenantId);

        // Apply linked filter
        if (isLinked.HasValue)
        {
            query = isLinked.Value
                ? query.Where(d => d.EmployeeId != null)
                : query.Where(d => d.EmployeeId == null);
        }

        // Apply active filter
        if (isActive.HasValue)
        {
            query = query.Where(d => d.IsActive == isActive.Value);
        }

        // Apply search filter (device ID or employee name)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(d =>
                d.DeviceIdentifier.ToLower().Contains(searchLower) ||
                (d.Employee != null &&
                    (d.Employee.FirstName.ToLower().Contains(searchLower) ||
                     d.Employee.LastName.ToLower().Contains(searchLower) ||
                     (d.Employee.FirstName + " " + d.Employee.LastName).ToLower().Contains(searchLower))));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(ct);

        // Order and paginate
        var devices = await query
            .OrderByDescending(d => d.LastActiveAt ?? d.RegisteredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new AdminDeviceListDto
            {
                Id = d.Id,
                DeviceIdentifier = d.DeviceIdentifier,
                Platform = d.Platform,
                DeviceName = d.DeviceName,
                RegisteredAt = d.RegisteredAt,
                LastActiveAt = d.LastActiveAt,
                IsActive = d.IsActive,
                EmployeeId = d.EmployeeId,
                EmployeeName = d.Employee != null
                    ? d.Employee.FirstName + " " + d.Employee.LastName
                    : null,
                EmployeeEmail = d.Employee != null ? d.Employee.Email : null,
                LinkedAt = d.LinkedAt,
                LinkedBy = d.LinkedBy
            })
            .ToListAsync(ct);

        return new PaginatedList<AdminDeviceListDto>(devices, totalCount, page, pageSize);
    }

    public async Task<AdminDeviceDetailDto?> GetDeviceAsync(
        Guid tenantId,
        Guid deviceId,
        CancellationToken ct = default)
    {
        var device = await _dbContext.DeviceRegistrations
            .Include(d => d.Employee)
            .Where(d => d.TenantId == tenantId && d.Id == deviceId)
            .Select(d => new AdminDeviceDetailDto
            {
                Id = d.Id,
                DeviceIdentifier = d.DeviceIdentifier,
                Platform = d.Platform,
                DeviceName = d.DeviceName,
                RegisteredAt = d.RegisteredAt,
                LastActiveAt = d.LastActiveAt,
                IsActive = d.IsActive,
                EmployeeId = d.EmployeeId,
                EmployeeName = d.Employee != null
                    ? d.Employee.FirstName + " " + d.Employee.LastName
                    : null,
                EmployeeEmail = d.Employee != null ? d.Employee.Email : null,
                LinkedAt = d.LinkedAt,
                LinkedBy = d.LinkedBy,
                PushToken = d.PushToken,
                UnlinkedAt = d.UnlinkedAt,
                UnlinkedReason = d.UnlinkedReason
            })
            .FirstOrDefaultAsync(ct);

        return device;
    }

    public async Task<Result> LinkDeviceToEmployeeAsync(
        Guid tenantId,
        Guid deviceId,
        Guid employeeId,
        CancellationToken ct = default)
    {
        var device = await _dbContext.DeviceRegistrations
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == deviceId, ct);

        if (device == null)
        {
            return Result.Fail("Device not found");
        }

        // Verify employee exists in the same tenant
        var employeeExists = await _dbContext.Set<Rascor.Core.Domain.Entities.Employee>()
            .AnyAsync(e => e.TenantId == tenantId && e.Id == employeeId && !e.IsDeleted, ct);

        if (!employeeExists)
        {
            return Result.Fail("Employee not found");
        }

        device.LinkToEmployee(employeeId, "Admin");
        await _dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }

    public async Task<Result> UnlinkDeviceAsync(
        Guid tenantId,
        Guid deviceId,
        string reason,
        CancellationToken ct = default)
    {
        var device = await _dbContext.DeviceRegistrations
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == deviceId, ct);

        if (device == null)
        {
            return Result.Fail("Device not found");
        }

        if (device.EmployeeId == null)
        {
            return Result.Fail("Device is not linked to any employee");
        }

        device.Unlink(reason);
        await _dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }

    public async Task<Result> DeactivateDeviceAsync(
        Guid tenantId,
        Guid deviceId,
        CancellationToken ct = default)
    {
        var device = await _dbContext.DeviceRegistrations
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == deviceId, ct);

        if (device == null)
        {
            return Result.Fail("Device not found");
        }

        if (!device.IsActive)
        {
            return Result.Fail("Device is already inactive");
        }

        device.Deactivate();
        await _dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }

    public async Task<Result> ReactivateDeviceAsync(
        Guid tenantId,
        Guid deviceId,
        CancellationToken ct = default)
    {
        var device = await _dbContext.DeviceRegistrations
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == deviceId, ct);

        if (device == null)
        {
            return Result.Fail("Device not found");
        }

        if (device.IsActive)
        {
            return Result.Fail("Device is already active");
        }

        device.Reactivate();
        await _dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
