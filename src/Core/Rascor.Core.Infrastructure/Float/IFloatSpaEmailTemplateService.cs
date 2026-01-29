using Rascor.Core.Domain.Entities;

namespace Rascor.Core.Infrastructure.Float;

/// <summary>
/// Service for generating SPA reminder email templates.
/// </summary>
public interface IFloatSpaEmailTemplateService
{
    /// <summary>
    /// Generate the HTML body for an SPA reminder email.
    /// </summary>
    /// <param name="employee">The employee to send to</param>
    /// <param name="site">The site they are scheduled at</param>
    /// <param name="scheduledDate">The scheduled work date</param>
    /// <param name="spaSubmissionUrl">URL to submit SPA</param>
    /// <returns>HTML email body</returns>
    string GenerateReminderEmailHtml(
        Employee employee,
        Site site,
        DateOnly scheduledDate,
        string spaSubmissionUrl);

    /// <summary>
    /// Generate the subject line for an SPA reminder email.
    /// </summary>
    /// <param name="site">The site they are scheduled at</param>
    /// <param name="scheduledDate">The scheduled work date</param>
    /// <returns>Email subject line</returns>
    string GenerateReminderEmailSubject(Site site, DateOnly scheduledDate);

    /// <summary>
    /// Generate the plain text body for an SPA reminder email.
    /// </summary>
    /// <param name="employee">The employee to send to</param>
    /// <param name="site">The site they are scheduled at</param>
    /// <param name="scheduledDate">The scheduled work date</param>
    /// <param name="spaSubmissionUrl">URL to submit SPA</param>
    /// <returns>Plain text email body</returns>
    string GenerateReminderEmailPlainText(
        Employee employee,
        Site site,
        DateOnly scheduledDate,
        string spaSubmissionUrl);
}
