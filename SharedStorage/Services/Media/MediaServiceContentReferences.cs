using System.Collections.Concurrent;
using Azure.Data.Tables;
using Azure;
using SharedStorage.Services.BaseServices;
using Utils.Constants;

namespace SharedStorage.Services.Media;

/// <summary>
/// Manages content references for media services to track file usage and dependencies
/// </summary>
public interface IMediaServiceContentReferences
{
    Task<bool> AddReferenceAsync(string mediaId, string referenceId, string referenceType);
    Task<bool> RemoveReferenceAsync(string mediaId, string referenceId);
    Task<IEnumerable<ContentReference>> GetReferencesAsync(string mediaId);
    Task<bool> HasReferencesAsync(string mediaId);
    Task<bool> CanDeleteMediaAsync(string mediaId);
    Task<int> GetReferenceCountAsync(string mediaId);
    Task<IEnumerable<string>> GetOrphanedMediaAsync();
}

public record ContentReference(
    string ReferenceId,
    string ReferenceType,
    DateTime CreatedAt,
    string? AdditionalData = null
);

public class MediaServiceContentReferences : IMediaServiceContentReferences
{
    private readonly ITableStorageService _tableStorageService;
    private readonly ConcurrentDictionary<string, HashSet<ContentReference>> _cache;
    private readonly string _tableName = "MediaContentReferences";



    public MediaServiceContentReferences(ITableStorageService tableStorageService)
    {
        _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
        _cache = new ConcurrentDictionary<string, HashSet<ContentReference>>();
    }

    public async Task<bool> AddReferenceAsync(string mediaId, string referenceId, string referenceType)
    {
        if (string.IsNullOrEmpty(mediaId) || string.IsNullOrEmpty(referenceId) || string.IsNullOrEmpty(referenceType))
            return false;

        var reference = new ContentReference(referenceId, referenceType, DateTime.UtcNow);
        
        // Update cache
        _cache.AddOrUpdate(mediaId, 
            new HashSet<ContentReference> { reference },
            (key, existing) => { existing.Add(reference); return existing; });

        // Persist to storage
        var entity = new MediaReferenceEntity
        {
            PartitionKey = mediaId,
            RowKey = $"{referenceType}_{referenceId}",
            MediaId = mediaId,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _tableStorageService.UpsertEntityAsync(_tableName, entity);
            return true;
        }
        catch
        {
            // Remove from cache if storage failed
            if (_cache.TryGetValue(mediaId, out var refs))
            {
                refs.Remove(reference);
                if (!refs.Any())
                {
                    _cache.TryRemove(mediaId, out _);
                }
            }
            return false;
        }
    }

    public async Task<bool> RemoveReferenceAsync(string mediaId, string referenceId)
    {
        if (string.IsNullOrEmpty(mediaId) || string.IsNullOrEmpty(referenceId))
            return false;

        // Update cache
        var removedFromCache = false;
        if (_cache.TryGetValue(mediaId, out var refs))
        {
            var refToRemove = refs.FirstOrDefault(r => r.ReferenceId == referenceId);
            if (refToRemove != null)
            {
                refs.Remove(refToRemove);
                removedFromCache = true;
                if (!refs.Any())
                {
                    _cache.TryRemove(mediaId, out _);
                }
            }
        }

        // Remove from storage
        try
        {
            var result = await _tableStorageService.GetEntitiesAsync(_tableName, 
                filter: $"PartitionKey eq '{mediaId}' and ReferenceId eq '{referenceId}'");
            
            var removedFromStorage = false;
            foreach (var entity in result.Entities)
            {
                await _tableStorageService.DeleteEntityAsync(_tableName, entity.PartitionKey, entity.RowKey);
                removedFromStorage = true;
            }
            
            return removedFromCache || removedFromStorage;
        }
        catch
        {
            return removedFromCache;
        }
    }

    public async Task<IEnumerable<ContentReference>> GetReferencesAsync(string mediaId)
    {
        if (string.IsNullOrEmpty(mediaId))
            return Enumerable.Empty<ContentReference>();

        // Check cache first
        if (_cache.TryGetValue(mediaId, out var cachedRefs))
        {
            return cachedRefs;
        }

        // Load from storage
        try
        {
            var result = await _tableStorageService.GetEntitiesAsync(_tableName, 
                filter: $"PartitionKey eq '{mediaId}'");
            
            var references = result.Entities.Select(e => new ContentReference(
                e.GetString("ReferenceId") ?? "",
                e.GetString("ReferenceType") ?? "",
                e.GetDateTime("CreatedAt") ?? DateTime.MinValue,
                e.GetString("AdditionalData")
            )).ToList();

            // Update cache
            _cache.TryAdd(mediaId, new HashSet<ContentReference>(references));
            
            return references;
        }
        catch
        {
            return Enumerable.Empty<ContentReference>();
        }
    }

    public async Task<bool> HasReferencesAsync(string mediaId)
    {
        var references = await GetReferencesAsync(mediaId);
        return references.Any();
    }

    public async Task<bool> CanDeleteMediaAsync(string mediaId)
    {
        return !await HasReferencesAsync(mediaId);
    }

    public async Task<int> GetReferenceCountAsync(string mediaId)
    {
        var references = await GetReferencesAsync(mediaId);
        return references.Count();
    }

    public Task<IEnumerable<string>> GetOrphanedMediaAsync()
    {
        try
        {
            // This would need to be implemented based on your specific storage setup
            // For now, returning empty list as we'd need to compare media storage with references
            return Task.FromResult(Enumerable.Empty<string>());
        }
        catch
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }
    }
}

internal class MediaReferenceEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public string MediaId { get; set; } = string.Empty;
    public string ReferenceId { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? AdditionalData { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}

/// <summary>
/// Content type constants organized by category
/// </summary>
public static class ContentTypes
{
    public static class Images
    {
        public const string Jpeg = "image/jpeg";
        public const string Jpg = "image/jpg";
        public const string Png = "image/png";
        public const string Gif = "image/gif";
        public const string Bmp = "image/bmp";
        public const string Webp = "image/webp";
        public const string Svg = "image/svg+xml";
        public const string Tiff = "image/tiff";
        public const string Ico = "image/x-icon";
    }

    public static class Videos
    {
        public const string Mp4 = "video/mp4";
        public const string Avi = "video/x-msvideo";
        public const string Mov = "video/quicktime";
        public const string Wmv = "video/x-ms-wmv";
        public const string Flv = "video/x-flv";
        public const string Webm = "video/webm";
        public const string Mkv = "video/x-matroska";
        public const string ThreeGp = "video/3gpp";
    }

    public static class Documents
    {
        public const string Pdf = "application/pdf";
        public const string Doc = "application/msword";
        public const string Docx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        public const string Xls = "application/vnd.ms-excel";
        public const string Xlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string Ppt = "application/vnd.ms-powerpoint";
        public const string Pptx = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
        public const string Txt = "text/plain";
        public const string Csv = "text/csv";
        public const string Rtf = "application/rtf";
    }
}

/// <summary>
/// File extension constants organized by category
/// </summary>
public static class FileExtensions
{
    public static class Images
    {
        public const string Jpg = ".jpg";
        public const string Jpeg = ".jpeg";
        public const string Png = ".png";
        public const string Gif = ".gif";
        public const string Bmp = ".bmp";
        public const string WebP = ".webp";
        public const string Svg = ".svg";
        public const string Tiff = ".tiff";
        public const string Tif = ".tif";
        public const string Ico = ".ico";
    }

    public static class Videos
    {
        public const string Mp4 = ".mp4";
        public const string Avi = ".avi";
        public const string Mov = ".mov";
        public const string Wmv = ".wmv";
        public const string Flv = ".flv";
        public const string WebM = ".webm";
        public const string Mkv = ".mkv";
        public const string ThreeGp = ".3gp";
    }

    public static class Documents
    {
        public const string Pdf = ".pdf";
        public const string Doc = ".doc";
        public const string Docx = ".docx";
        public const string Xls = ".xls";
        public const string Xlsx = ".xlsx";
        public const string Ppt = ".ppt";
        public const string Pptx = ".pptx";
        public const string Txt = ".txt";
        public const string Csv = ".csv";
        public const string Rtf = ".rtf";
    }
}

/// <summary>
/// Media categories and their associated content types and file extensions
/// </summary>
public static class MediaCategories
{
    public static readonly HashSet<string> ImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ContentTypes.Images.Jpeg,
        ContentTypes.Images.Jpg,
        ContentTypes.Images.Png,
        ContentTypes.Images.Gif,
        ContentTypes.Images.Bmp,
        ContentTypes.Images.Webp,
        ContentTypes.Images.Svg,
        ContentTypes.Images.Tiff,
        ContentTypes.Images.Ico
    };

    public static readonly HashSet<string> VideoContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ContentTypes.Videos.Mp4,
        ContentTypes.Videos.Avi,
        ContentTypes.Videos.Mov,
        ContentTypes.Videos.Wmv,
        ContentTypes.Videos.Flv,
        ContentTypes.Videos.Webm,
        ContentTypes.Videos.Mkv,
        ContentTypes.Videos.ThreeGp
    };

    public static readonly HashSet<string> DocumentContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ContentTypes.Documents.Pdf,
        ContentTypes.Documents.Doc,
        ContentTypes.Documents.Docx,
        ContentTypes.Documents.Xls,
        ContentTypes.Documents.Xlsx,
        ContentTypes.Documents.Ppt,
        ContentTypes.Documents.Pptx,
        ContentTypes.Documents.Txt,
        ContentTypes.Documents.Csv,
        ContentTypes.Documents.Rtf
    };

    public static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        FileExtensions.Images.Jpg,
        FileExtensions.Images.Jpeg,
        FileExtensions.Images.Png,
        FileExtensions.Images.Gif,
        FileExtensions.Images.Bmp,
        FileExtensions.Images.WebP,
        FileExtensions.Images.Svg,
        FileExtensions.Images.Tiff,
        FileExtensions.Images.Tif,
        FileExtensions.Images.Ico
    };

    public static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        FileExtensions.Videos.Mp4,
        FileExtensions.Videos.Avi,
        FileExtensions.Videos.Mov,
        FileExtensions.Videos.Wmv,
        FileExtensions.Videos.Flv,
        FileExtensions.Videos.WebM,
        FileExtensions.Videos.Mkv,
        FileExtensions.Videos.ThreeGp
    };

    public static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        FileExtensions.Documents.Pdf,
        FileExtensions.Documents.Doc,
        FileExtensions.Documents.Docx,
        FileExtensions.Documents.Xls,
        FileExtensions.Documents.Xlsx,
        FileExtensions.Documents.Ppt,
        FileExtensions.Documents.Pptx,
        FileExtensions.Documents.Txt,
        FileExtensions.Documents.Csv,
        FileExtensions.Documents.Rtf
    };

    /// <summary>
    /// Determines the asset type from a content type
    /// </summary>
    /// <param name="contentType">The content type to analyze</param>
    /// <returns>The corresponding AssetType</returns>
    public static AssetType GetAssetTypeFromContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return AssetType.Media;

        if (ImageContentTypes.Contains(contentType))
            return AssetType.Images;

        if (VideoContentTypes.Contains(contentType))
            return AssetType.Video;

        if (DocumentContentTypes.Contains(contentType))
            return AssetType.Data;

        return AssetType.Media;
    }

    /// <summary>
    /// Determines the asset type from a file name
    /// </summary>
    /// <param name="fileName">The file name to analyze</param>
    /// <returns>The corresponding AssetType</returns>
    public static AssetType GetAssetTypeFromFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return AssetType.Media;

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
            return AssetType.Media;

        if (ImageExtensions.Contains(extension))
            return AssetType.Images;

        if (VideoExtensions.Contains(extension))
            return AssetType.Video;

        if (DocumentExtensions.Contains(extension))
            return AssetType.Data;

        return AssetType.Media;
    }

    /// <summary>
    /// Checks if a file is a media file based on its name and content type
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <param name="contentType">The content type</param>
    /// <returns>True if the file is a media file</returns>
    public static bool IsMediaFile(string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName);
        
        return ImageContentTypes.Contains(contentType) ||
               VideoContentTypes.Contains(contentType) ||
               ImageExtensions.Contains(extension) ||
               VideoExtensions.Contains(extension);
    }
}