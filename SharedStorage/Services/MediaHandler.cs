using SharedStorage.Services.BaseServices;
using SharedStorage.Services.Media;
using Utils.Constants;

namespace SharedStorage.Services;

public record MediaMetadata(
    string FileName,
    string ContentType,
    long Size,
    int? Width = null,
    int? Height = null,
    string? Duration = null
);

public record MediaProcessingResult(
    string OriginalBlobName,
    string ProcessedBlobName,
    string? ThumbnailBlobName,
    MediaMetadata Metadata
);

public interface IMediaHandler
{
    Task<bool> IsMediaFileAsync(string fileName, string contentType);
    Task<MediaProcessingResult> ProcessMediaAsync(string containerName, string fileName, Stream content);
    Task<MediaMetadata> GetMediaMetadataAsync(Stream content, string fileName);
    Task<Stream> CreateThumbnailAsync(Stream content, string fileName, int maxWidth = 300, int maxHeight = 300);
}

public class MediaHandler : IMediaHandler
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly IImageService _imageService;
    private readonly IThumbnailService _thumbnailService;

    private static readonly HashSet<string> SupportedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp"
    };

    private static readonly HashSet<string> SupportedVideoTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "video/mp4", "video/avi", "video/mov", "video/wmv", "video/flv", "video/webm"
    };

    public MediaHandler(
        IBlobStorageService blobStorageService,
        IImageService imageService,
        IThumbnailService thumbnailService)
    {
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        _thumbnailService = thumbnailService ?? throw new ArgumentNullException(nameof(thumbnailService));
    }

    public Task<bool> IsMediaFileAsync(string fileName, string contentType)
    {
        var isImage = SupportedImageTypes.Contains(contentType);
        var isVideo = SupportedVideoTypes.Contains(contentType);
        var hasMediaExtension = Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or
            ".mp4" or ".avi" or ".mov" or ".wmv" or ".flv" or ".webm" => true,
            _ => false
        };

        return Task.FromResult(isImage || isVideo || hasMediaExtension);
    }

    public async Task<MediaProcessingResult> ProcessMediaAsync(string containerName, string fileName, Stream content)
    {
        var metadata = await GetMediaMetadataAsync(content, fileName);
        
        string processedBlobName = fileName;
        string? thumbnailBlobName = null;

        // Reset stream position
        content.Position = 0;

        // Process images
        if (SupportedImageTypes.Contains(metadata.ContentType))
        {
            // Convert to WebP if not already
            if (!metadata.ContentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase))
            {
                var conversionResult = await _imageService.ConvertToWebPAsync(content);
                processedBlobName = Path.ChangeExtension(fileName, ".webp");
                
                // Upload processed image
                await _blobStorageService.UploadBlobAsync(containerName, processedBlobName, conversionResult.Content);
                conversionResult.Content.Dispose();
            }
            else
            {
                // Upload original
                await _blobStorageService.UploadBlobAsync(containerName, processedBlobName, content);
            }

            // Create thumbnail
            content.Position = 0;
            var thumbnailStream = await CreateThumbnailAsync(content, fileName);
            thumbnailBlobName = $"thumbnails/{Path.GetFileNameWithoutExtension(fileName)}_thumb.webp";
            await _blobStorageService.UploadBlobAsync(containerName, thumbnailBlobName, thumbnailStream);
            thumbnailStream.Dispose();
        }
        else
        {
            // For non-image files, just upload as-is
            await _blobStorageService.UploadBlobAsync(containerName, processedBlobName, content);
        }

        return new MediaProcessingResult(
            fileName,
            processedBlobName,
            thumbnailBlobName,
            metadata
        );
    }

    public async Task<MediaMetadata> GetMediaMetadataAsync(Stream content, string fileName)
    {
        var contentType = GetContentTypeFromFileName(fileName);
        var size = content.Length;

        int? width = null;
        int? height = null;
        string? duration = null;

        // For images, try to get dimensions
        if (SupportedImageTypes.Contains(contentType))
        {
            try
            {
                content.Position = 0;
                using var image = await SixLabors.ImageSharp.Image.LoadAsync(content);
                width = image.Width;
                height = image.Height;
            }
            catch
            {
                // If we can't read the image, that's okay
            }
        }

        return new MediaMetadata(fileName, contentType, size, width, height, duration);
    }

    public async Task<Stream> CreateThumbnailAsync(Stream content, string fileName, int maxWidth = 300, int maxHeight = 300)
    {
        var thumbnailResult = await _thumbnailService.GenerateWebPThumbnailAsync(content);
        return thumbnailResult.Content;
    }

    private static string GetContentTypeFromFileName(string fileName)
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
            _ => "application/octet-stream"
        };
    }
}