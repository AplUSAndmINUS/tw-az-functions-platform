using SharedStorage.Services;
using SharedStorage.Services.Media;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests;

public class DocumentConversionServiceTests
{
    private readonly IDocumentConversionService _service;

    public DocumentConversionServiceTests()
    {
        _service = new DocumentConversionService();
    }

    [Theory]
    [InlineData("document.pdf", "application/pdf")]
    [InlineData("document.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("document.txt", "text/plain")]
    [InlineData("document.csv", "text/csv")]
    public async Task IsDocumentAsync_WithSupportedTypes_ReturnsTrue(string fileName, string contentType)
    {
        // Act
        var result = await _service.IsDocumentAsync(fileName, contentType);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("image.jpg", "image/jpeg")]
    [InlineData("video.mp4", "video/mp4")]
    [InlineData("unknown.xyz", "application/octet-stream")]
    public async Task IsDocumentAsync_WithUnsupportedTypes_ReturnsFalse(string fileName, string contentType)
    {
        // Act
        var result = await _service.IsDocumentAsync(fileName, contentType);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ConvertToTextAsync_WithTextFile_ReturnsTextContent()
    {
        // Arrange
        var text = "Hello, World!";
        var textBytes = System.Text.Encoding.UTF8.GetBytes(text);
        using var stream = new MemoryStream(textBytes);

        // Act
        var result = await _service.ConvertToTextAsync(stream, "test.txt");

        // Assert
        Assert.Equal("text/plain", result.OutputFormat);
        Assert.Equal("test.txt", result.OutputFileName);
        Assert.True(result.Size > 0);
        
        using var reader = new StreamReader(result.Content);
        var content = await reader.ReadToEndAsync();
        Assert.Equal(text, content);
    }

    [Fact]
    public async Task GetDocumentMetadataAsync_WithTextFile_ReturnsCorrectMetadata()
    {
        // Arrange
        var text = "This is a test document with multiple words that should be counted for page estimation.";
        var textBytes = System.Text.Encoding.UTF8.GetBytes(text);
        using var stream = new MemoryStream(textBytes);

        // Act
        var metadata = await _service.GetDocumentMetadataAsync(stream, "test.txt");

        // Assert
        Assert.Equal("test.txt", metadata.FileName);
        Assert.Equal("text/plain", metadata.ContentType);
        Assert.Equal(textBytes.Length, metadata.Size);
        Assert.Equal(1, metadata.PageCount); // Should be 1 page for a short text
    }

    [Fact]
    public async Task ExtractTextAsync_WithTextFile_ReturnsTextLines()
    {
        // Arrange
        var text = "Line 1\nLine 2\nLine 3";
        var textBytes = System.Text.Encoding.UTF8.GetBytes(text);
        using var stream = new MemoryStream(textBytes);

        // Act
        var lines = await _service.ExtractTextAsync(stream, "test.txt");

        // Assert
        var linesList = lines.ToList();
        Assert.Equal(3, linesList.Count);
        Assert.Equal("Line 1", linesList[0]);
        Assert.Equal("Line 2", linesList[1]);
        Assert.Equal("Line 3", linesList[2]);
    }
}