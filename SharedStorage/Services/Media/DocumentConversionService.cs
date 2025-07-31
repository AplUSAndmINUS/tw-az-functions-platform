using Microsoft.Extensions.Logging;

namespace SharedStorage.Services.Media;

public class DocumentConversionService : IDocumentConversionService
{
    private readonly ILogger<DocumentConversionService> _logger;
    private static readonly string[] SupportedFormats = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".csv" };

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

    public async Task<IEnumerable<string>> ExtractTextAsync(Stream documentStream, string fileName)
    {
        _logger.LogInformation("Extracting text from document {FileName}", fileName);
        
        if (!IsSupportedFormat(fileName))
        {
            throw new NotSupportedException($"File format is not supported for text extraction");
        }

        // For text files, read line by line
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension == ".txt" || extension == ".csv")
        {
            documentStream.Position = 0;
            using var reader = new StreamReader(documentStream, leaveOpen: true);
            var lines = new List<string>();
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lines.Add(line);
            }
            return lines;
        }

        // Simulate text extraction for other formats
        await Task.Delay(100);
        
        return new[] { "Sample extracted text" }; // Default text
    }

    public async Task<DocumentMetadata> GetDocumentMetadataAsync(Stream content, string fileName)
    {
        _logger.LogInformation("Getting metadata for document {FileName}", fileName);
        
        if (!IsSupportedFormat(fileName))
        {
            throw new NotSupportedException($"File format is not supported");
        }

        // Simulate metadata extraction
        await Task.Delay(50);
        
        var pageCount = await GetPageCountAsync(content, fileName);
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return new DocumentMetadata(
            fileName, 
            GetMimeType(extension), 
            content.Length, 
            pageCount
        );
    }

    public async Task<DocumentConversionResult> ConvertToTextAsync(Stream content, string fileName)
    {
        _logger.LogInformation("Converting document {FileName} to text", fileName);
        
        var textLines = await ExtractTextAsync(content, fileName);
        var text = string.Join(System.Environment.NewLine, textLines);
        var textBytes = System.Text.Encoding.UTF8.GetBytes(text);
        var textStream = new MemoryStream(textBytes);

        return new DocumentConversionResult(
            "text/plain",
            fileName,
            textBytes.Length,
            textStream
        );
    }

    public Task<bool> IsDocumentAsync(string fileName, string contentType)
    {
        var isSupported = IsSupportedFormat(fileName);
        
        // Also check by content type
        var supportedContentTypes = new[]
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.ms-powerpoint",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "text/plain",
            "text/csv"
        };

        var contentTypeSupported = supportedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);
        
        return Task.FromResult(isSupported || contentTypeSupported);
    }

    public bool IsSupportedFormat(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return SupportedFormats.Contains(extension);
    }

    private static string GetMimeType(string extension)
    {
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            _ => "application/octet-stream"
        };
    }
}