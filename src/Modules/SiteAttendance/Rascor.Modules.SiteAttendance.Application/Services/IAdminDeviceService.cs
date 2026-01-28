using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Services;

/// <summary>
/// Service for admin device management operations.
/// Supports the Zoho migration workflow where existing devices need to be manually linked to employees.
/// </summary>
public interface IAdminDeviceService
{
    /// <summary>
    /// Get paginated list of devices with optional filtering
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="isLinked">Filter: true=linked only, false=unlinked only, null=all</param>
    /// <param name="isActive">Filter: true=active only, false=inactive only, null=all</param>
    /// <param name="search">Search by device ID or employee name</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of devices</returns>
    Task<PaginatedList<AdminDeviceListDto>> GetDevicesAsync(
        Guid tenantId,
        bool? isLinked = null,
        bool? isActive = null,
        string? search = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Get detailed device information by ID
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="deviceId">Device ID (Guid)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Device details or null if not found</returns>
    Task<AdminDeviceDetailDto?> GetDeviceAsync(Guid tenantId, Guid deviceId, CancellationToken ct = default);

    /// <summary>
    /// Link a device to an employee (admin operation for Zoho migration)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="deviceId">Device ID (Guid)</param>
    /// <param name="employeeId">Employee to link to</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result with success or error</returns>
    Task<Result> LinkDeviceToEmployeeAsync(Guid tenantId, Guid deviceId, Guid employeeId, CancellationToken ct = default);

    /// <summary>
    /// Unlink a device from its current employee
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="deviceId">Device ID (Guid)</param>
    /// <param name="reason">Reason for unlinking</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result with success or error</returns>
    Task<Result> UnlinkDeviceAsync(Guid tenantId, Guid deviceId, string reason, CancellationToken ct = default);

    /// <summary>
    /// Deactivate a device (soft delete)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="deviceId">Device ID (Guid)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result with success or error</returns>
    Task<Result> DeactivateDeviceAsync(Guid tenantId, Guid deviceId, CancellationToken ct = default);

    /// <summary>
    /// Reactivate a previously deactivated device
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="deviceId">Device ID (Guid)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result with success or error</returns>
    Task<Result> ReactivateDeviceAsync(Guid tenantId, Guid deviceId, CancellationToken ct = default);
}
