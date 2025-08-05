using Microsoft.Extensions.Logging;
using SharedStorage.Services.BaseServices;
using SharedStorage.Models;

namespace SharedStorage.Services.Media.Handlers;

public interface IVideoHandler
{
    Task<bool> CanHandleAsync(string fileName, string contentType);
    Task<VideoProcessingResult> ProcessVideoAsync(string containerName, string fileName, Stream content);
    Task<VideoMetadata> GetVideoMetadataAsync(Stream content, string fileName);
    Task<Stream> CreateThumbnailAsync(Stream content, string fileName, int maxWidth = 300, int maxHeight = 300);

    Task<Stream> ConvertToWebMAsync(Stream content, string fileName);
}

public record VideoProcessingResult(
    string OriginalBlobName,
    string ProcessedBlobName,
    string? ThumbnailBlobName,
    VideoMetadata Metadata
);

public record VideoMetadata(
    string FileName,
    string ContentType,
    long Size,
    int? Width = null,
    int? Height = null,
    string? Duration = null,
    string? Codec = null,
    double? FrameRate = null,
    string? Format = null
);

public class VideoHandler : IVideoHandler
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly IThumbnailService _thumbnailService;
    private readonly ILogger<VideoHandler> _logger;

    private static readonly HashSet<string> SupportedVideoTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "video/mp4", "video/avi", "video/mov", "video/wmv", "video/flv", "video/webm", "video/mkv", "video/3gp"
    };

    private static readonly HashSet<string> SupportedVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm", ".mkv", ".3gp"
    };

    public VideoHandler(
        IBlobStorageService blobStorageService,
        IThumbnailService thumbnailService,
        ILogger<VideoHandler> logger)
    {
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _thumbnailService = thumbnailService ?? throw new ArgumentNullException(nameof(thumbnailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<bool> CanHandleAsync(string fileName, string contentType)
    {
        var isSupportedContentType = SupportedVideoTypes.Contains(contentType);
        var hasSupportedExtension = SupportedVideoExtensions.Contains(Path.GetExtension(fileName));
        
        return Task.FromResult(isSupportedContentType || hasSupportedExtension);
    }

    public async Task<VideoProcessingResult> ProcessVideoAsync(string containerName, string fileName, Stream content)
    {
        _logger.LogInformation("Processing video: {FileName}", fileName);

        try
        {
            var metadata = await GetVideoMetadataAsync(content, fileName);
            
            string processedBlobName = fileName;
            string? thumbnailBlobName = null;

            // Reset stream position
            content.Position = 0;

            // Upload original video
            await _blobStorageService.UploadBlobAsync(containerName, processedBlobName, content);

            _logger.LogInformation("Successfully processed video: {FileName}", fileName);

            return new VideoProcessingResult(
                fileName,
                processedBlobName,
                thumbnailBlobName,
                metadata
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing video: {FileName}", fileName);
            throw;
        }
    }

    public Task<VideoMetadata> GetVideoMetadataAsync(Stream content, string fileName)
    {
        var contentType = GetContentTypeFromFileName(fileName);
        var size = content.Length;
        var format = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();

        // Basic metadata - in a real implementation, you'd use a library like FFMpegCore
        // to extract video metadata (width, height, duration, codec, frame rate)
        var metadata = new VideoMetadata(
            fileName,
            contentType,
            size,
            Width: null,  // Would need video processing library
            Height: null, // Would need video processing library
            Duration: null, // Would need video processing library
            Codec: null,
            FrameRate: null,
            Format: format
        );

        return Task.FromResult(metadata);
    }

    public Task<Stream> CreateThumbnailAsync(Stream content, string fileName, int maxWidth = 300, int maxHeight = 300)
    {
        // Placeholder implementation - in a real scenario, you'd extract a frame from the video
        // and create a thumbnail image
        _logger.LogWarning("Video thumbnail generation not implemented for: {FileName}", fileName);
        return Task.FromResult<Stream>(new MemoryStream());
    }

    public Task<Stream> ConvertToWebMAsync(Stream content, string fileName)
    {
        // Placeholder implementation - in a real scenario, you'd use FFMpeg or similar
        // to convert video to WebM format
        _logger.LogWarning("Video conversion to WebM not implemented for: {FileName}", fileName);
        content.Position = 0;
        return Task.FromResult(content);
    }

    private static string GetContentTypeFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".mp4" => "video/mp4",
            ".avi" => "video/avi",
            ".mov" => "video/mov",
            ".wmv" => "video/wmv",
            ".flv" => "video/flv",
            ".webm" => "video/webm",
            ".mkv" => "video/mkv",
            ".3gp" => "video/3gp",
            _ => "application/octet-stream"
        };
    }
}