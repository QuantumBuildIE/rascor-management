namespace Rascor.Modules.SiteAttendance.Application.Abstractions.Storage;

/// <summary>
/// Result of a SPA file upload operation to R2 storage
/// </summary>
public class SpaUploadResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? PublicUrl { get; set; }
    public string? Key { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? ContentType { get; set; }

    public static SpaUploadResult SuccessResult(string publicUrl, string key, long fileSize, string contentType) =>
        new()
        {
            Success = true,
            PublicUrl = publicUrl,
            Key = key,
            FileSizeBytes = fileSize,
            ContentType = contentType
        };

    public static SpaUploadResult FailureResult(string errorMessage) =>
        new()
        {
            Success = false,
            ErrorMessage = errorMessage
        };
}
