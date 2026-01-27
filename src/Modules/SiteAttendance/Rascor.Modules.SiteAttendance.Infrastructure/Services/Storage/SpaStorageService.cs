using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rascor.Modules.SiteAttendance.Application.Abstractions.Storage;
using Rascor.Modules.SiteAttendance.Infrastructure.Configuration;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Services.Storage;

/// <summary>
/// Cloudflare R2 storage service for SPA (Site Photo Attendance) files with tenant isolation.
/// All files are stored under {tenant-id}/spa/{folder}/{filename} structure.
/// </summary>
public class SpaStorageService : ISpaStorageService, IDisposable
{
    private readonly IAmazonS3 _s3Client;
    private readonly SpaStorageSettings _settings;
    private readonly ILogger<SpaStorageService> _logger;

    private const string ImagesFolder = "spa/images";
    private const string SignaturesFolder = "spa/signatures";

    public SpaStorageService(
        IOptions<SpaStorageSettings> settings,
        ILogger<SpaStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var config = new AmazonS3Config
        {
            ServiceURL = _settings.Endpoint,
            ForcePathStyle = true
        };

        _s3Client = new AmazonS3Client(
            _settings.AccessKeyId,
            _settings.SecretAccessKey,
            config);
    }

    public async Task<SpaUploadResult> UploadImageAsync(
        Guid tenantId,
        Guid spaId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate size
            if (content.Length > _settings.MaxImageSizeBytes)
            {
                var maxMb = _settings.MaxImageSizeBytes / 1024 / 1024;
                var actualMb = content.Length / 1024 / 1024;
                return SpaUploadResult.FailureResult(
                    $"Image size ({actualMb}MB) exceeds maximum allowed ({maxMb}MB)");
            }

            // Validate content type
            if (!_settings.AllowedImageTypes.Contains(contentType.ToLower()))
            {
                return SpaUploadResult.FailureResult(
                    $"Invalid content type '{contentType}'. Allowed types: {string.Join(", ", _settings.AllowedImageTypes)}");
            }

            var extension = GetExtensionFromContentType(contentType);
            var fileName = $"{spaId}{extension}";
            var key = BuildKey(tenantId, ImagesFolder, fileName);

            _logger.LogInformation("Uploading SPA image to R2: {Key}", key);

            var request = new PutObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = key,
                InputStream = content,
                ContentType = contentType,
                DisablePayloadSigning = true,
                UseChunkEncoding = false
            };

            await _s3Client.PutObjectAsync(request, cancellationToken);

            var publicUrl = GeneratePublicUrl(tenantId, ImagesFolder, fileName);

            _logger.LogInformation("Successfully uploaded SPA image: {Url}", publicUrl);

            return SpaUploadResult.SuccessResult(publicUrl, key, content.Length, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image for SPA {SpaId}", spaId);
            return SpaUploadResult.FailureResult($"Image upload failed: {ex.Message}");
        }
    }

    public async Task<SpaUploadResult> UploadSignatureAsync(
        Guid tenantId,
        Guid spaId,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate size
            if (content.Length > _settings.MaxSignatureSizeBytes)
            {
                var maxKb = _settings.MaxSignatureSizeBytes / 1024;
                var actualKb = content.Length / 1024;
                return SpaUploadResult.FailureResult(
                    $"Signature size ({actualKb}KB) exceeds maximum allowed ({maxKb}KB)");
            }

            var fileName = $"{spaId}.png";
            var key = BuildKey(tenantId, SignaturesFolder, fileName);
            const string contentType = "image/png";

            _logger.LogInformation("Uploading SPA signature to R2: {Key}", key);

            var request = new PutObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = key,
                InputStream = content,
                ContentType = contentType,
                DisablePayloadSigning = true,
                UseChunkEncoding = false
            };

            await _s3Client.PutObjectAsync(request, cancellationToken);

            var publicUrl = GeneratePublicUrl(tenantId, SignaturesFolder, fileName);

            _logger.LogInformation("Successfully uploaded SPA signature: {Url}", publicUrl);

            return SpaUploadResult.SuccessResult(publicUrl, key, content.Length, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload signature for SPA {SpaId}", spaId);
            return SpaUploadResult.FailureResult($"Signature upload failed: {ex.Message}");
        }
    }

    public async Task DeleteSpaFilesAsync(
        Guid tenantId,
        Guid spaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var spaIdString = spaId.ToString();
            var prefix = $"{tenantId}/spa/";

            _logger.LogInformation("Deleting files for SPA {SpaId} with prefix {Prefix}",
                spaId, prefix);

            var listRequest = new ListObjectsV2Request
            {
                BucketName = _settings.BucketName,
                Prefix = prefix
            };

            var response = await _s3Client.ListObjectsV2Async(listRequest, cancellationToken);

            var keysToDelete = response.S3Objects
                .Where(obj => obj.Key.Contains(spaIdString))
                .Select(obj => new KeyVersion { Key = obj.Key })
                .ToList();

            if (keysToDelete.Count > 0)
            {
                var deleteRequest = new DeleteObjectsRequest
                {
                    BucketName = _settings.BucketName,
                    Objects = keysToDelete
                };

                await _s3Client.DeleteObjectsAsync(deleteRequest, cancellationToken);
                _logger.LogInformation("Deleted {Count} files for SPA {SpaId}",
                    keysToDelete.Count, spaId);
            }
            else
            {
                _logger.LogInformation("No files found to delete for SPA {SpaId}", spaId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete files for SPA {SpaId}", spaId);
            throw;
        }
    }

    public string GeneratePublicUrl(Guid tenantId, string folder, string fileName)
    {
        var publicUrl = _settings.PublicUrl.TrimEnd('/');
        var key = BuildKey(tenantId, folder, fileName);
        var encodedKey = Uri.EscapeDataString(key).Replace("%2F", "/");
        return $"{publicUrl}/{encodedKey}";
    }

    private static string BuildKey(Guid tenantId, string folder, string fileName)
    {
        return $"{tenantId}/{folder}/{fileName}";
    }

    private static string GetExtensionFromContentType(string contentType)
    {
        return contentType.ToLower() switch
        {
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
    }

    public void Dispose()
    {
        _s3Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
