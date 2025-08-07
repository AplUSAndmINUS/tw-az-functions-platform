using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedStorage.Models;
using SharedStorage.Services.Media;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Tests;

public class ImageSecurityTests
{
    private readonly ImageSecurityConfiguration _defaultConfig = new ImageSecurityConfiguration();
    
    [Fact]
    public void ImageSecurityConfiguration_HasSecureDefaults()
    {
        // Arrange & Act
        var config = new ImageSecurityConfiguration();
        
        // Assert
        Assert.Equal(8192, config.MaxImageWidth);
        Assert.Equal(8192, config.MaxImageHeight);
        Assert.Equal(256, config.MaxMemoryMB);
        Assert.Equal(50, config.MaxFileSizeMB);
        Assert.Equal(30, config.ProcessingTimeoutSeconds);
        Assert.True(config.AutoOrient);
        Assert.True(config.StripExifMetadata);
    }
    
    [Fact]
    public void ImageConversionService_ValidatesFileSizeLimits()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<ImageConversionService>();
        var options = Options.Create(new ImageSecurityConfiguration { MaxFileSizeMB = 1 }); // 1MB limit
        var service = new ImageConversionService(logger, options);
        
        // Create a mock stream that reports 2MB size
        var mockStream = new TestStream(2 * 1024 * 1024); // 2MB
        
        // Act & Assert
        var exception = Assert.ThrowsAsync<ValidationException>(
            async () => await service.ConvertToWebPAsync(mockStream));
        
        Assert.Contains("File size exceeds maximum allowed size", exception.Result.Message);
    }
    
    [Fact]
    public void ThumbnailService_ValidatesFileSizeLimits()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<ThumbnailService>();
        var options = Options.Create(new ImageSecurityConfiguration { MaxFileSizeMB = 1 }); // 1MB limit
        var service = new ThumbnailService(logger, options);
        
        // Create a mock stream that reports 2MB size
        var mockStream = new TestStream(2 * 1024 * 1024); // 2MB
        
        // Act & Assert
        var exception = Assert.ThrowsAsync<ValidationException>(
            async () => await service.GenerateWebPThumbnailAsync(mockStream));
        
        Assert.Contains("File size exceeds maximum allowed size", exception.Result.Message);
    }
}

/// <summary>
/// Test stream that simulates a stream of a specific size
/// </summary>
public class TestStream : Stream
{
    private readonly long _length;
    
    public TestStream(long length)
    {
        _length = length;
    }
    
    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => _length;
    public override long Position { get; set; }
    
    public override void Flush() { }
    
    public override int Read(byte[] buffer, int offset, int count)
    {
        // Return some dummy data for testing
        return Math.Min(count, (int)(Length - Position));
    }
    
    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                Position = offset;
                break;
            case SeekOrigin.Current:
                Position += offset;
                break;
            case SeekOrigin.End:
                Position = Length + offset;
                break;
        }
        return Position;
    }
    
    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }
    
    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
}