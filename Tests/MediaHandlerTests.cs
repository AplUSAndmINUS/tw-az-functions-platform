using SharedStorage.Services;
using SharedStorage.Services.BaseServices;
using SharedStorage.Services.Media;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests;

public class MediaHandlerTests
{
    private readonly Mock<IBlobStorageService> _mockBlobService;
    private readonly Mock<IImageService> _mockImageService;
    private readonly Mock<IThumbnailService> _mockThumbnailService;
    private readonly IMediaHandler _handler;

    public MediaHandlerTests()
    {
        _mockBlobService = new Mock<IBlobStorageService>();
        _mockImageService = new Mock<IImageService>();
        _mockThumbnailService = new Mock<IThumbnailService>();
        
        _handler = new MediaHandler(
            _mockBlobService.Object,
            _mockImageService.Object,
            _mockThumbnailService.Object);
    }

    [Theory]
    [InlineData("image.jpg", "image/jpeg")]
    [InlineData("image.png", "image/png")]
    [InlineData("video.mp4", "video/mp4")]
    [InlineData("image.webp", "image/webp")]
    public async Task IsMediaFileAsync_WithMediaTypes_ReturnsTrue(string fileName, string contentType)
    {
        // Act
        var result = await _handler.IsMediaFileAsync(fileName, contentType);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("document.pdf", "application/pdf")]
    [InlineData("text.txt", "text/plain")]
    [InlineData("unknown.xyz", "application/octet-stream")]
    public async Task IsMediaFileAsync_WithNonMediaTypes_ReturnsFalse(string fileName, string contentType)
    {
        // Act
        var result = await _handler.IsMediaFileAsync(fileName, contentType);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetMediaMetadataAsync_WithImageFile_ReturnsCorrectMetadata()
    {
        // Arrange
        var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // Mock PNG header
        using var stream = new MemoryStream(imageBytes);

        // Act
        var metadata = await _handler.GetMediaMetadataAsync(stream, "test.png");

        // Assert
        Assert.Equal("test.png", metadata.FileName);
        Assert.Equal("image/png", metadata.ContentType);
        Assert.Equal(imageBytes.Length, metadata.Size);
    }

    [Fact]
    public async Task CreateThumbnailAsync_CallsThumbnailService()
    {
        // Arrange
        var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        using var stream = new MemoryStream(imageBytes);
        var thumbnailStream = new MemoryStream();
        var thumbnailResult = new ThumbnailResult(thumbnailStream, 300, 300, "webp");
        
        _mockThumbnailService
            .Setup(x => x.GenerateWebPThumbnailAsync(It.IsAny<Stream>()))
            .ReturnsAsync(thumbnailResult);

        // Act
        var result = await _handler.CreateThumbnailAsync(stream, "test.png");

        // Assert
        Assert.Equal(thumbnailStream, result);
        _mockThumbnailService.Verify(x => x.GenerateWebPThumbnailAsync(It.IsAny<Stream>()), Times.Once);
    }
}