using SharedStorage.Services.BaseServices;
using SharedStorage.Services.Media;

namespace SharedStorage.Services.Media.Handlers;

public abstract class MediaHandler
{
    protected readonly IBlobStorageService _blobStorageService;

    protected MediaHandler(IBlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
    }

    public abstract Task<bool> CanHandleAsync(string fileName, string contentType);
    public abstract Task<MediaProcessingResult> ProcessAsync(string containerName, string fileName, Stream content);
    public abstract Task<MediaMetadata> GetMetadataAsync(Stream content, string fileName);

    protected static string GetContentTypeFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".mp4" => "video/mp4",
            ".avi" => "video/avi",
            ".mov" => "video/mov",
            ".wmv" => "video/wmv",
            ".flv" => "video/flv",
            ".webm" => "video/webm",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            _ => "application/octet-stream"
        };
    }
}

public record MediaProcessingResult(
    string OriginalBlobName,
    string ProcessedBlobName,
    string? ThumbnailBlobName,
    MediaMetadata Metadata
);

public record MediaMetadata(
    string FileName,
    string ContentType,
    long Size,
    int? Width = null,
    int? Height = null,
    string? Duration = null,
    int? PageCount = null
);