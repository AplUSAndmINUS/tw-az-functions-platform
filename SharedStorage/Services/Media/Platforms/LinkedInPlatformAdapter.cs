using SharedStorage.Services;

namespace SharedStorage.Services.Media.Platforms;

/// <summary>
/// LinkedIn platform media adapter
/// </summary>
public class LinkedInPlatformAdapter : IPlatformMediaAdapter
{
    private readonly IImageService _imageService;
    private readonly IThumbnailService _thumbnailService;

    public string PlatformName => "LinkedIn";

    public IEnumerable<string> SupportedMediaTypes => new[]
    {
        "image/jpeg", "image/png", "image/gif",
        "video/mp4", "video/mov", "video/avi", "video/wmv"
    };

    public long MaxFileSizeBytes => 200 * 1024 * 1024; // 200MB

    public LinkedInPlatformAdapter(IImageService imageService, IThumbnailService thumbnailService)
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
            if (metadata.Width is < 400 or > 7680 || metadata.Height is < 400 or > 4320)
                return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public async Task<MediaProcessingResult> ProcessMediaForPlatformAsync(Stream content, MediaMetadata metadata)
    {
        var processedBlobName = metadata.FileName;
        string? thumbnailBlobName = null;

        // For images, ensure they meet LinkedIn's requirements
        if (metadata.ContentType.StartsWith("image/"))
        {
            // Keep original format for LinkedIn as it supports multiple formats well
            // Create professional thumbnail
            content.Position = 0;
            var thumbnailStream = await _thumbnailService.GenerateJpegThumbnailAsync(content, 400, 400);
            thumbnailBlobName = $"thumbnails/linkedin_{Path.GetFileNameWithoutExtension(metadata.FileName)}_thumb.jpg";
        }

        return new MediaProcessingResult(
            metadata.FileName,
            processedBlobName,
            thumbnailBlobName,
            metadata
        );
    }

    public Task<PlatformMediaRequirements> GetPlatformRequirementsAsync()
    {
        var requirements = new PlatformMediaRequirements(
            MaxWidth: 7680,
            MaxHeight: 4320,
            MinWidth: 400,
            MinHeight: 400,
            MaxDuration: TimeSpan.FromMinutes(30),
            AllowedFormats: SupportedMediaTypes,
            AspectRatioRequirement: "16:9 landscape or 1.91:1 recommended for posts"
        );

        return Task.FromResult(requirements);
    }
}