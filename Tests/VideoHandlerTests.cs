using Microsoft.Extensions.Logging;
using Moq;
using SharedStorage.Services;
using SharedStorage.Services.BaseServices;
using SharedStorage.Services.Media;
using SharedStorage.Services.Media.Handlers;
using Xunit;

namespace Tests;

public class VideoHandlerTests
{
    private readonly Mock<IBlobStorageService> _blobStorageService;
    private readonly Mock<IThumbnailService> _thumbnailService;
    private readonly Mock<ILogger<VideoHandler>> _logger;
    private readonly VideoHandler _videoHandler;

    public VideoHandlerTests()
    {
        _blobStorageService = new Mock<IBlobStorageService>();
        _thumbnailService = new Mock<IThumbnailService>();
        _logger = new Mock<ILogger<VideoHandler>>();
        
        _videoHandler = new VideoHandler(_blobStorageService.Object, _thumbnailService.Object, _logger.Object);
    }

    [Theory]
    [InlineData("video.mp4", "video/mp4")]
    [InlineData("video.avi", "video/avi")]
    [InlineData("video.mov", "video/mov")]
    [InlineData("video.wmv", "video/wmv")]
    [InlineData("video.flv", "video/flv")]
    [InlineData("video.webm", "video/webm")]
    public async Task CanHandleAsync_WithVideoTypes_ReturnsTrue(string fileName, string contentType)
    {
        // Act
        var result = await _videoHandler.CanHandleAsync(fileName, contentType);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("image.jpg", "image/jpeg")]
    [InlineData("document.pdf", "application/pdf")]
    [InlineData("text.txt", "text/plain")]
    public async Task CanHandleAsync_WithNonVideoTypes_ReturnsFalse(string fileName, string contentType)
    {
        // Act
        var result = await _videoHandler.CanHandleAsync(fileName, contentType);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetVideoMetadataAsync_WithValidVideo_ReturnsMetadata()
    {
        // Arrange
        var fileName = "test.mp4";
        var content = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });

        // Act
        var metadata = await _videoHandler.GetVideoMetadataAsync(content, fileName);

        // Assert
        Assert.Equal(fileName, metadata.FileName);
        Assert.Equal("video/mp4", metadata.ContentType);
        Assert.Equal(5, metadata.Size);
        Assert.Equal("mp4", metadata.Format);
    }

    [Fact]
    public async Task ProcessVideoAsync_WithValidVideo_ReturnsProcessingResult()
    {
        // Arrange
        var containerName = "test-container";
        var fileName = "test.mp4";
        var content = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });

        // Act
        var result = await _videoHandler.ProcessVideoAsync(containerName, fileName, content);

        // Assert
        Assert.Equal(fileName, result.OriginalBlobName);
        Assert.Equal(fileName, result.ProcessedBlobName);
        Assert.NotNull(result.Metadata);
        Assert.Equal(fileName, result.Metadata.FileName);
        Assert.Equal("video/mp4", result.Metadata.ContentType);
        
        // Verify blob upload was called
        _blobStorageService.Verify(x => x.UploadBlobAsync(containerName, fileName, It.IsAny<Stream>()), Times.Once);
    }
}