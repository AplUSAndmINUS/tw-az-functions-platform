using SharedStorage.Services.Media.Handlers;
using SharedStorage.Services;
using SharedStorage.Services.BaseServices;
using SharedStorage.Services.Media;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests;

public class MediaHandlersTests
{
    private readonly Mock<IDocumentConversionService> _mockDocumentService;
    private readonly Mock<IBlobStorageService> _mockBlobService;
    private readonly Mock<ILogger<DocumentHandler>> _mockDocumentLogger;
    private readonly DocumentHandler _documentHandler;

    private readonly Mock<IImageService> _mockImageService;
    private readonly Mock<IThumbnailService> _mockThumbnailService;
    private readonly Mock<ILogger<ImageHandler>> _mockImageLogger;
    private readonly ImageHandler _imageHandler;

    private readonly Mock<ILogger<VideoHandler>> _mockVideoLogger;
    private readonly VideoHandler _videoHandler;

    public MediaHandlersTests()
    {
        // Document handler setup
        _mockDocumentService = new Mock<IDocumentConversionService>();
        _mockBlobService = new Mock<IBlobStorageService>();
        _mockDocumentLogger = new Mock<ILogger<DocumentHandler>>();
        
        _documentHandler = new DocumentHandler(
            _mockDocumentService.Object,
            _mockBlobService.Object,
            _mockDocumentLogger.Object
        );

        // Image handler setup
        _mockImageService = new Mock<IImageService>();
        _mockThumbnailService = new Mock<IThumbnailService>();
        _mockImageLogger = new Mock<ILogger<ImageHandler>>();
        
        _imageHandler = new ImageHandler(
            _mockImageService.Object,
            _mockThumbnailService.Object,
            _mockBlobService.Object,
            _mockImageLogger.Object
        );

        // Video handler setup
        _mockVideoLogger = new Mock<ILogger<VideoHandler>>();
        _videoHandler = new VideoHandler(
            _mockBlobService.Object,
            _mockThumbnailService.Object,
            _mockVideoLogger.Object
        );
    }

    [Theory]
    [InlineData("document.pdf", "application/pdf")]
    [InlineData("document.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("document.txt", "text/plain")]
    [InlineData("document.csv", "text/csv")]
    public async Task DocumentHandler_CanHandleAsync_ShouldReturnTrueForSupportedTypes(string fileName, string contentType)
    {
        // Act
        var result = await _documentHandler.CanHandleAsync(fileName, contentType);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("image.jpg", "image/jpeg")]
    [InlineData("image.png", "image/png")]
    [InlineData("image.webp", "image/webp")]
    [InlineData("image.gif", "image/gif")]
    public async Task ImageHandler_CanHandleAsync_ShouldReturnTrueForSupportedTypes(string fileName, string contentType)
    {
        // Act
        var result = await _imageHandler.CanHandleAsync(fileName, contentType);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("video.mp4", "video/mp4")]
    [InlineData("video.avi", "video/avi")]
    [InlineData("video.mov", "video/mov")]
    [InlineData("video.webm", "video/webm")]
    public async Task VideoHandler_CanHandleAsync_ShouldReturnTrueForSupportedTypes(string fileName, string contentType)
    {
        // Act
        var result = await _videoHandler.CanHandleAsync(fileName, contentType);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("video.mp4", "video/mp4")]
    [InlineData("audio.mp3", "audio/mp3")]
    [InlineData("unknown.xyz", "application/octet-stream")]
    public async Task DocumentHandler_CanHandleAsync_ShouldReturnFalseForUnsupportedTypes(string fileName, string contentType)
    {
        // Act
        var result = await _documentHandler.CanHandleAsync(fileName, contentType);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("image.jpg", "image/jpeg")]
    [InlineData("audio.mp3", "audio/mp3")]
    [InlineData("unknown.xyz", "application/octet-stream")]
    public async Task VideoHandler_CanHandleAsync_ShouldReturnFalseForUnsupportedTypes(string fileName, string contentType)
    {
        // Act
        var result = await _videoHandler.CanHandleAsync(fileName, contentType);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("video.mp4", "video/mp4")]
    [InlineData("audio.mp3", "audio/mp3")]
    [InlineData("unknown.xyz", "application/octet-stream")]
    public async Task ImageHandler_CanHandleAsync_ShouldReturnFalseForUnsupportedTypes(string fileName, string contentType)
    {
        // Act
        var result = await _imageHandler.CanHandleAsync(fileName, contentType);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DocumentHandler_ProcessDocumentAsync_ShouldCallServices()
    {
        // Arrange
        var fileName = "test.txt";
        var containerName = "documents";
        var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("test content"));
        
        var metadata = new DocumentMetadata(fileName, "text/plain", content.Length, 1);
        _mockDocumentService.Setup(x => x.GetDocumentMetadataAsync(It.IsAny<Stream>(), fileName))
            .ReturnsAsync(metadata);
        _mockDocumentService.Setup(x => x.ExtractTextAsync(It.IsAny<Stream>(), fileName))
            .ReturnsAsync(new[] { "test content" });

        // Act
        var result = await _documentHandler.ProcessDocumentAsync(containerName, fileName, content);

        // Assert
        Assert.Equal(fileName, result.OriginalBlobName);
        Assert.Equal(fileName, result.ProcessedBlobName);
        Assert.Equal("test content", result.TextContent);
        _mockBlobService.Verify(x => x.UploadBlobAsync(containerName, fileName, It.IsAny<Stream>()), Times.Once);
    }

    [Fact]
    public async Task VideoHandler_ProcessVideoAsync_ShouldCallServices()
    {
        // Arrange
        var fileName = "test.mp4";
        var containerName = "videos";
        var content = new MemoryStream(new byte[1024]);

        // Act
        var result = await _videoHandler.ProcessVideoAsync(containerName, fileName, content);

        // Assert
        Assert.Equal(fileName, result.OriginalBlobName);
        Assert.Equal(fileName, result.ProcessedBlobName);
        Assert.Null(result.ThumbnailBlobName);
        Assert.Equal(fileName, result.Metadata.FileName);
        Assert.Equal("video/mp4", result.Metadata.ContentType);
        _mockBlobService.Verify(x => x.UploadBlobAsync(containerName, fileName, It.IsAny<Stream>()), Times.Once);
    }
}