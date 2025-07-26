using Microsoft.Extensions.Logging;

namespace SharedStorage.Services.Media;

public class DocumentConversionService : IDocumentConversionService
{
    private readonly ILogger<DocumentConversionService> _logger;
    private static readonly string[] SupportedFormats = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };

    public DocumentConversionService(ILogger<DocumentConversionService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Stream> ConvertToPdfAsync(Stream documentStream, string fileName)
    {
        _logger.LogInformation("Converting document {FileName} to PDF", fileName);
        
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        if (!IsSupportedFormat(fileName))
        {
            throw new NotSupportedException($"File format {extension} is not supported for PDF conversion");
        }

        if (extension == ".pdf")
        {
            // Already a PDF, return as-is
            documentStream.Position = 0;
            return documentStream;
        }

        // For this implementation, we'll simulate conversion
        // In a real implementation, you would use libraries like:
        // - Aspose.Words for Word documents
        // - Aspose.Cells for Excel documents
        // - Aspose.Slides for PowerPoint documents
        // - LibreOffice headless for general conversion
        
        await Task.Delay(100); // Simulate processing time
        
        documentStream.Position = 0;
        return documentStream; // Return original for now
    }

    public async Task<Stream> ConvertToImageAsync(Stream documentStream, string fileName, int pageNumber = 1)
    {
        _logger.LogInformation("Converting document {FileName} page {PageNumber} to image", fileName, pageNumber);
        
        if (!IsSupportedFormat(fileName))
        {
            throw new NotSupportedException($"File format is not supported for image conversion");
        }

        // Simulate conversion
        await Task.Delay(100);
        
        documentStream.Position = 0;
        return documentStream;
    }

    public async Task<int> GetPageCountAsync(Stream documentStream, string fileName)
    {
        _logger.LogInformation("Getting page count for document {FileName}", fileName);
        
        if (!IsSupportedFormat(fileName))
        {
            throw new NotSupportedException($"File format is not supported");
        }

        // Simulate page count detection
        await Task.Delay(50);
        
        return 1; // Default to 1 page
    }

    public async Task<string> ExtractTextAsync(Stream documentStream, string fileName)
    {
        _logger.LogInformation("Extracting text from document {FileName}", fileName);
        
        if (!IsSupportedFormat(fileName))
        {
            throw new NotSupportedException($"File format is not supported for text extraction");
        }

        // Simulate text extraction
        await Task.Delay(100);
        
        return "Sample extracted text"; // Default text
    }

    public bool IsSupportedFormat(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return SupportedFormats.Contains(extension);
    }
}