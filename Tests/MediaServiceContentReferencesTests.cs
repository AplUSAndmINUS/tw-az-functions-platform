using SharedStorage.Services.Media;
using Utils.Constants;
using Xunit;
using static SharedStorage.Services.Media.MediaCategories;

namespace Tests;

public class MediaServiceContentReferencesTests
{
    [Theory]
    [InlineData("image/jpeg", AssetType.Images)]
    [InlineData("image/png", AssetType.Images)]
    [InlineData("video/mp4", AssetType.Video)]
    [InlineData("video/x-msvideo", AssetType.Video)]
    [InlineData("application/pdf", AssetType.Data)]
    [InlineData("text/plain", AssetType.Data)]
    [InlineData("application/unknown", AssetType.Media)]
    public void GetAssetTypeFromContentType_ReturnsCorrectAssetType(string contentType, AssetType expectedType)
    {
        // Act
        var result = GetAssetTypeFromContentType(contentType);

        // Assert
        Assert.Equal(expectedType, result);
    }

    [Theory]
    [InlineData("image.jpg", AssetType.Images)]
    [InlineData("image.png", AssetType.Images)]
    [InlineData("video.mp4", AssetType.Video)]
    [InlineData("video.avi", AssetType.Video)]
    [InlineData("document.pdf", AssetType.Data)]
    [InlineData("text.txt", AssetType.Data)]
    [InlineData("unknown.xyz", AssetType.Media)]
    public void GetAssetTypeFromFileName_ReturnsCorrectAssetType(string fileName, AssetType expectedType)
    {
        // Act
        var result = GetAssetTypeFromFileName(fileName);

        // Assert
        Assert.Equal(expectedType, result);
    }

    [Theory]
    [InlineData("image.jpg", "image/jpeg", true)]
    [InlineData("video.mp4", "video/mp4", true)]
    [InlineData("document.pdf", "application/pdf", false)]
    [InlineData("text.txt", "text/plain", false)]
    public void IsMediaFile_ReturnsCorrectResult(string fileName, string contentType, bool expected)
    {
        // Act
        var result = IsMediaFile(fileName, contentType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void MediaCategories_ContainExpectedTypes()
    {
        // Assert
        Assert.Contains("image/jpeg", MediaCategories.ImageContentTypes);
        Assert.Contains("video/mp4", MediaCategories.VideoContentTypes);
        Assert.Contains("application/pdf", MediaCategories.DocumentContentTypes);
        
        Assert.Contains(".jpg", MediaCategories.ImageExtensions);
        Assert.Contains(".mp4", MediaCategories.VideoExtensions);
        Assert.Contains(".pdf", MediaCategories.DocumentExtensions);
    }

    [Fact]
    public void ContentTypes_ContainExpectedConstants()
    {
        // Assert
        Assert.Equal("image/jpeg", ContentTypes.Images.Jpeg);
        Assert.Equal("video/mp4", ContentTypes.Videos.Mp4);
        Assert.Equal("application/pdf", ContentTypes.Documents.Pdf);
    }

    [Fact]
    public void FileExtensions_ContainExpectedConstants()
    {
        // Assert
        Assert.Equal(".jpg", FileExtensions.Images.Jpg);
        Assert.Equal(".mp4", FileExtensions.Videos.Mp4);
        Assert.Equal(".pdf", FileExtensions.Documents.Pdf);
    }
}