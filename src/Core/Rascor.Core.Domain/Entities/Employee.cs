using Rascor.Core.Domain.Common;

namespace Rascor.Core.Domain.Entities;

/// <summary>
/// Represents an employee within the organization
/// </summary>
public class Employee : TenantEntity
{
    /// <summary>
    /// Unique employee code within the tenant
    /// </summary>
    public string EmployeeCode { get; set; } = string.Empty;

    /// <summary>
    /// Employee's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Employee's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Full name (computed property)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Employee's email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Employee's phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Employee's mobile number
    /// </summary>
    public string? Mobile { get; set; }

    /// <summary>
    /// Job title or position
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Department the employee belongs to
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Associated User ID (links to Identity system)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Primary site where employee is based
    /// </summary>
    public Guid? PrimarySiteId { get; set; }

    /// <summary>
    /// Primary site navigation property
    /// </summary>
    public Site? PrimarySite { get; set; }

    /// <summary>
    /// Employee status - true if active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date when employment started
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Date when employment ended (null if still employed)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Additional notes about the employee
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Preferred language for communications (ISO 639-1 code, e.g., "en", "es", "fr", "pl")
    /// Used for toolbox talk translations and email notifications
    /// </summary>
    public string PreferredLanguage { get; set; } = "en";
}
