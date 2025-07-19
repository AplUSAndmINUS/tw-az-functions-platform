using SharedStorage.Services;

namespace SharedStorage.Services.Media.Platforms;

/// <summary>
/// YouTube platform media adapter
/// </summary>
public class YouTubePlatformAdapter : IPlatformMediaAdapter
{
    private readonly IImageService _imageService;
    private readonly IThumbnailService _thumbnailService;

    public string PlatformName => "YouTube";

    public IEnumerable<string> SupportedMediaTypes => new[]
    {
        "image/jpeg", "image/png", "image/gif", "image/bmp",
        "video/mp4", "video/mov", "video/avi", "video/wmv", "video/flv", "video/webm"
    };

    public long MaxFileSizeBytes => 2L * 1024 * 1024 * 1024; // 2GB

    public YouTubePlatformAdapter(IImageService imageService, IThumbnailService thumbnailService)
    {
        _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        _thumbnailService = thumbnailService ?? throw new ArgumentNullException(nameof(thumbnailService));
    }

    public Task<bool> ValidateMediaAsync(Stream content, MediaMetadata metadata)
    {
        // Check file size
        if (metadata.Size > MaxFileSizeBytes)
            return Task.FromResult(false);

        // Check content type
        if (!SupportedMediaTypes.Contains(metadata.ContentType))
            return Task.FromResult(false);

        // Check dimensions for images/videos (YouTube supports various formats)
        if (metadata.ContentType.StartsWith("image/") || metadata.ContentType.StartsWith("video/"))
        {
            if (metadata.Width is < 240 or > 3840 || metadata.Height is < 240 or > 2160)
                return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public async Task<MediaProcessingResult> ProcessMediaForPlatformAsync(Stream content, MediaMetadata metadata)
    {
        var processedBlobName = metadata.FileName;
        string? thumbnailBlobName = null;

        // For images, ensure they meet YouTube's requirements
        if (metadata.ContentType.StartsWith("image/"))
        {
            // Convert to JPEG for thumbnails
            if (!metadata.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
            {
                var conversionResult = await _imageService.ConvertToJpegAsync(content);
                processedBlobName = Path.ChangeExtension(metadata.FileName, ".jpg");
                content = conversionResult.Content;
            }

            // Create YouTube-optimized thumbnail (16:9 format)
            content.Position = 0;
            var thumbnailStream = await _thumbnailService.GenerateJpegThumbnailAsync(content, 1280, 720); // 16:9 HD
            thumbnailBlobName = $"thumbnails/youtube_{Path.GetFileNameWithoutExtension(metadata.FileName)}_thumb.jpg";
        }

        return new MediaProcessingResult(
            metadata.FileName,
            processedBlobName,
            thumbnailBlobName,
            metadata with { FileName = processedBlobName }
        );
    }

    public Task<PlatformMediaRequirements> GetPlatformRequirementsAsync()
    {
        var requirements = new PlatformMediaRequirements(
            MaxWidth: 3840,
            MaxHeight: 2160,
            MinWidth: 240,
            MinHeight: 240,
            MaxDuration: TimeSpan.FromHours(12),
            AllowedFormats: SupportedMediaTypes,
            AspectRatioRequirement: "16:9 landscape recommended, supports various ratios"
        );

        return Task.FromResult(requirements);
    }
}