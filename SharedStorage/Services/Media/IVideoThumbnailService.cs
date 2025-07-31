namespace SharedStorage.Services.Media;

public interface IVideoThumbnailService
{
    Task<Stream> GenerateThumbnailAsync(Stream videoStream, string fileName, TimeSpan? timeOffset = null);
    Task<IEnumerable<Stream>> GenerateMultipleThumbnailsAsync(Stream videoStream, string fileName, int count = 3);
    Task<(int width, int height, double duration)> GetVideoMetadataAsync(Stream videoStream, string fileName);
    bool IsSupportedFormat(string fileName);
}