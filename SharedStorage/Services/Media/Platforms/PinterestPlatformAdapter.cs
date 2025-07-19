using SharedStorage.Services;

namespace SharedStorage.Services.Media.Platforms;

/// <summary>
/// Pinterest platform media adapter
/// </summary>
public class PinterestPlatformAdapter : IPlatformMediaAdapter
{
    private readonly IImageService _imageService;
    private readonly IThumbnailService _thumbnailService;

    public string PlatformName => "Pinterest";

    public IEnumerable<string> SupportedMediaTypes => new[]
    {
        "image/jpeg", "image/png", "image/gif", "image/webp",
        "video/mp4", "video/mov"
    };

    public long MaxFileSizeBytes => 32 * 1024 * 1024; // 32MB

    public PinterestPlatformAdapter(IImageService imageService, IThumbnailService thumbnailService)
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

        // Check dimensions for images (Pinterest prefers vertical/portrait orientation)
        if (metadata.ContentType.StartsWith("image/"))
        {
            if (metadata.Width is < 100 or > 2000 || metadata.Height is < 100 or > 4000)
                return Task.FromResult(false);
                
            // Pinterest works best with aspect ratios between 2:3 and 1:3.5
            if (metadata.Width.HasValue && metadata.Height.HasValue)
            {
                var aspectRatio = (double)metadata.Width.Value / metadata.Height.Value;
                if (aspectRatio > 0.67) // Less vertical than 2:3
                    return Task.FromResult(false);
            }
        }

        return Task.FromResult(true);
    }

    public async Task<MediaProcessingResult> ProcessMediaForPlatformAsync(Stream content, MediaMetadata metadata)
    {
        var processedBlobName = metadata.FileName;
        string? thumbnailBlobName = null;

        // For images, ensure they meet Pinterest's requirements
        if (metadata.ContentType.StartsWith("image/"))
        {
            // Convert to WebP for better quality and compression on Pinterest
            if (!metadata.ContentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase))
            {
                var conversionResult = await _imageService.ConvertToWebPAsync(content);
                processedBlobName = Path.ChangeExtension(metadata.FileName, ".webp");
                content = conversionResult.Content;
            }

            // Create Pinterest-optimized thumbnail (vertical format)
            content.Position = 0;
            var thumbnailStream = await _thumbnailService.GenerateWebPThumbnailAsync(content, 236, 354); // 2:3 aspect ratio
            thumbnailBlobName = $"thumbnails/pinterest_{Path.GetFileNameWithoutExtension(metadata.FileName)}_thumb.webp";
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
            MaxWidth: 2000,
            MaxHeight: 4000,
            MinWidth: 100,
            MinHeight: 100,
            MaxDuration: TimeSpan.FromMinutes(15),
            AllowedFormats: SupportedMediaTypes,
            AspectRatioRequirement: "2:3 to 1:3.5 vertical orientation recommended"
        );

        return Task.FromResult(requirements);
    }
}