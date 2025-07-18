using SharedStorage.Services.Media;
using Utils.Constants;
using Xunit;

namespace Tests;

public class MediaServiceContentReferencesTests
{
    [Theory]
    [InlineData("image/jpeg", AssetType.Images)]
    [InlineData("image/png", AssetType.Images)]
    [InlineData("video/mp4", AssetType.Video)]
    [InlineData("video/avi", AssetType.Video)]
    [InlineData("application/pdf", AssetType.Data)]
    [InlineData("text/plain", AssetType.Data)]
    [InlineData("application/unknown", AssetType.Media)]
    public void GetAssetTypeFromContentType_ReturnsCorrectAssetType(string contentType, AssetType expectedType)
    {
        // Act
        var result = MediaServiceContentReferences.GetAssetTypeFromContentType(contentType);

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
        var result = MediaServiceContentReferences.GetAssetTypeFromFileName(fileName);

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
        var result = MediaServiceContentReferences.IsMediaFile(fileName, contentType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void MediaCategories_ContainExpectedTypes()
    {
        // Assert
        Assert.Contains("image/jpeg", MediaServiceContentReferences.MediaCategories.ImageContentTypes);
        Assert.Contains("video/mp4", MediaServiceContentReferences.MediaCategories.VideoContentTypes);
        Assert.Contains("application/pdf", MediaServiceContentReferences.MediaCategories.DocumentContentTypes);
        
        Assert.Contains(".jpg", MediaServiceContentReferences.MediaCategories.ImageExtensions);
        Assert.Contains(".mp4", MediaServiceContentReferences.MediaCategories.VideoExtensions);
        Assert.Contains(".pdf", MediaServiceContentReferences.MediaCategories.DocumentExtensions);
    }

    [Fact]
    public void ContentTypes_ContainExpectedConstants()
    {
        // Assert
        Assert.Equal("image/jpeg", MediaServiceContentReferences.ContentTypes.Images.Jpeg);
        Assert.Equal("video/mp4", MediaServiceContentReferences.ContentTypes.Videos.Mp4);
        Assert.Equal("application/pdf", MediaServiceContentReferences.ContentTypes.Documents.Pdf);
    }

    [Fact]
    public void FileExtensions_ContainExpectedConstants()
    {
        // Assert
        Assert.Equal(".jpg", MediaServiceContentReferences.FileExtensions.Images.Jpg);
        Assert.Equal(".mp4", MediaServiceContentReferences.FileExtensions.Videos.Mp4);
        Assert.Equal(".pdf", MediaServiceContentReferences.FileExtensions.Documents.Pdf);
    }
}