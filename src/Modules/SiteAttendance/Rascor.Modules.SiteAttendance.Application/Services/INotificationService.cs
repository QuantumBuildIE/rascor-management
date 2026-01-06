using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Application.Services;

public interface INotificationService
{
    Task SendPushNotificationAsync(Guid employeeId, string title, string message, string? deepLink = null, CancellationToken cancellationToken = default);

    Task SendEmailNotificationAsync(Guid employeeId, string subject, string htmlContent, CancellationToken cancellationToken = default);

    Task SendSmsNotificationAsync(Guid employeeId, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if employee has SPA for today and sends notification if missing
    /// </summary>
    Task CheckAndNotifyMissingSpaAsync(AttendanceEvent entryEvent, CancellationToken cancellationToken = default);
}
