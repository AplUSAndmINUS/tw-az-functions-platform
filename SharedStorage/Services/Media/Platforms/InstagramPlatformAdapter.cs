using SharedStorage.Services;

namespace SharedStorage.Services.Media.Platforms;

/// <summary>
/// Instagram platform media adapter
/// </summary>
public class InstagramPlatformAdapter : IPlatformMediaAdapter
{
    private readonly IImageService _imageService;
    private readonly IThumbnailService _thumbnailService;

    public string PlatformName => "Instagram";

    public IEnumerable<string> SupportedMediaTypes => new[]
    {
        "image/jpeg", "image/png",
        "video/mp4", "video/mov"
    };

    public long MaxFileSizeBytes => 50 * 1024 * 1024; // 50MB

    public InstagramPlatformAdapter(IImageService imageService, IThumbnailService thumbnailService)
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

        // Check dimensions for images (Instagram prefers square or 4:5 aspect ratio)
        if (metadata.ContentType.StartsWith("image/"))
        {
            if (metadata.Width is < 320 or > 1080 || metadata.Height is < 320 or > 1350)
                return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public async Task<MediaProcessingResult> ProcessMediaForPlatformAsync(Stream content, MediaMetadata metadata)
    {
        var processedBlobName = metadata.FileName;
        string? thumbnailBlobName = null;

        // For images, ensure they meet Instagram's requirements
        if (metadata.ContentType.StartsWith("image/"))
        {
            // Convert to JPEG for better compression
            if (!metadata.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
            {
                var conversionResult = await _imageService.ConvertToOptimizedFormatAsync(content, "jpeg");
                processedBlobName = Path.ChangeExtension(metadata.FileName, ".jpg");
                content = conversionResult.Content;
            }

            // Create Instagram-optimized thumbnail (square format) using WebP for better compression
            content.Position = 0;
            var thumbnailStream = await _thumbnailService.GenerateWebPThumbnailAsync(content, maxSize: 150, quality: 85);
            thumbnailBlobName = $"thumbnails/instagram_{Path.GetFileNameWithoutExtension(metadata.FileName)}_thumb.webp";
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
            MaxHeight: 1350,
            MinWidth: 320,
            MinHeight: 320,
            MaxDuration: TimeSpan.FromMinutes(10),
            AllowedFormats: SupportedMediaTypes,
            AspectRatioRequirement: "1:1 (square) or 4:5 (portrait) recommended"
        );

        return Task.FromResult(requirements);
    }
}