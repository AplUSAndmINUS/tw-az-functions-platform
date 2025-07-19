using SharedStorage.Services;

namespace SharedStorage.Services.Media.Platforms;

/// <summary>
/// Facebook platform media adapter
/// </summary>
public class FacebookPlatformAdapter : IPlatformMediaAdapter
{
    private readonly IImageService _imageService;
    private readonly IThumbnailService _thumbnailService;

    public string PlatformName => "Facebook";

    public IEnumerable<string> SupportedMediaTypes => new[]
    {
        "image/jpeg", "image/png", "image/gif",
        "video/mp4", "video/mov", "video/avi"
    };

    public long MaxFileSizeBytes => 100 * 1024 * 1024; // 100MB

    public FacebookPlatformAdapter(IImageService imageService, IThumbnailService thumbnailService)
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

        // Check dimensions for images
        if (metadata.ContentType.StartsWith("image/"))
        {
            if (metadata.Width is < 200 or > 8000 || metadata.Height is < 200 or > 8000)
                return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public async Task<MediaProcessingResult> ProcessMediaForPlatformAsync(Stream content, MediaMetadata metadata)
    {
        var processedBlobName = metadata.FileName;
        string? thumbnailBlobName = null;

        // For images, ensure they meet Facebook's requirements
        if (metadata.ContentType.StartsWith("image/"))
        {
            // Convert to JPEG if not already
            if (!metadata.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
            {
                var conversionResult = await _imageService.ConvertToJpegAsync(content);
                processedBlobName = Path.ChangeExtension(metadata.FileName, ".jpg");
                content = conversionResult.Content;
            }

            // Create thumbnail
            content.Position = 0;
            var thumbnailStream = await _thumbnailService.GenerateJpegThumbnailAsync(content, 320, 320);
            thumbnailBlobName = $"thumbnails/facebook_{Path.GetFileNameWithoutExtension(metadata.FileName)}_thumb.jpg";
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
            MaxWidth: 8000,
            MaxHeight: 8000,
            MinWidth: 200,
            MinHeight: 200,
            MaxDuration: TimeSpan.FromMinutes(60),
            AllowedFormats: SupportedMediaTypes,
            AspectRatioRequirement: "1:1 to 16:9 recommended"
        );

        return Task.FromResult(requirements);
    }
}