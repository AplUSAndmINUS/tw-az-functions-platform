using Microsoft.Extensions.Logging;
using SharedStorage.Extensions;
using SharedStorage.Models;
using SharedStorage.Services.BaseServices;
using SharedStorage.Services.Media.Handlers;

namespace SharedStorage.Services.Media;

public interface IMediaService
{
    Task<MediaReference> ProcessMediaAsync(Stream mediaStream, string fileName, string containerName);
    Task<MediaEntity> CreateMediaEntityAsync(Stream mediaStream, string fileName, string containerName);
    Task<bool> DeleteMediaAsync(string containerName, string blobName);
    Task<MediaEntity?> GetMediaEntityAsync(string id);
}

public class MediaService : IMediaService
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly IDocumentHandler _documentHandler;
    private readonly IImageHandler _imageHandler;
    private readonly IVideoHandler _videoHandler;
    private readonly ILogger<MediaService> _logger;

    public MediaService(
        IBlobStorageService blobStorageService,
        IDocumentHandler documentHandler,
        IImageHandler imageHandler,
        IVideoHandler videoHandler,
        ILogger<MediaService> logger)
    {
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _documentHandler = documentHandler ?? throw new ArgumentNullException(nameof(documentHandler));
        _imageHandler = imageHandler ?? throw new ArgumentNullException(nameof(imageHandler));
        _videoHandler = videoHandler ?? throw new ArgumentNullException(nameof(videoHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MediaReference> ProcessMediaAsync(Stream mediaStream, string fileName, string containerName)
    {
        _logger.LogInformation("Processing media file: {FileName}", fileName);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        try
        {
            if (IsImageFile(extension))
            {
                return await _imageHandler.ProcessImageAsync(containerName, fileName, mediaStream);
            }
            else if (IsVideoFile(extension))
            {
                return await _videoHandler.ProcessVideoAsync(containerName, fileName, mediaStream);
            }
            else if (IsDocumentFile(extension))
            {
                return await _documentHandler.ProcessDocumentAsync(containerName, fileName, mediaStream);
            }
            else
            {
                // For unsupported files, just upload as-is
                var mediaReference = await _blobStorageService.UploadBlobAsync(containerName, fileName, mediaStream);
                return mediaReference;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing media file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<MediaEntity> CreateMediaEntityAsync(Stream mediaStream, string fileName, string containerName)
    {
        _logger.LogInformation("Creating media entity for file: {FileName}", fileName);

        try
        {
            var mediaReference = await ProcessMediaAsync(mediaStream, fileName, containerName);
            
            var entity = new MediaEntity("media", Guid.NewGuid().ToString())
            {
                OriginalFileName = fileName,
                BlobName = mediaReference.OriginalBlobName,
                ThumbnailBlobName = mediaReference.ThumbnailBlobName,
                ContainerName = containerName,
                CdnUrl = mediaReference.CdnUrl,
                ThumbnailCdnUrl = mediaReference.ThumbnailCdnUrl,
                ProcessingStatus = "Completed",
                ProcessedDate = DateTime.UtcNow,
                FileSize = mediaStream.Length,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            // Set media type based on file extension
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            entity.MimeType = GetMimeTypeFromExtension(extension);

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating media entity for file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<bool> DeleteMediaAsync(string containerName, string blobName)
    {
        try
        {
            _logger.LogInformation("Deleting media: {ContainerName}/{BlobName}", containerName, blobName);
            await _blobStorageService.DeleteBlobAsync(containerName, blobName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting media: {ContainerName}/{BlobName}", containerName, blobName);
            return false;
        }
    }

    public async Task<MediaEntity?> GetMediaEntityAsync(string id)
    {
        // This would typically retrieve from table storage
        // For now, returning null as placeholder
        await Task.CompletedTask;
        return null;
    }

    private static bool IsImageFile(string extension)
    {
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" or ".svg";
    }

    private static bool IsVideoFile(string extension)
    {
        return extension is ".mp4" or ".avi" or ".mov" or ".mkv" or ".webm" or ".flv" or ".wmv";
    }

    private static bool IsDocumentFile(string extension)
    {
        return extension is ".pdf" or ".doc" or ".docx" or ".xls" or ".xlsx" or ".ppt" or ".pptx" or ".txt";
    }

    private static string GetMimeTypeFromExtension(string extension)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            ".mkv" => "video/x-matroska",
            ".webm" => "video/webm",
            ".flv" => "video/x-flv",
            ".wmv" => "video/x-ms-wmv",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}