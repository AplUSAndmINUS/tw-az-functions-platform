namespace SharedStorage.Services.Media;

public interface IDocumentConversionService
{
    Task<Stream> ConvertToPdfAsync(Stream documentStream, string fileName);
    Task<Stream> ConvertToImageAsync(Stream documentStream, string fileName, int pageNumber = 1);
    Task<int> GetPageCountAsync(Stream documentStream, string fileName);
    Task<IEnumerable<string>> ExtractTextAsync(Stream documentStream, string fileName);
    Task<DocumentMetadata> GetDocumentMetadataAsync(Stream content, string fileName);
    Task<DocumentConversionResult> ConvertToTextAsync(Stream content, string fileName);
    Task<bool> IsDocumentAsync(string fileName, string contentType);
    bool IsSupportedFormat(string fileName);
}

public record DocumentMetadata(
    string FileName,
    string ContentType,
    long Size,
    int PageCount
);

public record DocumentConversionResult(
    string OutputFormat,
    string OutputFileName, 
    long Size,
    Stream Content
);