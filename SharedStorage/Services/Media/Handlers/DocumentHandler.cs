using Microsoft.Extensions.Logging;
using SharedStorage.Services.BaseServices;
using SharedStorage.Models;

namespace SharedStorage.Services.Media.Handlers;

public interface IDocumentHandler
{
    Task<bool> CanHandleAsync(string fileName, string contentType);
    Task<DocumentProcessingResult> ProcessDocumentAsync(string containerName, string fileName, Stream content);
    Task<DocumentMetadata> GetDocumentMetadataAsync(Stream content, string fileName);
    Task<Stream> ConvertToTextAsync(Stream content, string fileName);
    Task<Stream> ConvertToPdfAsync(Stream content, string fileName);
}

public record DocumentProcessingResult(
    string OriginalBlobName,
    string ProcessedBlobName,
    DocumentMetadata Metadata,
    string? TextContent = null
);

public class DocumentHandler : IDocumentHandler
{
    private readonly IDocumentConversionService _documentConversionService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<DocumentHandler> _logger;

    private static readonly HashSet<string> SupportedDocumentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "text/plain",
        "text/csv",
        "application/rtf"
    };

    private static readonly HashSet<string> SupportedDocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".csv", ".rtf"
    };

    public DocumentHandler(
        IDocumentConversionService documentConversionService,
        IBlobStorageService blobStorageService,
        ILogger<DocumentHandler> logger)
    {
        _documentConversionService = documentConversionService ?? throw new ArgumentNullException(nameof(documentConversionService));
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<bool> CanHandleAsync(string fileName, string contentType)
    {
        var isSupportedContentType = SupportedDocumentTypes.Contains(contentType);
        var hasSupportedExtension = SupportedDocumentExtensions.Contains(Path.GetExtension(fileName));
        
        return Task.FromResult(isSupportedContentType || hasSupportedExtension);
    }

    public async Task<DocumentProcessingResult> ProcessDocumentAsync(string containerName, string fileName, Stream content)
    {
        _logger.LogInformation("Processing document: {FileName}", fileName);

        try
        {
            var metadata = await GetDocumentMetadataAsync(content, fileName);
            
            // Reset stream position
            content.Position = 0;

            // Upload original document
            await _blobStorageService.UploadBlobAsync(containerName, fileName, content);

            // Extract text content for indexing/searching
            content.Position = 0;
            var extractedText = await _documentConversionService.ExtractTextAsync(content, fileName);
            var textContent = string.Join("\n", extractedText);

            _logger.LogInformation("Successfully processed document: {FileName}", fileName);

            return new DocumentProcessingResult(
                fileName,
                fileName,
                metadata,
                textContent
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document: {FileName}", fileName);
            throw;
        }
    }

    public async Task<DocumentMetadata> GetDocumentMetadataAsync(Stream content, string fileName)
    {
        return await _documentConversionService.GetDocumentMetadataAsync(content, fileName);
    }

    public async Task<Stream> ConvertToTextAsync(Stream content, string fileName)
    {
        var result = await _documentConversionService.ConvertToTextAsync(content, fileName);
        return result.Content;
    }

    public async Task<Stream> ConvertToPdfAsync(Stream content, string fileName)
    {
        var result = await _documentConversionService.ConvertToPdfAsync(content, fileName);
        return result.Content;
    }
}