using SharedStorage.Services;

namespace SharedStorage.Services.Media.Platforms;

/// <summary>
/// TikTok platform media adapter
/// </summary>
public class TikTokPlatformAdapter : IPlatformMediaAdapter
{
    private readonly IImageService _imageService;
    private readonly IThumbnailService _thumbnailService;

    public string PlatformName => "TikTok";

    public IEnumerable<string> SupportedMediaTypes => new[]
    {
        "image/jpeg", "image/png",
        "video/mp4", "video/mov", "video/webm"
    };

    public long MaxFileSizeBytes => 500 * 1024 * 1024; // 500MB

    public TikTokPlatformAdapter(IImageService imageService, IThumbnailService thumbnailService)
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

        // Check dimensions for images/videos (TikTok is primarily vertical 9:16)
        if (metadata.ContentType.StartsWith("image/") || metadata.ContentType.StartsWith("video/"))
        {
            if (metadata.Width is < 540 or > 1080 || metadata.Height is < 960 or > 1920)
                return Task.FromResult(false);
                
            // TikTok strongly prefers 9:16 aspect ratio
            if (metadata.Width.HasValue && metadata.Height.HasValue)
            {
                var aspectRatio = (double)metadata.Width.Value / metadata.Height.Value;
                if (Math.Abs(aspectRatio - 9.0/16.0) > 0.1) // Allow some tolerance
                    return Task.FromResult(false);
            }
        }

        return Task.FromResult(true);
    }

    public async Task<MediaProcessingResult> ProcessMediaForPlatformAsync(Stream content, MediaMetadata metadata)
    {
        var processedBlobName = metadata.FileName;
        string? thumbnailBlobName = null;

        // For images, ensure they meet TikTok's requirements
        if (metadata.ContentType.StartsWith("image/"))
        {
            // Convert to JPEG format preferred by TikTok
            var conversionResult = await _imageService.ConvertToOptimizedFormatAsync(content, "jpeg");
            processedBlobName = Path.ChangeExtension(metadata.FileName, ".jpg");
            content = conversionResult.Content;

            // Create TikTok-optimized thumbnail (vertical format) using WebP for better compression
            content.Position = 0;
            var thumbnailStream = await _thumbnailService.GenerateWebPThumbnailAsync(content, maxSize: 480, quality: 85); // Vertical format
            thumbnailBlobName = $"thumbnails/tiktok_{Path.GetFileNameWithoutExtension(metadata.FileName)}_thumb.webp";
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
            MaxWidth: 1080,
            MaxHeight: 1920,
            MinWidth: 540,
            MinHeight: 960,
            MaxDuration: TimeSpan.FromMinutes(10),
            AllowedFormats: SupportedMediaTypes,
            AspectRatioRequirement: "9:16 vertical orientation required"
        );

        return Task.FromResult(requirements);
    }
}