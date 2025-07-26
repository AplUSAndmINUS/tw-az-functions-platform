namespace SharedStorage.Services.Media;

public interface IDocumentConversionService
{
    Task<Stream> ConvertToPdfAsync(Stream documentStream, string fileName);
    Task<Stream> ConvertToImageAsync(Stream documentStream, string fileName, int pageNumber = 1);
    Task<int> GetPageCountAsync(Stream documentStream, string fileName);
    Task<string> ExtractTextAsync(Stream documentStream, string fileName);
    bool IsSupportedFormat(string fileName);
}