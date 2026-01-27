namespace Rascor.Modules.SiteAttendance.Application.Abstractions.Storage;

/// <summary>
/// Service for managing SPA (Site Photo Attendance) files in R2 storage with tenant isolation.
/// All files are stored under {tenant-id}/spa/ prefix.
/// </summary>
public interface ISpaStorageService
{
    /// <summary>
    /// Uploads an image file to R2 storage.
    /// Path: {tenantId}/spa/images/{spaId}.{ext}
    /// </summary>
    Task<SpaUploadResult> UploadImageAsync(
        Guid tenantId,
        Guid spaId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a signature image to R2 storage.
    /// Path: {tenantId}/spa/signatures/{spaId}.png
    /// </summary>
    Task<SpaUploadResult> UploadSignatureAsync(
        Guid tenantId,
        Guid spaId,
        Stream content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all files associated with a SPA record (image and signature).
    /// </summary>
    Task DeleteSpaFilesAsync(
        Guid tenantId,
        Guid spaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates the public URL for a file in the R2 bucket.
    /// </summary>
    string GeneratePublicUrl(Guid tenantId, string folder, string fileName);
}
