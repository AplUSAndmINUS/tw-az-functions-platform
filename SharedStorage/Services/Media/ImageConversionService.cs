using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedStorage.Models;
using System.ComponentModel.DataAnnotations;

namespace SharedStorage.Services.Media;

public interface IImageService
{
  Task<ImageConversionResult> ConvertToWebPAsync(Stream input);
  Task<ImageConversionResult> ConvertToJpegAsync(Stream input);
}

public record ImageConversionResult(Stream Content, int Width, int Height, string Format);

public class ImageConversionService : IImageService
{
  private readonly ILogger<ImageConversionService> _logger;
  private readonly ImageSecurityConfiguration _securityConfig;

  public ImageConversionService(ILogger<ImageConversionService> logger, IOptions<ImageSecurityConfiguration> securityOptions)
  {
    _logger = logger;
    _securityConfig = securityOptions.Value;
  }

  public async Task<ImageConversionResult> ConvertToWebPAsync(Stream input)
  {
    if (input == null)
    {
      _logger.LogError("Input stream is null. Cannot convert to WebP.");
      throw new ArgumentNullException(nameof(input), "Input stream cannot be null.");
    }

    ValidateInputStreamAsync(input);

    try
    {
      // Load the image from the input stream with timeout
      _logger.LogInformation("Loading image from input stream for WebP conversion.");
      input.Position = 0; // Reset stream position to the beginning
      
      using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_securityConfig.ProcessingTimeoutSeconds));
      using var image = await Image.LoadAsync(input, cts.Token);
      
      ValidateImageDimensions(image);
      
      // Handle EXIF orientation securely
      if (_securityConfig.AutoOrient)
      {
        image.Mutate(x => x.AutoOrient());
      }

      _logger.LogInformation("Image loaded successfully. Dimensions: {Width}x{Height}", image.Width, image.Height);

      if (image.Width < 600 || image.Height < 600)
      {
        _logger.LogInformation("Resizing image for WebP conversion due to insufficient dimensions: {Width}x{Height}.", image.Width, image.Height);
        double scaleFactor = 600.0 / Math.Min(image.Width, image.Height);
        image.Mutate(x => x.Resize(new ResizeOptions
        {
          Size = new Size((int)Math.Round(image.Width * scaleFactor), (int)Math.Round(image.Height * scaleFactor)),
          Mode = ResizeMode.Max
        }));
      }

      var resizedWidth = image.Width;
      var resizedHeight = image.Height;

      image.Metadata.HorizontalResolution = 96;
      image.Metadata.VerticalResolution = 96;
      // Create a memory stream to hold the converted image
      var output = new MemoryStream();

      // Save the image as WebP with quality settings
      var encoder = new WebpEncoder { Quality = 85 };
      await image.SaveAsWebpAsync(output, encoder);
      
      _logger.LogInformation("WebP conversion completed successfully. Size: {Size} bytes", output.Length);
      
      output.Position = 0; // Reset stream position to the beginning for reading
      return new ImageConversionResult(output, resizedWidth, resizedHeight, "webp");
    }

    catch (Exception ex)
    {
      _logger.LogError(ex, "Error converting image to WebP format.");
      throw new InvalidOperationException("Failed to convert image to WebP format.", ex);
    }
  }

  public async Task<ImageConversionResult> ConvertToJpegAsync(Stream input)
  {
    if (input == null)
    {
      _logger.LogError("Input stream is null. Cannot convert to JPEG.");
      throw new ArgumentNullException(nameof(input), "Input stream cannot be null.");
    }

    ValidateInputStreamAsync(input);

    try
    {
      // Load the image from the input stream with timeout
      _logger.LogInformation("Loading image from input stream for JPEG conversion.");
      input.Position = 0; // Reset stream position to the beginning
      
      using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_securityConfig.ProcessingTimeoutSeconds));
      using var image = await Image.LoadAsync(input, cts.Token);
      
      ValidateImageDimensions(image);
      
      // Handle EXIF orientation securely
      if (_securityConfig.AutoOrient)
      {
        image.Mutate(x => x.AutoOrient());
      }

      _logger.LogInformation("Image loaded successfully. Dimensions: {Width}x{Height}", image.Width, image.Height);

      var resizedWidth = image.Width;
      var resizedHeight = image.Height;

      // Create a memory stream to hold the converted image
      var output = new MemoryStream();

      // Save the image as JPEG with quality settings
      var encoder = new JpegEncoder { Quality = 85 };
      await image.SaveAsJpegAsync(output, encoder);
      
      _logger.LogInformation("JPEG conversion completed successfully. Size: {Size} bytes", output.Length);
      
      output.Position = 0; // Reset stream position to the beginning for reading
      return new ImageConversionResult(output, resizedWidth, resizedHeight, "jpeg");
    }

    catch (Exception ex)
    {
      _logger.LogError(ex, "Error converting image to JPEG format.");
      throw new InvalidOperationException("Failed to convert image to JPEG format.", ex);
    }
  }
  
  /// <summary>
  /// Validates input stream for security constraints
  /// </summary>
  private void ValidateInputStreamAsync(Stream input)
  {
    // Check file size limit
    var maxSizeBytes = _securityConfig.MaxFileSizeMB * 1024 * 1024;
    if (input.Length > maxSizeBytes)
    {
      _logger.LogWarning("Input stream size {Size} exceeds maximum allowed size {MaxSize}", 
        input.Length, maxSizeBytes);
      throw new ValidationException($"File size exceeds maximum allowed size of {_securityConfig.MaxFileSizeMB} MB.");
    }
    
    _logger.LogInformation("Input stream validated. Size: {Size} bytes", input.Length);
  }
  
  /// <summary>
  /// Validates image dimensions for security constraints
  /// </summary>
  private void ValidateImageDimensions(Image image)
  {
    if (image.Width > _securityConfig.MaxImageWidth || image.Height > _securityConfig.MaxImageHeight)
    {
      _logger.LogWarning("Image dimensions {Width}x{Height} exceed maximum allowed dimensions {MaxWidth}x{MaxHeight}", 
        image.Width, image.Height, _securityConfig.MaxImageWidth, _securityConfig.MaxImageHeight);
      throw new ValidationException($"Image dimensions exceed maximum allowed size of {_securityConfig.MaxImageWidth}x{_securityConfig.MaxImageHeight} pixels.");
    }
    
    _logger.LogInformation("Image dimensions validated. Size: {Width}x{Height}", image.Width, image.Height);
  }
}