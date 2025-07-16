using System.Text;

namespace SharedStorage.Services;

public record DocumentConversionResult(
    Stream Content,
    string OutputFormat,
    string OutputFileName,
    long Size
);

public record DocumentMetadata(
    string FileName,
    string ContentType,
    long Size,
    int? PageCount = null,
    string? Title = null,
    string? Author = null
);

public interface IDocumentConversionService
{
    Task<bool> IsDocumentAsync(string fileName, string contentType);
    Task<DocumentConversionResult> ConvertToPdfAsync(Stream input, string fileName);
    Task<DocumentConversionResult> ConvertToTextAsync(Stream input, string fileName);
    Task<DocumentMetadata> GetDocumentMetadataAsync(Stream content, string fileName);
    Task<IEnumerable<string>> ExtractTextAsync(Stream content, string fileName);
}

public class DocumentConversionService : IDocumentConversionService
{
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

    public Task<bool> IsDocumentAsync(string fileName, string contentType)
    {
        var isSupportedContentType = SupportedDocumentTypes.Contains(contentType);
        var hasSupportedExtension = SupportedDocumentExtensions.Contains(Path.GetExtension(fileName));
        
        return Task.FromResult(isSupportedContentType || hasSupportedExtension);
    }

    public async Task<DocumentConversionResult> ConvertToPdfAsync(Stream input, string fileName)
    {
        // For this basic implementation, we'll focus on text files
        // In a real implementation, you'd use libraries like iTextSharp, PdfSharpCore, or call external services
        
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        if (extension == ".pdf")
        {
            // Already PDF, return as-is
            input.Position = 0;
            var pdfContent = new MemoryStream();
            await input.CopyToAsync(pdfContent);
            pdfContent.Position = 0;
            
            return new DocumentConversionResult(
                pdfContent,
                "application/pdf",
                fileName,
                pdfContent.Length
            );
        }
        
        if (extension == ".txt")
        {
            // Convert text to PDF (simplified implementation)
            input.Position = 0;
            using var reader = new StreamReader(input);
            var text = await reader.ReadToEndAsync();
            
            // This is a placeholder - in reality you'd use a PDF library
            var pdfBytes = Encoding.UTF8.GetBytes($"PDF Content: {text}");
            var pdfStream = new MemoryStream(pdfBytes);
            
            return new DocumentConversionResult(
                pdfStream,
                "application/pdf",
                Path.ChangeExtension(fileName, ".pdf"),
                pdfStream.Length
            );
        }
        
        throw new NotSupportedException($"Conversion to PDF is not supported for file type: {extension}");
    }

    public async Task<DocumentConversionResult> ConvertToTextAsync(Stream input, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        if (extension == ".txt")
        {
            // Already text, return as-is
            input.Position = 0;
            var textContent = new MemoryStream();
            await input.CopyToAsync(textContent);
            textContent.Position = 0;
            
            return new DocumentConversionResult(
                textContent,
                "text/plain",
                fileName,
                textContent.Length
            );
        }
        
        // Extract text from other formats
        var extractedText = await ExtractTextAsync(input, fileName);
        var allText = string.Join("\n", extractedText);
        var textBytes = Encoding.UTF8.GetBytes(allText);
        var textStream = new MemoryStream(textBytes);
        
        return new DocumentConversionResult(
            textStream,
            "text/plain",
            Path.ChangeExtension(fileName, ".txt"),
            textStream.Length
        );
    }

    public async Task<DocumentMetadata> GetDocumentMetadataAsync(Stream content, string fileName)
    {
        var contentType = GetContentTypeFromFileName(fileName);
        var size = content.Length;
        
        int? pageCount = null;
        string? title = null;
        string? author = null;
        
        // For basic implementation, we'll just return basic metadata
        // In reality, you'd use libraries to extract metadata from different document types
        
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension == ".txt")
        {
            // For text files, count approximate pages (assuming 500 words per page)
            content.Position = 0;
            using var reader = new StreamReader(content);
            var text = await reader.ReadToEndAsync();
            var wordCount = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            pageCount = Math.Max(1, wordCount / 500);
        }
        
        return new DocumentMetadata(fileName, contentType, size, pageCount, title, author);
    }

    public async Task<IEnumerable<string>> ExtractTextAsync(Stream content, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var lines = new List<string>();
        
        switch (extension)
        {
            case ".txt":
                content.Position = 0;
                using (var reader = new StreamReader(content))
                {
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lines.Add(line);
                    }
                }
                break;
                
            case ".csv":
                content.Position = 0;
                using (var reader = new StreamReader(content))
                {
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        // For CSV, you might want to parse columns differently
                        lines.Add(line);
                    }
                }
                break;
                
            default:
                // For other document types, you'd use appropriate libraries
                // For now, return empty
                lines.Add($"Text extraction not implemented for {extension} files");
                break;
        }
        
        return lines;
    }

    private static string GetContentTypeFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
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
            ".rtf" => "application/rtf",
            _ => "application/octet-stream"
        };
    }
}