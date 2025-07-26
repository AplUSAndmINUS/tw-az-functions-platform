using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SharedStorage.Services.BaseServices;
using SharedStorage.Models;

namespace SharedStorage.Services.Media.Handlers;

public interface IImageHandler
{
    Task<bool> CanHandleAsync(string fileName, string contentType);
    Task<ImageProcessingResult> ProcessImageAsync(string containerName, string fileName, Stream content);
    Task<ImageMetadata> GetImageMetadataAsync(Stream content, string fileName);
    Task<Stream> CreateThumbnailAsync(Stream content, string fileName, int maxWidth = 300, int maxHeight = 300);
    Task<Stream> ConvertToWebPAsync(Stream content, string fileName);
}

public record ImageProcessingResult(
    string OriginalBlobName,
    string ProcessedBlobName,
    string? ThumbnailBlobName,
    ImageMetadata Metadata
);

public record ImageMetadata(
    string FileName,
    string ContentType,
    long Size,
    int Width,
    int Height,
    string Format
);

public class ImageHandler : IImageHandler
{
    private readonly IImageService _imageService;
    private readonly IThumbnailService _thumbnailService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<ImageHandler> _logger;

    private static readonly HashSet<string> SupportedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp"
    };

    private static readonly HashSet<string> SupportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
    };

    public ImageHandler(
        IImageService imageService,
        IThumbnailService thumbnailService,
        IBlobStorageService blobStorageService,
        ILogger<ImageHandler> logger)
    {
        _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        _thumbnailService = thumbnailService ?? throw new ArgumentNullException(nameof(thumbnailService));
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<bool> CanHandleAsync(string fileName, string contentType)
    {
        var isSupportedContentType = SupportedImageTypes.Contains(contentType);
        var hasSupportedExtension = SupportedImageExtensions.Contains(Path.GetExtension(fileName));
        
        return Task.FromResult(isSupportedContentType || hasSupportedExtension);
    }

    public async Task<ImageProcessingResult> ProcessImageAsync(string containerName, string fileName, Stream content)
    {
        _logger.LogInformation("Processing image: {FileName}", fileName);

        try
        {
            var metadata = await GetImageMetadataAsync(content, fileName);
            
            string processedBlobName = fileName;
            string? thumbnailBlobName = null;

            // Reset stream position
            content.Position = 0;

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

            _logger.LogInformation("Successfully processed image: {FileName}", fileName);

            return new ImageProcessingResult(
                fileName,
                processedBlobName,
                thumbnailBlobName,
                metadata
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image: {FileName}", fileName);
            throw;
        }
    }

    public async Task<ImageMetadata> GetImageMetadataAsync(Stream content, string fileName)
    {
        var contentType = GetContentTypeFromFileName(fileName);
        var size = content.Length;

        try
        {
            content.Position = 0;
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(content);
            
            return new ImageMetadata(
                fileName,
                contentType,
                size,
                image.Width,
                image.Height,
                Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant()
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read image metadata for {FileName}", fileName);
            
            // Return basic metadata if image can't be read
            return new ImageMetadata(
                fileName,
                contentType,
                size,
                0,
                0,
                Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant()
            );
        }
    }

    public async Task<Stream> CreateThumbnailAsync(Stream content, string fileName, int maxWidth = 300, int maxHeight = 300)
    {
        var thumbnailResult = await _thumbnailService.GenerateWebPThumbnailAsync(content);
        return thumbnailResult.Content;
    }

    public async Task<Stream> ConvertToWebPAsync(Stream content, string fileName)
    {
        var conversionResult = await _imageService.ConvertToWebPAsync(content);
        return conversionResult.Content;
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
            _ => "application/octet-stream"
        };
    }
}