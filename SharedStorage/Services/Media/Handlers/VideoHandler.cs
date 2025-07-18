using Microsoft.Extensions.Logging;

namespace SharedStorage.Services.Media.Handlers;

public interface IVideoHandler
{
    Task<bool> CanHandleAsync(string fileName, string contentType);
    Task<VideoProcessingResult> ProcessVideoAsync(string containerName, string fileName, Stream content);
    Task<VideoMetadata> GetVideoMetadataAsync(Stream content, string fileName);
    Task<Stream> CreateThumbnailAsync(Stream content, string fileName, int maxWidth = 300, int maxHeight = 300);
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
    string? Format = null
);

public class VideoHandler : IVideoHandler
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly IThumbnailService _thumbnailService;
    private readonly ILogger<VideoHandler> _logger;

    private static readonly HashSet<string> SupportedVideoTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "video/mp4", "video/avi", "video/mov", "video/wmv", "video/flv", "video/webm"
    };

    private static readonly HashSet<string> SupportedVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm"
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

            // Create thumbnail from video if possible
            content.Position = 0;
            try
            {
                var thumbnailStream = await CreateThumbnailAsync(content, fileName);
                thumbnailBlobName = $"thumbnails/{Path.GetFileNameWithoutExtension(fileName)}_thumb.webp";
                await _blobStorageService.UploadBlobAsync(containerName, thumbnailBlobName, thumbnailStream);
                thumbnailStream.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not create thumbnail for video: {FileName}", fileName);
            }

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

        // For now, we don't extract video dimensions or duration
        // This could be enhanced with FFmpeg or similar video processing library
        var metadata = new VideoMetadata(
            fileName,
            contentType,
            size,
            null, // Width - would need video processing library
            null, // Height - would need video processing library
            null, // Duration - would need video processing library
            format
        );

        return Task.FromResult(metadata);
    }

    public Task<Stream> CreateThumbnailAsync(Stream content, string fileName, int maxWidth = 300, int maxHeight = 300)
    {
        // For now, we can't create thumbnails from video without additional libraries
        // This would typically use FFmpeg or similar to extract a frame
        // For the minimal implementation, we'll just return an empty stream
        // In a real implementation, this would extract a frame from the video
        _logger.LogWarning("Video thumbnail generation not implemented for: {FileName}", fileName);
        return Task.FromResult<Stream>(new MemoryStream());
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
            _ => "application/octet-stream"
        };
    }
}