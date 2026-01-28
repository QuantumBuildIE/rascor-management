using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rascor.Core.Application.Features.Employees.DTOs;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Core.Domain.Entities;

namespace Rascor.Core.Application.Features.Employees;

public class EmployeeService : IEmployeeService
{
    private readonly ICoreDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmployeeService> _logger;

    private const string DefaultUserRole = "SiteManager";

    public EmployeeService(
        ICoreDbContext context,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        ILogger<EmployeeService> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<List<EmployeeDto>>> GetAllAsync()
    {
        try
        {
            var employees = await _context.Employees
                .Include(e => e.PrimarySite)
                .OrderBy(e => e.EmployeeCode)
                .Select(e => new EmployeeDto(
                    e.Id,
                    e.EmployeeCode,
                    e.FirstName,
                    e.LastName,
                    e.FirstName + " " + e.LastName,
                    e.Email,
                    e.Phone,
                    e.Mobile,
                    e.JobTitle,
                    e.Department,
                    e.PrimarySiteId,
                    e.PrimarySite != null ? e.PrimarySite.SiteName : null,
                    e.StartDate,
                    e.EndDate,
                    e.IsActive,
                    e.Notes,
                    e.GeoTrackerID,
                    e.UserId != null,
                    e.UserId != null ? Guid.Parse(e.UserId) : null,
                    e.PreferredLanguage
                ))
                .ToListAsync();

            return Result.Ok(employees);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<EmployeeDto>>($"Error retrieving employees: {ex.Message}");
        }
    }

    public async Task<Result<PaginatedList<EmployeeDto>>> GetPaginatedAsync(GetEmployeesQueryDto query)
    {
        try
        {
            var employeesQuery = _context.Employees
                .Include(e => e.PrimarySite)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var searchLower = query.Search.ToLower();
                employeesQuery = employeesQuery.Where(e =>
                    e.EmployeeCode.ToLower().Contains(searchLower) ||
                    e.FirstName.ToLower().Contains(searchLower) ||
                    e.LastName.ToLower().Contains(searchLower) ||
                    (e.FirstName + " " + e.LastName).ToLower().Contains(searchLower) ||
                    (e.Email != null && e.Email.ToLower().Contains(searchLower)) ||
                    (e.JobTitle != null && e.JobTitle.ToLower().Contains(searchLower))
                );
            }

            // Apply sorting
            employeesQuery = ApplySorting(employeesQuery, query.SortColumn, query.SortDirection);

            // Get total count before pagination
            var totalCount = await employeesQuery.CountAsync();

            // Apply pagination
            var employees = await employeesQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(e => new EmployeeDto(
                    e.Id,
                    e.EmployeeCode,
                    e.FirstName,
                    e.LastName,
                    e.FirstName + " " + e.LastName,
                    e.Email,
                    e.Phone,
                    e.Mobile,
                    e.JobTitle,
                    e.Department,
                    e.PrimarySiteId,
                    e.PrimarySite != null ? e.PrimarySite.SiteName : null,
                    e.StartDate,
                    e.EndDate,
                    e.IsActive,
                    e.Notes,
                    e.GeoTrackerID,
                    e.UserId != null,
                    e.UserId != null ? Guid.Parse(e.UserId) : null,
                    e.PreferredLanguage
                ))
                .ToListAsync();

            var result = new PaginatedList<EmployeeDto>(employees, totalCount, query.PageNumber, query.PageSize);
            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            return Result.Fail<PaginatedList<EmployeeDto>>($"Error retrieving employees: {ex.Message}");
        }
    }

    private static IQueryable<Employee> ApplySorting(IQueryable<Employee> query, string? sortColumn, string? sortDirection)
    {
        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortColumn?.ToLower() switch
        {
            "employeecode" => isDescending ? query.OrderByDescending(e => e.EmployeeCode) : query.OrderBy(e => e.EmployeeCode),
            "firstname" => isDescending ? query.OrderByDescending(e => e.FirstName) : query.OrderBy(e => e.FirstName),
            "lastname" => isDescending ? query.OrderByDescending(e => e.LastName) : query.OrderBy(e => e.LastName),
            "fullname" or "name" => isDescending
                ? query.OrderByDescending(e => e.FirstName + " " + e.LastName)
                : query.OrderBy(e => e.FirstName + " " + e.LastName),
            "email" => isDescending ? query.OrderByDescending(e => e.Email) : query.OrderBy(e => e.Email),
            "jobtitle" => isDescending ? query.OrderByDescending(e => e.JobTitle) : query.OrderBy(e => e.JobTitle),
            "primarysitename" => isDescending
                ? query.OrderByDescending(e => e.PrimarySite != null ? e.PrimarySite.SiteName : "")
                : query.OrderBy(e => e.PrimarySite != null ? e.PrimarySite.SiteName : ""),
            "isactive" => isDescending ? query.OrderByDescending(e => e.IsActive) : query.OrderBy(e => e.IsActive),
            _ => query.OrderBy(e => e.EmployeeCode)
        };
    }

    public async Task<Result<EmployeeDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var employee = await _context.Employees
                .Include(e => e.PrimarySite)
                .Where(e => e.Id == id)
                .Select(e => new EmployeeDto(
                    e.Id,
                    e.EmployeeCode,
                    e.FirstName,
                    e.LastName,
                    e.FirstName + " " + e.LastName,
                    e.Email,
                    e.Phone,
                    e.Mobile,
                    e.JobTitle,
                    e.Department,
                    e.PrimarySiteId,
                    e.PrimarySite != null ? e.PrimarySite.SiteName : null,
                    e.StartDate,
                    e.EndDate,
                    e.IsActive,
                    e.Notes,
                    e.GeoTrackerID,
                    e.UserId != null,
                    e.UserId != null ? Guid.Parse(e.UserId) : null,
                    e.PreferredLanguage
                ))
                .FirstOrDefaultAsync();

            if (employee == null)
            {
                return Result.Fail<EmployeeDto>($"Employee with ID {id} not found");
            }

            return Result.Ok(employee);
        }
        catch (Exception ex)
        {
            return Result.Fail<EmployeeDto>($"Error retrieving employee: {ex.Message}");
        }
    }

    public async Task<Result<EmployeeDto>> CreateAsync(CreateEmployeeDto dto)
    {
        try
        {
            var tenantId = _currentUserService.TenantId;

            // Validate that PrimarySiteId exists if provided
            if (dto.PrimarySiteId.HasValue)
            {
                var siteExists = await _context.Sites
                    .AnyAsync(s => s.Id == dto.PrimarySiteId.Value);

                if (!siteExists)
                {
                    return Result.Fail<EmployeeDto>($"Site with ID {dto.PrimarySiteId} not found");
                }
            }

            // Check for duplicate EmployeeCode within the same tenant
            var duplicateCode = await _context.Employees
                .AnyAsync(e => e.EmployeeCode == dto.EmployeeCode);

            if (duplicateCode)
            {
                return Result.Fail<EmployeeDto>($"Employee with code '{dto.EmployeeCode}' already exists");
            }

            // Validate email uniqueness if creating a user account
            if (dto.CreateUserAccount && !string.IsNullOrWhiteSpace(dto.Email))
            {
                // Check for existing employee with same email
                var existingEmployee = await _context.Employees
                    .AnyAsync(e => e.Email == dto.Email);

                if (existingEmployee)
                {
                    return Result.Fail<EmployeeDto>($"An employee with email '{dto.Email}' already exists");
                }

                // Check for existing user with same email
                var existingUser = await _userManager.Users
                    .Where(u => u.TenantId == tenantId && u.NormalizedEmail == dto.Email.ToUpperInvariant())
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    return Result.Fail<EmployeeDto>($"A user account with email '{dto.Email}' already exists");
                }
            }

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                EmployeeCode = dto.EmployeeCode,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Mobile = dto.Mobile,
                JobTitle = dto.JobTitle,
                Department = dto.Department,
                PrimarySiteId = dto.PrimarySiteId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive,
                Notes = dto.Notes,
                PreferredLanguage = dto.PreferredLanguage
            };

            employee.SetGeoTrackerID(dto.GeoTrackerID);

            _context.Employees.Add(employee);

            User? createdUser = null;

            // Create linked User account if requested and email is provided
            if (dto.CreateUserAccount && !string.IsNullOrWhiteSpace(dto.Email))
            {
                var userCreationResult = await CreateLinkedUserAccountAsync(
                    employee,
                    dto.Email,
                    tenantId,
                    dto.UserRole);

                if (!userCreationResult.Success)
                {
                    return Result.Fail<EmployeeDto>(userCreationResult.Errors);
                }

                createdUser = userCreationResult.Data;
                employee.UserId = createdUser.Id.ToString();
            }

            await _context.SaveChangesAsync();

            // Send password setup email if user was created
            if (createdUser != null && !string.IsNullOrWhiteSpace(dto.Email))
            {
                try
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(createdUser);
                    await _emailService.SendPasswordSetupEmailAsync(dto.Email, dto.FirstName, token);
                    _logger.LogInformation(
                        "Sent password setup email to {Email} for Employee {EmployeeId}",
                        dto.Email,
                        employee.Id);
                }
                catch (Exception emailEx)
                {
                    // Log but don't fail - the user account was created successfully
                    _logger.LogWarning(
                        emailEx,
                        "Failed to send password setup email to {Email} for Employee {EmployeeId}",
                        dto.Email,
                        employee.Id);
                }
            }

            // Reload with related entities to get site name
            var createdEmployee = await _context.Employees
                .Include(e => e.PrimarySite)
                .FirstAsync(e => e.Id == employee.Id);

            var employeeDto = new EmployeeDto(
                createdEmployee.Id,
                createdEmployee.EmployeeCode,
                createdEmployee.FirstName,
                createdEmployee.LastName,
                createdEmployee.FirstName + " " + createdEmployee.LastName,
                createdEmployee.Email,
                createdEmployee.Phone,
                createdEmployee.Mobile,
                createdEmployee.JobTitle,
                createdEmployee.Department,
                createdEmployee.PrimarySiteId,
                createdEmployee.PrimarySite?.SiteName,
                createdEmployee.StartDate,
                createdEmployee.EndDate,
                createdEmployee.IsActive,
                createdEmployee.Notes,
                createdEmployee.GeoTrackerID,
                createdEmployee.UserId != null,
                createdUser?.Id,
                createdEmployee.PreferredLanguage
            );

            return Result.Ok(employeeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return Result.Fail<EmployeeDto>($"Error creating employee: {ex.Message}");
        }
    }

    private async Task<Result<User>> CreateLinkedUserAccountAsync(
        Employee employee,
        string email,
        Guid tenantId,
        string? roleName)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            TenantId = tenantId,
            EmployeeId = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            EmailConfirmed = false, // Will confirm when they set password
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUserService.UserId
        };

        // Generate a secure temporary password
        var tempPassword = GenerateTemporaryPassword();
        var createResult = await _userManager.CreateAsync(user, tempPassword);

        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning(
                "Failed to create user account for Employee {EmployeeId}: {Errors}",
                employee.Id,
                string.Join(", ", errors));
            return Result.Fail<User>($"Failed to create user account: {string.Join(", ", errors)}");
        }

        // Assign role
        var role = roleName ?? DefaultUserRole;
        var roleExists = await _roleManager.RoleExistsAsync(role);

        if (!roleExists)
        {
            _logger.LogWarning(
                "Role '{Role}' does not exist, falling back to default role '{DefaultRole}'",
                role,
                DefaultUserRole);
            role = DefaultUserRole;
            roleExists = await _roleManager.RoleExistsAsync(role);
        }

        if (roleExists)
        {
            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                _logger.LogWarning(
                    "Failed to assign role '{Role}' to user {UserId}: {Errors}",
                    role,
                    user.Id,
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }

        _logger.LogInformation(
            "Created User account {UserId} for Employee {EmployeeId} with role '{Role}'",
            user.Id,
            employee.Id,
            role);

        return Result.Ok(user);
    }

    private static string GenerateTemporaryPassword()
    {
        // Generate a secure temporary password that meets ASP.NET Identity requirements
        // This password will never be used - user will reset via email link
        return $"Temp{Guid.NewGuid():N}!Aa1";
    }

    public async Task<Result<EmployeeDto>> UpdateAsync(Guid id, UpdateEmployeeDto dto)
    {
        try
        {
            var employee = await _context.Employees
                .Include(e => e.PrimarySite)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return Result.Fail<EmployeeDto>($"Employee with ID {id} not found");
            }

            // Validate that PrimarySiteId exists if provided
            if (dto.PrimarySiteId.HasValue)
            {
                var siteExists = await _context.Sites
                    .AnyAsync(s => s.Id == dto.PrimarySiteId.Value);

                if (!siteExists)
                {
                    return Result.Fail<EmployeeDto>($"Site with ID {dto.PrimarySiteId} not found");
                }
            }

            // Check for duplicate EmployeeCode (excluding current employee)
            var duplicateCode = await _context.Employees
                .AnyAsync(e => e.EmployeeCode == dto.EmployeeCode && e.Id != id);

            if (duplicateCode)
            {
                return Result.Fail<EmployeeDto>($"Employee with code '{dto.EmployeeCode}' already exists");
            }

            var emailChanged = employee.Email != dto.Email;

            // Check email uniqueness if changed
            if (emailChanged && !string.IsNullOrWhiteSpace(dto.Email))
            {
                var emailExists = await _context.Employees
                    .AnyAsync(e => e.Email == dto.Email && e.Id != id);

                if (emailExists)
                {
                    return Result.Fail<EmployeeDto>($"An employee with email '{dto.Email}' already exists");
                }
            }

            employee.EmployeeCode = dto.EmployeeCode;
            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.Email = dto.Email;
            employee.Phone = dto.Phone;
            employee.Mobile = dto.Mobile;
            employee.JobTitle = dto.JobTitle;
            employee.Department = dto.Department;
            employee.PrimarySiteId = dto.PrimarySiteId;
            employee.StartDate = dto.StartDate;
            employee.EndDate = dto.EndDate;
            employee.IsActive = dto.IsActive;
            employee.Notes = dto.Notes;
            employee.SetGeoTrackerID(dto.GeoTrackerID);

            // Update preferred language if provided
            if (!string.IsNullOrWhiteSpace(dto.PreferredLanguage))
            {
                employee.PreferredLanguage = dto.PreferredLanguage;
            }

            // Sync User account if email changed and employee has a linked user
            if (emailChanged && !string.IsNullOrWhiteSpace(employee.UserId))
            {
                var syncResult = await SyncUserEmailAsync(employee.UserId, dto.Email);
                if (!syncResult.Success)
                {
                    _logger.LogWarning(
                        "Failed to sync email change to User account for Employee {EmployeeId}: {Error}",
                        employee.Id,
                        string.Join(", ", syncResult.Errors));
                }
            }

            // Sync name changes to linked User if exists
            if (!string.IsNullOrWhiteSpace(employee.UserId))
            {
                await SyncUserNameAsync(employee.UserId, dto.FirstName, dto.LastName);
            }

            await _context.SaveChangesAsync();

            // Reload to get updated related entity name
            await _context.Employees
                .Entry(employee)
                .Reference(e => e.PrimarySite)
                .LoadAsync();

            var employeeDto = new EmployeeDto(
                employee.Id,
                employee.EmployeeCode,
                employee.FirstName,
                employee.LastName,
                employee.FirstName + " " + employee.LastName,
                employee.Email,
                employee.Phone,
                employee.Mobile,
                employee.JobTitle,
                employee.Department,
                employee.PrimarySiteId,
                employee.PrimarySite?.SiteName,
                employee.StartDate,
                employee.EndDate,
                employee.IsActive,
                employee.Notes,
                employee.GeoTrackerID,
                employee.UserId != null,
                employee.UserId != null ? Guid.Parse(employee.UserId) : null,
                employee.PreferredLanguage
            );

            return Result.Ok(employeeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee {EmployeeId}", id);
            return Result.Fail<EmployeeDto>($"Error updating employee: {ex.Message}");
        }
    }

    private async Task<Result> SyncUserEmailAsync(string userId, string? newEmail)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail($"Linked user account {userId} not found");
            }

            if (string.IsNullOrWhiteSpace(newEmail))
            {
                // Don't clear the email - user needs it to log in
                _logger.LogWarning(
                    "Cannot clear email for User {UserId} - email is required for login",
                    userId);
                return Result.Ok();
            }

            // Check if new email conflicts with another user
            var existingUser = await _userManager.FindByEmailAsync(newEmail);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                return Result.Fail($"Email '{newEmail}' is already in use by another user");
            }

            user.Email = newEmail;
            user.UserName = newEmail;
            user.NormalizedEmail = newEmail.ToUpperInvariant();
            user.NormalizedUserName = newEmail.ToUpperInvariant();
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = _currentUserService.UserId;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Result.Fail(result.Errors.Select(e => e.Description).ToList());
            }

            _logger.LogInformation(
                "Synced email change to User {UserId}: {NewEmail}",
                userId,
                newEmail);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing email to User {UserId}", userId);
            return Result.Fail($"Error syncing email: {ex.Message}");
        }
    }

    private async Task SyncUserNameAsync(string userId, string firstName, string lastName)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return;
            }

            if (user.FirstName != firstName || user.LastName != lastName)
            {
                user.FirstName = firstName;
                user.LastName = lastName;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = _currentUserService.UserId;

                await _userManager.UpdateAsync(user);

                _logger.LogInformation(
                    "Synced name change to User {UserId}: {FirstName} {LastName}",
                    userId,
                    firstName,
                    lastName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error syncing name to User {UserId}", userId);
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return Result.Fail($"Employee with ID {id} not found");
            }

            // Soft delete Employee
            employee.IsDeleted = true;
            employee.IsActive = false;

            // Deactivate linked User account
            if (!string.IsNullOrWhiteSpace(employee.UserId))
            {
                await DeactivateLinkedUserAsync(employee.UserId);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted Employee {EmployeeId}", id);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee {EmployeeId}", id);
            return Result.Fail($"Error deleting employee: {ex.Message}");
        }
    }

    private async Task DeactivateLinkedUserAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Linked user account {UserId} not found for deactivation", userId);
                return;
            }

            user.IsActive = false;
            user.LockoutEnd = DateTimeOffset.MaxValue; // Lock them out permanently
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = _currentUserService.UserId;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "Deactivated User account {UserId} due to Employee deletion",
                    userId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to deactivate User {UserId}: {Errors}",
                    userId,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deactivating User {UserId}", userId);
        }
    }
}
