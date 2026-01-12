using Rascor.Modules.Rams.Application.DTOs;

namespace Rascor.Modules.Rams.Application.Services;

/// <summary>
/// Service interface for generating RAMS email templates
/// </summary>
public interface IRamsEmailTemplateService
{
    /// <summary>
    /// Generates email template for document submission notification
    /// </summary>
    RamsEmailTemplateDto GetSubmitTemplate(
        string projectReference,
        string projectName,
        string submittedBy,
        string documentUrl,
        int riskCount,
        int highRiskCount);

    /// <summary>
    /// Generates email template for approval notification
    /// </summary>
    RamsEmailTemplateDto GetApprovalTemplate(
        string projectReference,
        string projectName,
        string approvedBy,
        string documentUrl);

    /// <summary>
    /// Generates email template for rejection notification
    /// </summary>
    RamsEmailTemplateDto GetRejectionTemplate(
        string projectReference,
        string projectName,
        string rejectedBy,
        string? rejectionComments,
        string documentUrl);

    /// <summary>
    /// Generates email template for daily digest
    /// </summary>
    RamsEmailTemplateDto GetDailyDigestTemplate(
        int pendingCount,
        int overdueCount,
        List<RamsPendingApprovalDto> pendingItems,
        List<RamsOverdueDocumentDto> overdueItems,
        string dashboardUrl);

    /// <summary>
    /// Generates a test notification email template
    /// </summary>
    RamsEmailTemplateDto GetTestTemplate();
}
