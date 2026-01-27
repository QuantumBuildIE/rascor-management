namespace Rascor.Modules.SiteAttendance.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for SPA (Site Photo Attendance) R2 storage.
/// Binds to the "SpaStorage" section in appsettings.json.
/// Falls back to "R2Storage" if "SpaStorage" is not configured.
/// </summary>
public class SpaStorageSettings
{
    public const string SectionName = "SpaStorage";
    public const string FallbackSectionName = "R2Storage";

    /// <summary>
    /// R2 bucket name (e.g., "rascor-media")
    /// </summary>
    public string BucketName { get; set; } = "rascor-media";

    /// <summary>
    /// Public URL base for the R2 bucket (e.g., "https://pub-xxxxx.r2.dev")
    /// </summary>
    public string PublicUrl { get; set; } = string.Empty;

    /// <summary>
    /// S3-compatible endpoint URL (e.g., "https://xxxxx.eu.r2.cloudflarestorage.com")
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// R2 API access key ID
    /// </summary>
    public string AccessKeyId { get; set; } = string.Empty;

    /// <summary>
    /// R2 API secret access key
    /// </summary>
    public string SecretAccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Maximum image file size in bytes (default: 10MB)
    /// </summary>
    public long MaxImageSizeBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Maximum signature file size in bytes (default: 1MB)
    /// </summary>
    public long MaxSignatureSizeBytes { get; set; } = 1 * 1024 * 1024;

    /// <summary>
    /// Allowed image content types
    /// </summary>
    public string[] AllowedImageTypes { get; set; } =
    [
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    ];
}
